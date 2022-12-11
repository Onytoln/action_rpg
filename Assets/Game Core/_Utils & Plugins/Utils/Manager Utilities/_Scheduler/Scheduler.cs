using MEC;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class Scheduler : MonoBehaviour {
    #region Action queues variables

    [SerializeField, Header("Action Queues")] private ActionQueue[] actionQueues;

    private static ActionQueue[] runtimeActionQueues;

    #endregion

    #region SphereCast Commands variables
    
    [SerializeField, Header("Sphere Cast Commands")] int sphereCastsRefreshRate = 60;
    [SerializeField] int sphereCastsWaitFramesBetween = 8;

    static float currentSphereCastsRefreshRate;
    static int currentSphereCastsWaitFramesBetween;
    static float nextSphereCastsExecuteTime = -1f;

    static CoroutineHandle executeSphereCastsCoroutineHandle;

    static readonly List<SphereCastCommandData> sphereCastCommandData = new List<SphereCastCommandData>();
    #endregion

    #region Execute on Main Thread variables

    private static List<Action> executeOnMainThread = new List<Action>();
    private static volatile bool hasExecutables;

    private static List<Action> currentlyExecuting = new List<Action>();

    #endregion

    #region Execute Delayed variables

    private static readonly List<ExecuteActionDelayedByFrames> executionsDelayedByFrames = new List<ExecuteActionDelayedByFrames>(); 

    #endregion

    static float deltaTime;

    void Start() {
        if (actionQueues == null) {
            actionQueues = new ActionQueue[2];

            actionQueues[0] = new ActionQueue(ActionQueueType.None);
            actionQueues[1] = new ActionQueue(ActionQueueType.NavMeshAgentSetDestination);
        }

        runtimeActionQueues = actionQueues;

        IFpsProvider fpsProvider = FPSCounter.Instance;
        for (int i = 0; i < runtimeActionQueues.Length; i++) {
            runtimeActionQueues[i].FpsProvider = fpsProvider;
            runtimeActionQueues[i].Initialize();
        }

        currentSphereCastsRefreshRate = 1f / sphereCastsRefreshRate;
        currentSphereCastsWaitFramesBetween = sphereCastsWaitFramesBetween;
    }


    void Update() {
        deltaTime = Time.time;

        try {
            TryExecuteDelayedActions();

            ExecuteActionsOnMainThread();

            for (int i = 0; i < actionQueues.Length; i++) {
                runtimeActionQueues[i].Execute();
            }

            ExecuteSphereCasts();
           
        } catch(Exception exc) {
            Debug.LogError($"An error occured when updating Scheduler. Message: {exc.Message}");
        }
    }

    #region Action Queues

    public static bool EnqueueAction(Action actionToExecute, ActionQueueType actionQueueType) {
        for (int i = 0; i < runtimeActionQueues.Length; i++) {
            if (runtimeActionQueues[i].ActionQueueType == actionQueueType) {
                return runtimeActionQueues[i].TryExecuteOrEnqueue(actionToExecute);
            }
        }

        return false;
    }

    #endregion

    #region SphereCast Commands

    public static void ScheduleSphereCast(Func<Vector3> getOrigin, float radius, Func<Vector3> getDirection, float distance, LayerMask layerMask, Action<RaycastHit> onCompleteAction) {
        if (getOrigin == null) {
            Debug.LogError("getOrigin is null for ScheduleSphereCast, this is not allowed!");
            return;
        }

        if (getDirection == null) {
            Debug.LogError("getDirection is null for ScheduleSphereCast, this is not allowed!");
            return;
        }

        if (onCompleteAction == null) {
            Debug.LogError("onCompleteAction is null for ScheduleSphereCast, this is not allowed!");
            return;
        }

        sphereCastCommandData.Add(new SphereCastCommandData(
                getOrigin,
                radius,
                getDirection,
                distance,
                layerMask,
                onCompleteAction));
    }

    private static void ExecuteSphereCasts() {
        if (deltaTime < nextSphereCastsExecuteTime) return;
        nextSphereCastsExecuteTime = deltaTime + (currentSphereCastsRefreshRate * currentSphereCastsWaitFramesBetween);

        if (sphereCastCommandData.Count == 0) return;

        executeSphereCastsCoroutineHandle = Timing.RunCoroutine(ExecuteSphereCastsCoroutine());
    }

    private static IEnumerator<float> ExecuteSphereCastsCoroutine() {
        SphereCastCommandData[] sphereCastCommandDataTemp = sphereCastCommandData.ToArray();
        sphereCastCommandData.Clear();

        NativeArray<SpherecastCommand> spherecastCommands = new NativeArray<SpherecastCommand>(sphereCastCommandDataTemp.Length, Allocator.TempJob);
        NativeArray<RaycastHit> sphereCastCommandsHits = new NativeArray<RaycastHit>(sphereCastCommandDataTemp.Length, Allocator.TempJob);

        try {
            for (int i = 0; i < sphereCastCommandDataTemp.Length; i++) {
                //Debug.DrawRay(sphereCastCommandDataTemp[i].getOrigin.Invoke(), sphereCastCommandDataTemp[i].getDirection() * sphereCastCommandDataTemp[i].distance, Color.blue, 3f);
                spherecastCommands[i] = new SpherecastCommand(
                        sphereCastCommandDataTemp[i].getOrigin.Invoke(),
                        sphereCastCommandDataTemp[i].radius,
                        sphereCastCommandDataTemp[i].getDirection.Invoke(),
                        sphereCastCommandDataTemp[i].distance,
                        sphereCastCommandDataTemp[i].layerMask
                    );
            }
        } catch (Exception exc) {
            Debug.LogError($"An error occured while setting up sphere cast commands. Message: {exc.Message}");
            Dispose();
        } 

        JobHandle sphereCastsJobs = SpherecastCommand.ScheduleBatch(spherecastCommands, sphereCastCommandsHits, Mathf.Clamp(spherecastCommands.Length / 5, 1, int.MaxValue));

        yield return Timing.WaitForOneFrame;

        sphereCastsJobs.Complete();

        try {
            for (int i = 0; i < sphereCastCommandDataTemp.Length; i++) {
                sphereCastCommandDataTemp[i].onCompleteAction.SafeInvoke(sphereCastCommandsHits[i]);
            }
        } finally {
            Dispose();
        }

        void Dispose() {
            spherecastCommands.Dispose();
            sphereCastCommandsHits.Dispose();
        }
   }

    private class SphereCastCommandData {
        public readonly Func<Vector3> getOrigin;
        public readonly float radius;
        public readonly Func<Vector3> getDirection;
        public readonly float distance;
        public readonly LayerMask layerMask;
        public readonly Action<RaycastHit> onCompleteAction;

        public SphereCastCommandData(Func<Vector3> getOrigin, float radius, Func<Vector3> getDirection, float distance, LayerMask layerMask, Action<RaycastHit> onCompleteAction) {
            this.getOrigin = getOrigin;
            this.radius = radius;
            this.getDirection = getDirection;
            this.distance = distance;
            this.layerMask = layerMask;
            this.onCompleteAction = onCompleteAction;
        }
    }

    #endregion

    #region Execute on Main Thread

    private static void ExecuteActionsOnMainThread() {
        if (!hasExecutables) return;

        lock (executeOnMainThread) {
            var tmp = currentlyExecuting;
            currentlyExecuting = executeOnMainThread;
            executeOnMainThread = tmp;
            hasExecutables = false;
        }

        for (int i = 0; i < currentlyExecuting.Count; i++) {
            currentlyExecuting[i].SafeInvoke();
        }

        currentlyExecuting.Clear();
    }


    public static void ExecuteOnMainThread(Action action) {
        lock (executeOnMainThread) {
            executeOnMainThread.Add(action);
            hasExecutables = true;
        }
    }

    #endregion

    #region Execute Delayed

    private static void TryExecuteDelayedActions() {
        for (int i = executionsDelayedByFrames.Count; i-- > 0;) {
            if (executionsDelayedByFrames[i].TryExecute()) {
                executionsDelayedByFrames.RemoveAt(i);
            }
        }
    }

    public static void ExecuteDelayed(Action action, int delayByFrames) {
        executionsDelayedByFrames.Add(new ExecuteActionDelayedByFrames(action, delayByFrames));
    }

    public class ExecuteActionDelayedByFrames {
        private readonly Action toExecute;
        private readonly int delayFrames;
        private readonly int initFrame;
        private int framesElapsed = 0;

        public ExecuteActionDelayedByFrames(Action action, int delayFrames) {
            this.toExecute = action;
            this.delayFrames = delayFrames;
            initFrame = Time.frameCount;
        }

        public bool TryExecute() {
            if (Time.frameCount == initFrame) return false;

            framesElapsed++;
            if (framesElapsed < delayFrames) return false;

            toExecute.SafeInvoke();
            return true;
        }
    }

    #endregion

}




