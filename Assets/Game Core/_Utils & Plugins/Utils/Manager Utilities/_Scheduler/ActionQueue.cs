using System;
using System.Collections.Generic;
using UnityEngine;

public enum ActionQueueType { None = 0, NavMeshAgentSetDestination }

[System.Serializable]
public class ActionQueue {
    [field: SerializeField] public ActionQueueType ActionQueueType { get; private set; }

    [field: SerializeField] public FpsToMaxExecuteCount[] FpsToMaxExecuteCount { get; private set; }

    [SerializeField] private uint executesPerSecond = 60;
    private float executesPerSecondRealTime;
    private float nextExecuteTime = 0f;

    [SerializeField] private uint waitExecutesIterationsBeforeNextExecute = 1;

    private Queue<Action> queue;

    private bool initialized = false;

    private int maxExecutesThisInterval = 0;

    public IFpsProvider FpsProvider { get; set; }

    public ActionQueue(ActionQueueType actionQueueType) {
        ActionQueueType = actionQueueType;

        executesPerSecond = executesPerSecond == 0 ? 60 : executesPerSecond;
        waitExecutesIterationsBeforeNextExecute = waitExecutesIterationsBeforeNextExecute == 0 ? 1 : waitExecutesIterationsBeforeNextExecute;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="actionToExecute"></param>
    /// <returns>True if was executed or enqueued.</returns>
    public bool TryExecuteOrEnqueue(Action actionToExecute) {
        if (actionToExecute == null) return false;

        if (CanExecute()) {
            actionToExecute.SafeInvoke();
            maxExecutesThisInterval--;
            return true;
        }

        queue.Enqueue(actionToExecute);
        return true;
    }

    public void Execute() {
        while (CanExecute()) {
            if (queue.Count > 0) {
                queue.Dequeue().SafeInvoke();
                maxExecutesThisInterval--;
            } else {
                break;
            }
        }
    }

    private bool CanExecute() {
        maxExecutesThisInterval = MaxExecutesThisFrame();
        if (maxExecutesThisInterval > 0)
            return true;

        return false;
    }

    private int MaxExecutesThisFrame() {
        float currentTime = Time.time;
        if (currentTime < nextExecuteTime) return maxExecutesThisInterval;
        nextExecuteTime = currentTime + (executesPerSecondRealTime * waitExecutesIterationsBeforeNextExecute);

        float fps = FpsProvider.CurrentFps;

        for (int i = FpsToMaxExecuteCount.Length - 1; i >= 0; i--) {
            if (fps >= FpsToMaxExecuteCount[i].Fps) return FpsToMaxExecuteCount[i].MaxExecuteCount;
        }

        return FpsToMaxExecuteCount[0].MaxExecuteCount;
    }

    public void Initialize() {
        if (initialized) return;
        initialized = true;

        queue ??= new Queue<Action>();

        if (FpsToMaxExecuteCount == null || FpsToMaxExecuteCount.Length == 0) {
            FpsToMaxExecuteCount = new FpsToMaxExecuteCount[2];
            FpsToMaxExecuteCount[0] = new FpsToMaxExecuteCount(30, 2);
            FpsToMaxExecuteCount[1] = new FpsToMaxExecuteCount(60, 4);
            FpsToMaxExecuteCount[1] = new FpsToMaxExecuteCount(100, 6);
        }

        float prev = float.NegativeInfinity;
        for (int i = 0; i < FpsToMaxExecuteCount.Length; i++) {
            if (FpsToMaxExecuteCount[i].Fps <= prev) {
                throw new Exception("Action queue has incorrect fps to execute count values. Must be ordered from lowest to highest. Values must also be unique for Fps.");
            }

            prev = FpsToMaxExecuteCount[i].Fps;
        }

        executesPerSecondRealTime = 1f / executesPerSecond;
    }
}


[System.Serializable]
public struct FpsToMaxExecuteCount {
    [field: SerializeField] public float Fps { get; private set; }
    [field: SerializeField] public int MaxExecuteCount { get; private set; }

    public FpsToMaxExecuteCount(float fps, int maxExecuteCount) {
        Fps = fps;
        MaxExecuteCount = maxExecuteCount;
    }
}
