using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Scenes {
    CoreScene = 0,
    UserInterface = 1,
    Player_Mage = 2,
    TestScene = 3,
    SecondScene = 4
}

public enum ExecuteAmount { Once, Always }

public enum GameStage { AnyStage = 0, IntroLoadingScreen, GameTime }

public enum SceneLoadStage {
    PreSceneLoad, UnloadingScenes, LoadingScenes, PostSceneLoad, LatePostSceneLoad, UIPhase, FinishingPhase
}

public class GameSceneManager : MonoBehaviour, ILoadable {

    public bool IsLoaded { get; private set; }
    public event Action<ILoadable> OnLoad;

    #region Singleton
    public static GameSceneManager Instance { get; private set; }

    private void Awake() {
        if (Instance == null) Instance = this;
        IsLoaded = true;
        OnLoad?.Invoke(this);
    }

    #endregion

    #region Phases

    public static SceneLoadPhase PreSceneLoadPhase { get; private set; } = new SceneLoadPhase();
    public static SceneLoadPhase PostSceneLoadPhase { get; private set; } = new SceneLoadPhase();
    public static SceneLoadPhase LatePostSceneLoadPhase { get; private set; } = new SceneLoadPhase();
    public static SceneLoadPhase UIPhase { get; private set; } = new SceneLoadPhase();
    public static SceneLoadPhase FinishingPhase { get; private set; } = new SceneLoadPhase();

    #endregion

    #region Loading Info

    public static event Action<bool> OnLoadStarted;
    public static bool IsLoading { get; private set; }

    public static event Action<float> OnCurrentProgressChanged;
    public static float CurrentProgress { get; private set; }
    private static float currentAsyncOperationsProggress;
    private static float currentPhaseProgress;
    private static int currentTotalContributors;
    private static int currentPhasesFinished;

    private static readonly int phasesCount = Enum.GetValues(typeof(SceneLoadStage)).Length;

    public event Action<SceneLoadStage> OnLoadPhaseChanged;
    public SceneLoadPhase CurrentLoadingPhase { get; private set; }

    private List<SceneLoadExecutable<Action>> currentlyExecutingSyncActions;
    private List<SceneLoadExecutable<Func<Task>>> currentlyExecutingTasks;
    private List<SceneLoadExecutable<Func<IEnumerator<float>>>> currentlyExecutingCoroutines;

    private List<AsyncOperation> currentUnloadingScenes;
    private List<AsyncOperation> currentLoadingScenes;

    private CoroutineHandle progressReporterHandle;
    #endregion

    #region Stage and Scene Info

    public GameStage GameStage { get; private set; }

    public Scenes CurrentActiveScene => (Scenes)UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
    public UnityEngine.SceneManagement.Scene CurrentActiveUnityScene => UnityEngine.SceneManagement.SceneManager.GetActiveScene();

    public static event Action<GameStage> OnGameStageChanged;

    #endregion

    #region Editor Debug Info

#if UNITY_EDITOR
    private Scenes currentScene;
#endif

    #endregion

#if UNITY_EDITOR
    [SerializeField] private bool skipIntroScreen;
    [SerializeField] private Scenes loadAreaSceneInEditorOnRun = Scenes.TestScene;
    [SerializeField] private Scenes loadPlayerInEditorOnRun = Scenes.Player_Mage;
#endif


    void OnValidate() {
#if UNITY_EDITOR
        if (!IsValidNonCoreScene(loadAreaSceneInEditorOnRun)) {
            Debug.Log("Load scene in editor is not valid, setting to TestScene.");
            loadAreaSceneInEditorOnRun = Scenes.TestScene;
        }
#endif
    }

    void Start() {
        IntroScreenLoad();
    }

    #region Intro Scene load

    public void IntroScreenLoad() {
#if UNITY_EDITOR
        if (skipIntroScreen) {
            GameTimeLoad(loadPlayerInEditorOnRun, loadAreaSceneInEditorOnRun);
        } else {
            LoadIntro();
        }
#else
        LoadIntro();
#endif

        void LoadIntro() {
            GameStage = GameStage.IntroLoadingScreen;

            _ = Utils.WaitUntilDoneThenExecute((CoroutineHandle)IntroScreenLoadInternal(), () => {
                OnGameStageChanged?.Invoke(GameStage);
            });
        }
    }

    private CoroutineHandle? IntroScreenLoadInternal() {
        return LoadScenesInternal(new[] { Scenes.UserInterface }, Scenes.UserInterface, GetAllLoadedScenesExcept(new[] { Scenes.CoreScene, Scenes.UserInterface }));
    }

    #endregion

    #region GameTime Scene Load

    public void GameTimeLoad(Scenes playerScene, Scenes areaScene) {
        GameStage = GameStage.GameTime;

        _ = Utils.WaitUntilDoneThenExecute((CoroutineHandle)GameTimeLoadInternal(playerScene, areaScene), () => {
            OnGameStageChanged?.Invoke(GameStage);
        });
    }

    private CoroutineHandle? GameTimeLoadInternal(Scenes playerScene, Scenes areaScene) {
        if (!IsPlayerScene(playerScene)) {
            Debug.LogError("Invalid player scene to load!");
            return null;
        }

        if (!IsValidNonCoreScene(areaScene)) {
            Debug.LogError("Invalid scene to load!");
            return null;
        }

        return LoadScenesInternal(new Scenes[] { Scenes.UserInterface, playerScene, areaScene }, areaScene, new Scenes[] { CurrentActiveScene });
    }

    public void LoadSceneGameTime(Scenes scene) {
        _ = LoadSceneGameTimeInternal(scene);
    }

    private CoroutineHandle? LoadSceneGameTimeInternal(Scenes scene) {
        if (!IsValidNonCoreScene(scene)) {
            Debug.LogError("Invalid scene to load!");
            return null;
        }

        return LoadScenesInternal(new Scenes[] { scene }, scene, new Scenes[] { CurrentActiveScene });
    }

    #endregion

    private CoroutineHandle? LoadScenesInternal(Scenes[] scenes, Scenes activateScene, Scenes[] scenesToUnload) {
        scenes ??= new Scenes[0];
        scenesToUnload ??= new Scenes[0];

        var loadHandle = Timing.RunCoroutine(LoadScenesCoroutine(scenes, activateScene, scenesToUnload));
        progressReporterHandle = Timing.RunCoroutine(ProgressReporterCoroutine());
        return loadHandle;
    }

    private IEnumerator<float> LoadScenesCoroutine(Scenes[] scenes, Scenes activateScene, Scenes[] scenesToUnload) {
        IsLoading = true;
        OnLoadStarted?.Invoke(IsLoading);

        currentPhasesFinished = 0;

        //pre-scene load
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(ExecutePhaseHelper(ExecutePhaseCoroutine(PreSceneLoadPhase), SceneLoadStage.PreSceneLoad, PreSceneLoadPhase)));

        //unloading scenes
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(ExecutePhaseHelper(UnloadScenesCoroutine(scenesToUnload), SceneLoadStage.UnloadingScenes, null)));

        //loading scenes
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(ExecutePhaseHelper(LoadScenesCoroutine(scenes, activateScene), SceneLoadStage.LoadingScenes, null)));

        //post-scene load
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(ExecutePhaseHelper(ExecutePhaseCoroutine(PostSceneLoadPhase), SceneLoadStage.PostSceneLoad, PostSceneLoadPhase)));

        //late post-scene load
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(ExecutePhaseHelper(ExecutePhaseCoroutine(LatePostSceneLoadPhase), SceneLoadStage.LatePostSceneLoad, LatePostSceneLoadPhase)));

        //ui phase
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(ExecutePhaseHelper(ExecutePhaseCoroutine(UIPhase), SceneLoadStage.UIPhase, UIPhase)));

        //finishing phase
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(ExecutePhaseHelper(ExecutePhaseCoroutine(FinishingPhase), SceneLoadStage.FinishingPhase, FinishingPhase)));

        FinishLoad();
    }

    private IEnumerator<float> ExecutePhaseHelper(IEnumerator<float> runPhaseExecution, SceneLoadStage sceneLoadStage, SceneLoadPhase sceneLoadPhase) {
        SetPhaseInfo(sceneLoadStage, sceneLoadPhase);
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(runPhaseExecution));
        currentPhasesFinished++;
    }

    private IEnumerator<float> UnloadScenesCoroutine(Scenes[] scenesToUnload) {
        currentUnloadingScenes = new List<AsyncOperation>();

        for (int i = 0; i < scenesToUnload.Length; i++) {
            if (CanUnloadScene(scenesToUnload[i])) {
                currentUnloadingScenes.Add(SceneManager.UnloadSceneAsync((int)scenesToUnload[i]));
            }
        }

        yield return Timing.WaitUntilDone(new AwaitAsyncOperations(currentUnloadingScenes));
        currentUnloadingScenes = null;
    }

    private IEnumerator<float> LoadScenesCoroutine(Scenes[] scenes, Scenes activateScene) {
        currentLoadingScenes = new List<AsyncOperation>();

        for (int i = 0; i < scenes.Length; i++) {
            if (!IsSceneLoaded(scenes[i])) {
                currentLoadingScenes.Add(SceneManager.LoadSceneAsync((int)scenes[i], LoadSceneMode.Additive));
            }
        }
        yield return Timing.WaitUntilDone(new AwaitAsyncOperations(currentLoadingScenes));

        SceneManager.SetActiveScene(UnitySceneByMyScene(activateScene));

        yield return Timing.WaitForOneFrame;
        currentLoadingScenes = null;
    }


    private IEnumerator<float> ExecutePhaseCoroutine(SceneLoadPhase phase) {
        if (phase == null) {
            Debug.LogError("Scene loading phase is null!");
            yield break;
        }

        phase.CurrentGameStage = GameStage;

        currentlyExecutingSyncActions = phase.ExecuteSyncList;
        currentlyExecutingTasks = phase.ExecuteTasksConcurrentlyList;
        currentlyExecutingCoroutines = phase.ExecuteCoroutinesConcurrentlyList;
        
        for (int i = 0; i < currentlyExecutingSyncActions.Count; i++) {
            currentlyExecutingSyncActions[i].Executable?.SafeInvoke();
        }

        Task[] tasks = new Task[currentlyExecutingTasks.Count];
        for (int i = 0; i < currentlyExecutingTasks.Count; i++) {
            tasks[i] = currentlyExecutingTasks[i].Executable?.SafeInvoke();
        }
        yield return Timing.WaitUntilDone(new AwaitTasks(tasks));

        IEnumerator<float>[] coroutines = new IEnumerator<float>[currentlyExecutingCoroutines.Count];
        for (int i = 0; i < currentlyExecutingCoroutines.Count; i++) {
            coroutines[i] = currentlyExecutingCoroutines[i].Executable?.SafeInvoke();
        }
        yield return Timing.WaitUntilDone(new AwaitCoroutines(coroutines));
    }

    private void FinishLoad() {
        Timing.KillCoroutines(progressReporterHandle);
        CurrentProgress = 1f;
        OnCurrentProgressChanged?.Invoke(CurrentProgress);

        PreSceneLoadPhase.LoadFinished();
        PostSceneLoadPhase.LoadFinished();
        LatePostSceneLoadPhase.LoadFinished();
        UIPhase.LoadFinished();
        FinishingPhase.LoadFinished();

#if UNITY_EDITOR
        currentScene = CurrentActiveScene;
#endif

        IsLoading = false;
        OnLoadStarted?.Invoke(IsLoading);
    }

    private IEnumerator<float> ProgressReporterCoroutine() {
        while (true) {
            currentAsyncOperationsProggress = 0f;
            currentPhaseProgress = 0f;
            currentTotalContributors = 0;

            if (CurrentLoadingPhase != null) {
                if (currentlyExecutingSyncActions != null) {
                    AddProgress(GetProgressFromSceneLoadExecutables(currentlyExecutingSyncActions));
                }

                if (currentlyExecutingTasks != null) {
                    AddProgress(GetProgressFromSceneLoadExecutables(currentlyExecutingTasks));
                }

                if (currentlyExecutingCoroutines != null) {
                    AddProgress(GetProgressFromSceneLoadExecutables(currentlyExecutingCoroutines));
                }

                currentPhaseProgress = Validate(Mathf.Clamp01(currentPhaseProgress /= currentTotalContributors));
            }

            if (currentUnloadingScenes != null) {
                currentAsyncOperationsProggress += GetProgressFromAsyncOperations(currentUnloadingScenes);
            }

            if (currentLoadingScenes != null) {
                currentAsyncOperationsProggress += GetProgressFromAsyncOperations(currentLoadingScenes);
            }

            CurrentProgress = Validate(Mathf.Clamp01((currentAsyncOperationsProggress + currentPhaseProgress + currentPhasesFinished) / phasesCount));
            OnCurrentProgressChanged?.Invoke(CurrentProgress);

            yield return Timing.WaitForOneFrame;
        }

        void AddProgress((float progress, bool contributed) result) {
            currentPhaseProgress += result.progress;

            if (result.contributed) {
                currentTotalContributors++;
            }
        }

        float Validate(float num) {
            return float.IsNaN(num) ? 0f : num;
        }
    }

    #region Helpers

    private void SetPhaseInfo(SceneLoadStage currentlyLoadingPhaseEnum, SceneLoadPhase currentlyLoadingPhase) {
        CurrentLoadingPhase = currentlyLoadingPhase;
        OnLoadPhaseChanged?.Invoke(currentlyLoadingPhaseEnum);
    }

    public static bool IsSceneLoaded(Scenes scene) {
        return UnitySceneByMyScene(scene).isLoaded;
    }

    public static bool CanUnloadScene(Scenes sceneToUnload) {
        if (IsUnloadableScene(sceneToUnload) || !IsSceneLoaded(sceneToUnload)) {
            return false;
        }

        return true;
    }

    public static bool IsPlayerScene(Scenes scene) => scene switch {
        Scenes.Player_Mage => true,
        _ => false,
    };

    public static bool IsValidNonCoreScene(Scenes scene) => scene switch {
        Scenes.TestScene => true,
        Scenes.SecondScene => true,
        _ => false
    };

    public static bool IsUnloadableScene(Scenes scene) {
        return (int)scene <= 0;
    }

    public static UnityEngine.SceneManagement.Scene UnitySceneByMyScene(Scenes scene) {
        return UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex((int)scene);
    }

    public static UnityEngine.SceneManagement.Scene UnitySceneByMyScene(int buildIndex) {
        return UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(buildIndex);
    }

    public static Scenes UnitySceneToMyScene(UnityEngine.SceneManagement.Scene unityScene) {
        return (Scenes)unityScene.buildIndex;
    }

    public static Scenes[] GetAllLoadedScenesExcept(Scenes[] except) {
        List<Scenes> loadedScenesExcept = new List<Scenes>();

        Scene[] exceptUnityScenes = new Scene[except.Length];
        for (int i = 0; i < except.Length; i++) {
            exceptUnityScenes[i] = UnitySceneByMyScene(except[i]);
        }

        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++) {
            Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);

            if (!exceptUnityScenes.Contains(scene))
                loadedScenesExcept.Add(UnitySceneToMyScene(scene));
        }

        return loadedScenesExcept.ToArray();
    }

    private static (float progress, bool contributed) GetProgressFromSceneLoadExecutables<T>(List<SceneLoadExecutable<T>> sceneLoadExecutables) {
        float totalProgress = 0f;
        int totalContributors = 0;

        for (int i = 0; i < sceneLoadExecutables.Count; i++) {
            if (sceneLoadExecutables[i].progress != null && sceneLoadExecutables[i].progress.ReportsProgress) {
                totalContributors++;
                totalProgress += sceneLoadExecutables[i].progress.Progress;
            }
        }

        float result = Mathf.Clamp01(totalProgress / totalContributors);
        return (float.IsNaN(result) ? 0f : result, totalContributors > 0);
    }

    private static float GetProgressFromAsyncOperations(List<AsyncOperation> asyncOperations) {
        float totalProggress = 0f;

        for (int i = 0; i < asyncOperations.Count; i++) {
            totalProggress += asyncOperations[i].progress;
        }

        float result = Mathf.Clamp01(totalProggress / asyncOperations.Count);
        return float.IsNaN(result) ? 0f : result;
    }

    #endregion
}


