using MEC;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class GameStateManager : MonoBehaviour, ISaveLoadPlayerContext {
    public static GameStateManager Instance { get; private set; }

    public string SaveFileNameSuffix => $"{GameSceneManager.Instance.CurrentActiveScene}_{DateTime.Now.ToDirectoryFriendlyDateTime()}";

    public static string AutoSaveFileName => $"auto_save_{Instance.SaveFileNameSuffix}";

    private Func<Task> savePlayerDataOnLoad;
    private Func<Task> loadPlayerDataOnLoad;

    private void Awake() {
        if (Instance == null) Instance = this;

        GameSceneManager.PostSceneLoadPhase.ExecuteTaskConcurrently(() => {
            CoroutineHandle waitHandle = SaveLoadManager.LoadAllSavesIntoCache();

            return Task.Run(async () => {
                await Utils.AwaitCoroutine(waitHandle);
            });
        }, SaveLoadManager.Progress, ExecuteAmount.Always, GameStage.AnyStage);

        savePlayerDataOnLoad = () => {
            CoroutineHandle waitHandle = SaveLoadManager.SavePlayer(AutoSaveFileName, PlayerCache.CurrentPlayer);

            return Task.Run(async () => {
                await Utils.AwaitCoroutine(waitHandle);
            });
        };

        GameSceneManager.OnGameStageChanged += (gameStage) => {
            if (gameStage == GameStage.GameTime) {
                GameStageGameTime();
            } else if (gameStage == GameStage.IntroLoadingScreen) {
                GameStageIntroScreen();
            }
        };
    }

    /*void Update() {
        if (Input.GetKeyDown(KeyCode.S)) {
            SaveLoadManager.SavePlayer("hihi", PlayerCache.CurrentPlayer, SaveLoadManager.SaveType.CreateNew);
            SaveLoadManager.SaveGlobals();
        }

        if (Input.GetKeyDown(KeyCode.L)) {
            Utils.WaitUntilDoneThenExecute(SaveLoadManager.LoadAllSavesIntoCache(),
                () => {
                    SaveLoadManager.LoadPlayerData(PlayerCache.CurrentPlayer);
                    SaveLoadManager.LoadGlobalData();
                });
        }
    }*/

    private void GameStageGameTime() {
        GameSceneManager.PostSceneLoadPhase.ExecuteTaskConcurrently(savePlayerDataOnLoad, SaveLoadManager.Progress, ExecuteAmount.Always, GameStage.AnyStage);
    }

    private void GameStageIntroScreen() {
        GameSceneManager.PostSceneLoadPhase.RemoveExecuteTask(savePlayerDataOnLoad);
        GameSceneManager.FinishingPhase.RemoveExecuteTask(loadPlayerDataOnLoad);
    }

    public void SetPlayerSaveForLoading(IPlayerCore playerCore) {
        if (playerCore == null) {
            Debug.LogError("Cannot load player save of null IPlayerCore");
            return;
        }

        loadPlayerDataOnLoad = () => {
            CoroutineHandle waitHandle = SaveLoadManager.LoadPlayerData(playerCore);

            return Task.Run(async () => {
                await Utils.AwaitCoroutine(waitHandle);
            });
        };

        GameSceneManager.FinishingPhase.ExecuteTaskConcurrently(loadPlayerDataOnLoad, SaveLoadManager.Progress, ExecuteAmount.Once, GameStage.GameTime);
    }

public object GetContextData() {
    return null;
}

public void SetContextData(object contextData) {

}
}
