using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UnityEngine;

public class SceneLoadPhase {
    private readonly List<SceneLoadExecutable<Action>> executeSync = new List<SceneLoadExecutable<Action>>();
    public List<SceneLoadExecutable<Action>> ExecuteSyncList => GetExecutables(executeSync);

    private readonly List<SceneLoadExecutable<Func<Task>>> executeTasksConcurrently = new List<SceneLoadExecutable<Func<Task>>>();
    public List<SceneLoadExecutable<Func<Task>>> ExecuteTasksConcurrentlyList => GetExecutables(executeTasksConcurrently);

    private readonly List<SceneLoadExecutable<Func<IEnumerator<float>>>> executeCoroutinesConcurrently = new List<SceneLoadExecutable<Func<IEnumerator<float>>>>();
    public List<SceneLoadExecutable<Func<IEnumerator<float>>>> ExecuteCoroutinesConcurrentlyList => GetExecutables(executeCoroutinesConcurrently);

    public GameStage CurrentGameStage { get; set; }

    public void ExecuteSync(Action action, IProgress progress, ExecuteAmount executeAmount, GameStage executeOnGameStage = GameStage.GameTime) {
        if (action == null) {
            Debug.LogError("Cannot send null action to ExecuteSync!");
            return;
        }

        executeSync.Add(new SceneLoadExecutable<Action>(action, progress, executeAmount, executeOnGameStage));
    }

    public void RemoveExecuteSync(Action action) =>
        executeSync.Remove(executeSync.FirstOrDefault(x => x.Executable == action));

    public void ExecuteTaskConcurrently(Func<Task> task, IProgress progress, ExecuteAmount executeAmount, GameStage executeOnGameStage = GameStage.GameTime) {
        if (task == null) {
            Debug.LogError("Cannot send null func to ExecuteTaskAtOnce!");
            return;
        }

        executeTasksConcurrently.Add(new SceneLoadExecutable<Func<Task>>(task, progress, executeAmount, executeOnGameStage));
    }

    public void RemoveExecuteTask(Func<Task> task) =>
        executeTasksConcurrently.Remove(executeTasksConcurrently.FirstOrDefault(x => x.Executable == task));
    

    public void ExecuteCoroutineConcurrently(Func<IEnumerator<float>> coroutine, IProgress progress, ExecuteAmount executeAmount, GameStage executeOnGameStage = GameStage.GameTime) {
        if (coroutine == null) {
            Debug.LogError("Cannot send null coroutine to ExecuteCoroutineAtOnce!");
            return;
        }

        executeCoroutinesConcurrently.Add(new SceneLoadExecutable<Func<IEnumerator<float>>>(coroutine, progress, executeAmount, executeOnGameStage));
    }

    public void RemoveExecuteCoroutine(Func<IEnumerator<float>> coroutine) =>
        executeCoroutinesConcurrently.Remove(executeCoroutinesConcurrently.FirstOrDefault(x => x.Executable == coroutine));

    private List<SceneLoadExecutable<T>> GetExecutables<T>(List<SceneLoadExecutable<T>> sceneLoadExecutable) {
        List<SceneLoadExecutable<T>> result = new List<SceneLoadExecutable<T>>();

        for (int i = 0; i < sceneLoadExecutable.Count; i++) {
            if (CanExecuteThisStage(sceneLoadExecutable[i].executeOnGameStage)) {
                result.Add(sceneLoadExecutable[i]);
            }
        }

        return result;
    }

    private bool CanExecuteThisStage(GameStage executeOnGameStage) {
        return executeOnGameStage == GameStage.AnyStage || executeOnGameStage == CurrentGameStage;
    }

    internal void LoadFinished() {
        RemoveOnConditionTrue(executeSync);
        RemoveOnConditionTrue(executeTasksConcurrently);
        RemoveOnConditionTrue(executeCoroutinesConcurrently);
    }

    private void RemoveOnConditionTrue<T>(List<SceneLoadExecutable<T>> executables) {
        for (int i = executables.Count; i-- > 0;) {
            executables[i].Executed();
            if (CanExecuteThisStage(executables[i].executeOnGameStage) && executables[i].ExecutedRequiredTimes) {
                executables.RemoveAt(i);
            }
        }
    }
}

public class SceneLoadExecutable<T> {
    private readonly T executable; 
    public T Executable {
        get => executable;
    }

    internal readonly IProgress progress;
    internal readonly ExecuteAmount executeAmount;
    internal readonly GameStage executeOnGameStage;

    int executedAmount = 0;

    public bool ExecutedRequiredTimes {
        get {
            if(executeAmount == ExecuteAmount.Once && executedAmount > 0) {
                return true;
            }

            return false;
        }
    }

    public SceneLoadExecutable(T executable, IProgress progress, ExecuteAmount executeAmount, GameStage executeOnGameStage) {
        this.executable = executable;
        this.progress = progress;
        this.executeAmount = executeAmount;
        this.executeOnGameStage = executeOnGameStage;
    }

    public void Executed() {
        executedAmount++;
    }
}