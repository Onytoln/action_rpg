using MEC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;
using JsonPropertyAttribute = Newtonsoft.Json.JsonPropertyAttribute;

public static class SaveLoadManager {
    public enum SaveType { Overwrite, CreateNew }

    public const string SAVE_FILE_EXTENSION = ".sav";
    public const string GLOBAL_SAVE_FILE_EXTENSION = ".globSav";
    public const string SAVE_DIRECTORY_NAME = "MaskARPG\\Saves";
    public const string GLOBAL_SAVE_DIRECTORY_NAME = "GlobalSaves";

    private static string _fileSystemSaveDirParent;
    public static string FileSystemSaveDirParent {
        get {
            if (string.IsNullOrEmpty(_fileSystemSaveDirParent)) {
                _fileSystemSaveDirParent = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            return _fileSystemSaveDirParent;
        }
    }

    public static string FullPlayerSystemSaveDirPath
        => Path.Combine(FileSystemSaveDirParent, SAVE_DIRECTORY_NAME);

    public static string FullGlobalSystemSaveDirPath
        => Path.Combine(FullPlayerSystemSaveDirPath, GLOBAL_SAVE_DIRECTORY_NAME);

    #region Saving variables

    public static bool IsSavingPlayerData { get; private set; }
    public static bool IsSavingGlobalData { get; private set; }
    public static bool IsSaving { get; private set; }
    public static event Action<bool> OnIsSaving;

    private static float _savingPlayerProgress;
    private static float SavingPlayerProgress {
        get => _savingPlayerProgress;
        set {
            _savingPlayerProgress = value;
            SetProgress();
        }
    }

    private static float _savingGlobalProgress;
    private static float SavingGlobalProgress {
        get => _savingGlobalProgress;
        set {
            _savingGlobalProgress = value;
            SetProgress();
        }
    }

    private static CoroutineHandle? savingPlayerCoroutine;
    private static CoroutineHandle? savingGlobalCoroutine;

    #endregion

    #region Loading variables

    public static bool IsLoadingAllData { get; private set; }
    public static bool IsLoadingPlayerData { get; private set; }
    public static bool IsLoadingGlobalData { get; private set; }
    public static bool IsLoading { get; private set; }
    public static event Action<bool> OnIsLoading;
    private static CoroutineHandle? loadingAllCoroutine;
    private static CoroutineHandle? loadingPlayerCoroutine;
    private static CoroutineHandle? loadingGlobalCoroutine;

    private static float _loadingAllPlayerProgress;
    private static float LoadingAllPlayerProgress {
        get => _loadingAllPlayerProgress;
        set {
            _loadingAllPlayerProgress = value;
            SetProgress();
        }
    }

    private static float _loadingAllGlobalProgress;
    private static float LoadingAllGlobalProgress {
        get => _loadingAllGlobalProgress;
        set {
            _loadingAllGlobalProgress = value;
            SetProgress();
        }
    }

    private static float _loadingPlayerProgress;
    private static float LoadingPlayerProgress {
        get => _loadingPlayerProgress;
        set {
            _loadingPlayerProgress = value;
            SetProgress();
        }
    }

    private static float _loadingGlobalProgress;
    private static float LoadingGlobalProgress {
        get => _loadingGlobalProgress;
        set {
            _loadingGlobalProgress = value;
            SetProgress();
        }
    }

    public static readonly List<PlayerSaveFileContent> LoadedPlayerSaves = new List<PlayerSaveFileContent>();
    public static readonly List<GlobalSaveFileContent> LoadedGlobalSaves = new List<GlobalSaveFileContent>();

    #endregion

    private readonly static ProgressContainer _progressContainer = new ProgressContainer(true);
    public static ProgressContainer Progress => _progressContainer;

    #region Save

    #region Save Player Data

    public static CoroutineHandle SavePlayer(string saveFileName, IPlayerCore player, SaveType saveType = SaveType.CreateNew) {
        return SavePlayerInternal(saveFileName, player, saveType);
    }

    private static CoroutineHandle SavePlayerInternal(string saveFileName, IPlayerCore player, SaveType saveType) {
        if (IsSavingPlayerData) {
            Debug.Log("Game is already saving player data.");
            return default;
        }

        if (player == null) {
            Debug.LogError("Cannot save player data with null player!");
            return default;
        }

        return (CoroutineHandle)(savingPlayerCoroutine = Timing.RunCoroutine(SavePlayerCoroutine(saveFileName, player, saveType)));
    }

    private static IEnumerator<float> SavePlayerCoroutine(string saveFileName, IPlayerCore player, SaveType saveType) {
        SavingPlayerProgress = 0f;
        SetSaving(SaveLoadDataType.Player, true);

        List<ISaveLoadComponent> saveLoadComps = new List<ISaveLoadComponent>();

        yield return Timing.WaitUntilDone(Utils.GetAllComponentsOfTypeOverTime(saveLoadComps));

        SavingPlayerProgress = 0.20f;

        yield return Timing.WaitUntilDone(new AwaitTasks(ValidateSaveables(saveLoadComps)));

        SavingPlayerProgress = 0.40f;

        ISaveLoadPlayerContext saveLoadContext = GetSaveLoadPlayerContext(saveLoadComps);

        if (string.IsNullOrEmpty(saveFileName)) {
            saveFileName = PlayerSaveLoadFileIdent.GenerateSaveFileName(player.Name, saveLoadContext.SaveFileNameSuffix);
        }

        if (saveType == SaveType.CreateNew) {
            var files = Utils.GetAllFilesWithString(FullPlayerSystemSaveDirPath, saveFileName, SearchOption.TopDirectoryOnly);

            if (!files.NullOrEmpty()) {
                int highestNumbering = Utils.GetHighestFileNumbering(files.ToArray());
                saveFileName += $"({highestNumbering + 1})";
            }
        }

        PlayerSaveLoadFileIdent saveLoadManagerFileIdent = new PlayerSaveLoadFileIdent(saveFileName, player);

        Dictionary<string, object> saveData = new Dictionary<string, object>();

        for (int i = 0; i < saveLoadComps.Count; i++) {
            List<ISaveable> saveables = GetSaveLoadCompsOfType<ISaveable, IGlobalSaveable>(saveLoadComps[i]);
            if (saveables.NullOrEmpty()) continue;

            foreach (var saveable in saveables) {
                saveData.Add(saveable.SaveableID, saveable.Save());
            }
        }

        yield return Timing.WaitUntilDone(new AwaitTasks(
            SaveCoreAsync(saveLoadManagerFileIdent.FullFileName,
            new PlayerSaveFileContent(saveLoadManagerFileIdent,
            saveLoadContext,
            saveData))));

        SavingPlayerProgress = 1f;

        SetSaving(SaveLoadDataType.Player, false);
        savingPlayerCoroutine = null;
    }

    #endregion

    #region Save Global Data

    public static CoroutineHandle SaveGlobals() {
        return SaveGlobalsInternal();
    }

    private static CoroutineHandle SaveGlobalsInternal() {
        if (IsSavingGlobalData) {
            Debug.Log("Game is already saving global data.");
            return default;
        }

        return (CoroutineHandle)(savingGlobalCoroutine = Timing.RunCoroutine(SaveGlobalsCoroutine()));
    }

    private static IEnumerator<float> SaveGlobalsCoroutine() {
        SavingGlobalProgress = 0f;
        SetSaving(SaveLoadDataType.Global, true);

        List<ISaveLoadComponent> saveLoadComps = new List<ISaveLoadComponent>();

        yield return Timing.WaitUntilDone(Utils.GetAllComponentsOfTypeOverTime(saveLoadComps));

        SavingGlobalProgress = 0.2f;

        yield return Timing.WaitUntilDone(new AwaitTasks(ValidateGlobals(saveLoadComps)));

        SavingGlobalProgress = 0.4f;

        for (int i = 0; i < saveLoadComps.Count; i++) {
            List<IGlobalSaveable> globalSaveables = GetSaveLoadCompsOfType<IGlobalSaveable>(saveLoadComps[i]);
            if (globalSaveables.NullOrEmpty()) continue;

            foreach (var globalSaveable in globalSaveables) {
                var globalSaveFileContent = new GlobalSaveFileContent(globalSaveable.SaveFileName, globalSaveable.SaveableID, globalSaveable.Save());

                yield return Timing.WaitUntilDone(new AwaitTasks(
                    SaveCoreAsync(globalSaveFileContent.FullFileName, globalSaveFileContent, GLOBAL_SAVE_DIRECTORY_NAME)));

                SavingGlobalProgress = 0.4f + (0.6f * (i / saveLoadComps.Count));
            }
        }

        SavingGlobalProgress = 1f;

        SetSaving(SaveLoadDataType.Global, false);
    }

    #endregion

    private static Task SaveCoreAsync<T>(string fileName, T saveContent, string subDirectory = null) {
        return Task.Run(async () => {
            string jsonified = JsonConvert.SerializeObject(saveContent);

            string directoryPath = Path.Combine(FileSystemSaveDirParent, SAVE_DIRECTORY_NAME);

            if (!string.IsNullOrEmpty(subDirectory)) {
                directoryPath = Path.Combine(directoryPath, subDirectory);
            }

            Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, fileName);
            await Utils.WriteFileAsync(filePath, jsonified);
        });
    }

    #endregion

    #region Load

    #region Load All Into cache

    public static CoroutineHandle LoadAllSavesIntoCache() {
        if (IsLoadingAllData) {
            Debug.Log("Game is already loading some saves.");
            return default;
        }

        return (CoroutineHandle)(loadingAllCoroutine = Timing.RunCoroutine(LoadAllSavesIntoCacheAsyncCoroutine()));
    }

    private static IEnumerator<float> LoadAllSavesIntoCacheAsyncCoroutine() {
        LoadingAllPlayerProgress = 0f;
        LoadingAllGlobalProgress = 0f;
        SetLoading(SaveLoadDataType.All, true);

        CoroutineHandle playerLoad = Timing.RunCoroutine(LoadGlobalSavesCoroutine());
        CoroutineHandle globalLoad = Timing.RunCoroutine(LoadPlayerSavesCoroutine());

        yield return Timing.WaitUntilDone(playerLoad);
        yield return Timing.WaitUntilDone(globalLoad);

        LoadingAllPlayerProgress = 1f;
        LoadingAllGlobalProgress = 1f;

        SetLoading(SaveLoadDataType.All, false);
        loadingGlobalCoroutine = null;
        loadingPlayerCoroutine = null;
    }

    private static IEnumerator<float> LoadPlayerSavesCoroutine() {
        LoadedPlayerSaves.Clear();

        var files = Utils.GetAllFilesWithString(FullPlayerSystemSaveDirPath, SearchOption.TopDirectoryOnly);

        for (int i = 0; i < files.Count; i++) {
            ValueWrapper<PlayerSaveFileContent> result = new ValueWrapper<PlayerSaveFileContent>();

            yield return Timing.WaitUntilDone(new AwaitTasks(LoadCoreAsync(files[i], result)));

            if (result.Value != null) {
                LoadedPlayerSaves.Add(result.Value);
            }

            LoadingAllPlayerProgress = (float)(i + 1) / files.Count;
        }

        //Debug.Log($"Found {LoadedPlayerSaves.Count} saves.");
    }

    private static IEnumerator<float> LoadGlobalSavesCoroutine() {
        LoadedGlobalSaves.Clear();

        var files = Utils.GetAllFilesWithString(FullGlobalSystemSaveDirPath, SearchOption.TopDirectoryOnly);

        for (int i = 0; i < files.Count; i++) {
            ValueWrapper<GlobalSaveFileContent> result = new ValueWrapper<GlobalSaveFileContent>();

            yield return Timing.WaitUntilDone(new AwaitTasks(LoadCoreAsync(files[i], result)));

            if (result.Value != null) {
                LoadedGlobalSaves.Add(result.Value);
            }

            LoadingAllGlobalProgress = (float)(i + 1) / files.Count;
        }

        //Debug.Log($"Found {LoadedGlobalSaves.Count} saves.");
    }

    #endregion

    private static Task LoadCoreAsync<T>(string path, ValueWrapper<T> result) {
        return Task.Run(async () => {
            string fileText = await Utils.ReadFileAsync(path);

            T fromJson = JsonConvert.DeserializeObject<T>(fileText);

            result.Value = fromJson;
        });
    }

    #region Load Player Data

    public static CoroutineHandle LoadPlayerData(PlayerSaveFileContent playerSaveFileContent) {
        if (IsLoadingPlayerData) {
            Debug.Log("Game is already loading player saves.");
            return default;
        }

        if (playerSaveFileContent == null) {
            Debug.LogError("Cannot load player save of null player save file content.");
            return default;
        }

        return (CoroutineHandle)(loadingPlayerCoroutine = Timing.RunCoroutine(LoadPlayerDataCoroutine(playerSaveFileContent)));
    }

    public static CoroutineHandle LoadPlayerData(IPlayerCore playerCore) {
        if (IsLoadingPlayerData) {
            Debug.Log("Game is already loading player saves.");
            return default;
        }

        if (playerCore == null) {
            Debug.LogError("Cannot load player save of null IPlayerCore");
            return default;
        }

        PlayerSaveFileContent playerSaveFileContent = GetPlayerSaveFileContent(playerCore);
        if (playerSaveFileContent == null) {
            Debug.LogError("No player save found based on sent player.");
            return default;
        }

        return (CoroutineHandle)(loadingPlayerCoroutine = Timing.RunCoroutine(LoadPlayerDataCoroutine(playerSaveFileContent)));
    }

    private static IEnumerator<float> LoadPlayerDataCoroutine(PlayerSaveFileContent playerSaveFileContent) {
        LoadingPlayerProgress = 0f;

        SetLoading(SaveLoadDataType.Player, true);

        List<ISaveLoadComponent> saveLoadComps = new List<ISaveLoadComponent>();

        LoadingPlayerProgress = 0.5f;

        yield return Timing.WaitUntilDone(Utils.GetAllComponentsOfTypeOverTime(saveLoadComps));

        ISaveLoadPlayerContext saveLoadContext = GetSaveLoadPlayerContext(saveLoadComps);
        saveLoadContext.SetContextData(playerSaveFileContent.SaveLoadContextData);

        Dictionary<string, object> saveData = playerSaveFileContent.SaveData as Dictionary<string, object>;

        for (int i = 0; i < saveLoadComps.Count; i++) {
            List<ISaveable> saveables = GetSaveLoadCompsOfType<ISaveable, IGlobalSaveable>(saveLoadComps[i]);

            for (int j = 0; j < saveables.Count; j++) {
                if (saveData.TryGetValue(saveables[j].SaveableID, out object value)) {
                    saveables[j].Load(value);
                }
            }
        }

        LoadingPlayerProgress = 1f;

        SetLoading(SaveLoadDataType.Player, false);
        loadingPlayerCoroutine = null;
    }

    #endregion

    #region Load Global Data

    public static CoroutineHandle LoadGlobalData() {
        if (IsLoadingGlobalData) {
            Debug.Log("Game is already loading global saves.");
            return default;
        }

        return (CoroutineHandle)(loadingGlobalCoroutine = Timing.RunCoroutine(LoadGlobalDataCoroutine()));
    }

    private static IEnumerator<float> LoadGlobalDataCoroutine() {
        LoadingGlobalProgress = 0f;

        SetLoading(SaveLoadDataType.Global, true);

        List<ISaveLoadComponent> saveLoadComps = new List<ISaveLoadComponent>();

        LoadingGlobalProgress = 0.5f;

        yield return Timing.WaitUntilDone(Utils.GetAllComponentsOfTypeOverTime(saveLoadComps));

        for (int i = 0; i < saveLoadComps.Count; i++) {
            List<IGlobalSaveable> globalSaveables = GetSaveLoadCompsOfType<IGlobalSaveable>(saveLoadComps[i]);

            for (int j = 0; j < globalSaveables.Count; j++) {
                GlobalSaveFileContent globalSaveFileContent = GetGlobalSaveFileContent(globalSaveables[j]?.SaveableID);

                if (globalSaveFileContent == null) {
                    Debug.LogError($"Save for global saveable {globalSaveables[j].GetType()} is not loaded.");
                    continue;
                }

                globalSaveables[j].Load(globalSaveFileContent.SaveData);
            }
        }

        LoadingGlobalProgress = 1f;

        SetLoading(SaveLoadDataType.Global, false);
        loadingGlobalCoroutine = null;
    }

    #endregion

    #endregion

    #region Helpers

    private enum SaveLoadDataType { Player, Global, All }

    private static void SetSaving(SaveLoadDataType saveLoadDataType, bool value) {
        switch (saveLoadDataType) {
            case SaveLoadDataType.Player:
                IsSavingPlayerData = value;
                break;
            case SaveLoadDataType.Global:
                IsSavingGlobalData = value;
                break;
            case SaveLoadDataType.All:
                throw new Exception("Saving data type All does not exist.");
        }

        bool prevIsSaving = IsSaving;

        if (!IsSavingPlayerData && !IsSavingGlobalData) {
            IsSaving = false;
        } else {
            IsSaving = true;
        }

        if (prevIsSaving != IsSaving)
            OnIsSaving?.Invoke(IsSaving);
    }

    private static void SetLoading(SaveLoadDataType saveLoadDataType, bool value) {
        switch (saveLoadDataType) {
            case SaveLoadDataType.Player:
                IsLoadingPlayerData = value;
                break;
            case SaveLoadDataType.Global:
                IsLoadingGlobalData = value;
                break;
            case SaveLoadDataType.All:
                IsLoadingAllData = value;
                break;
        }

        bool prevIsLoading = IsLoading;

        if (!IsLoadingAllData && !IsLoadingPlayerData && !IsLoadingGlobalData) {
            IsLoading = false;
        } else {
            IsLoading = true;
        }

        if (prevIsLoading != IsLoading)
            OnIsLoading?.Invoke(IsLoading);
    }

    private static Task ValidateSaveables(List<ISaveLoadComponent> saveLoadComps) {
        return Task.Run(() => {
            HashSet<string> ids = new HashSet<string>();

            for (int i = 0; i < saveLoadComps.Count; i++) {
                List<ISaveable> saveables = GetSaveLoadCompsOfType<ISaveable>(saveLoadComps[i]);
                if (saveables.NullOrEmpty()) continue;

                foreach (ISaveable saveable in saveables) {
                    if (string.IsNullOrEmpty(saveable.SaveableID)) {
                        throw new Exception($"Saveables have empty ID! Saveable: {saveable.GetType()}");
                    }

                    if (ids.Contains(saveable.SaveableID)) {
                        throw new Exception($"Saveables have a duplicate ID! ID: {saveable.SaveableID} Saveable: {saveable.GetType()}");
                    }

                    ids.Add(saveable.SaveableID);
                }
            }
        });
    }

    private static Task ValidateGlobals(List<ISaveLoadComponent> saveLoadComps) {
        return Task.Run(() => {
            HashSet<string> ids = new HashSet<string>();
            HashSet<string> filesNames = new HashSet<string>();

            for (int i = 0; i < saveLoadComps.Count; i++) {
                List<IGlobalSaveable> globalSaveables = GetSaveLoadCompsOfType<IGlobalSaveable>(saveLoadComps[i]);
                if (globalSaveables.NullOrEmpty()) continue;

                foreach (IGlobalSaveable globalSaveable in globalSaveables) {
                    if (string.IsNullOrEmpty(globalSaveable.SaveableID)) {
                        throw new Exception($"Global saveables have empty ID! Global saveable: {globalSaveable.GetType()}");
                    }

                    if (ids.Contains(globalSaveable.SaveableID)) {
                        throw new Exception($"Global saveables have a duplicate ID! ID: {globalSaveable.SaveableID}, Global saveable: {globalSaveable.GetType()}");
                    }

                    if (string.IsNullOrEmpty(globalSaveable.SaveFileName)) {
                        throw new Exception($"Global saveables have empty save file name! Global saveable: {globalSaveable.GetType()}");
                    }

                    if (filesNames.Contains(globalSaveable.SaveFileName)) {
                        throw new Exception($"Global saveables have duplicate save file name!" +
                            $" Duplicate name: {globalSaveable.SaveFileName}, Global saveable: {globalSaveable.GetType()}");
                    }

                    if (string.IsNullOrEmpty(globalSaveable.GlobalSaveableContext)) {
                        Debug.LogError($"Global saveable has empty or null context. This should not happen. Global saveable: {globalSaveable.GetType()}");
                    }

                    ids.Add(globalSaveable.SaveableID);
                    filesNames.Add(globalSaveable.SaveFileName);
                }
            }
        });
    }

    private static List<T> GetSaveLoadCompsOfType<T>(ISaveLoadComponent saveLoadComponent) where T : ISaveLoadComponent {
        List<T> results = new List<T>();

        if (saveLoadComponent is T saveable) {
            results.Add(saveable);
        } else if (saveLoadComponent is ISaveLoadComponentProvider saveLoadComponentProvider) {
            foreach (var saveloadComp in saveLoadComponentProvider.GetSaveLoadComponents()) {
                if (saveloadComp is T saveable1) {
                    results.Add(saveable1);
                }
            }
        }

        return results;
    }

    private static List<T> GetSaveLoadCompsOfType<T, U>(ISaveLoadComponent saveLoadComponent, U exclude = default)
        where T : ISaveLoadComponent
        where U : ISaveLoadComponent {

        List<T> results = new List<T>();

        if (saveLoadComponent is T saveable && !typeof(U).IsAssignableFrom(saveLoadComponent.GetType())) {
            results.Add(saveable);
        } else if (saveLoadComponent is ISaveLoadComponentProvider saveLoadComponentProvider) {
            foreach (var saveloadComp in saveLoadComponentProvider.GetSaveLoadComponents()) {
                if (saveloadComp is T saveable1 && !typeof(U).IsAssignableFrom(saveLoadComponent.GetType())) {
                    results.Add(saveable1);
                }
            }
        }

        return results;
    }

    private static ISaveLoadPlayerContext GetSaveLoadPlayerContext(List<ISaveLoadComponent> saveLoadComponents) {
        ISaveLoadPlayerContext saveLoadContextResult = null;

        for (int i = 0; i < saveLoadComponents.Count; i++) {
            List<ISaveLoadPlayerContext> saveLoadPlayerContexts = GetSaveLoadCompsOfType<ISaveLoadPlayerContext>(saveLoadComponents[i]);
            if (saveLoadPlayerContexts.NullOrEmpty()) continue;

            foreach (var saveLoadPlayerContext in saveLoadPlayerContexts) {
                if (saveLoadContextResult == null) {
                    saveLoadContextResult = saveLoadPlayerContext;
                } else {
                    throw new Exception("Game has multiple SaveLoad contexts. This is not allowed!");
                }
            }
        }

        if (saveLoadContextResult == null) {
            throw new Exception("Game has no SaveLoad context!");
        }

        return saveLoadContextResult;
    }

    private static PlayerSaveFileContent GetPlayerSaveFileContent(IPlayerCore playerCore) {
        if (playerCore == null) {
            Debug.LogError($"{nameof(playerCore)} is null for GetPlayerSaveData!");
            return null;
        }

        DateTime newest = LoadedPlayerSaves.Max(x => x.SaveLoadManagerFileIdent.SaveDateTime);

        for (int i = 0; i < LoadedPlayerSaves.Count; i++) {
            if (LoadedPlayerSaves[i].SaveLoadManagerFileIdent.SaveGuid == playerCore.Guid && LoadedPlayerSaves[i].SaveLoadManagerFileIdent.SaveDateTime == newest)
                return LoadedPlayerSaves[i];
        }

        return null;
    }

    private static GlobalSaveFileContent GetGlobalSaveFileContent(string id) {
        for (int i = 0; i < LoadedGlobalSaves.Count; i++) {
            if (LoadedGlobalSaves[i].SaveableID == id) return LoadedGlobalSaves[i];
        }

        return null;
    }

    private static void SetProgress() {
        float totalProgress = 0f;
        int totalContributors = 0;

        if (savingPlayerCoroutine != null) {
            totalContributors++;
            totalProgress += SavingPlayerProgress;
        }

        if (savingGlobalCoroutine != null) {
            totalContributors++;
            totalProgress += SavingGlobalProgress;
        }

        if (loadingAllCoroutine != null) {
            totalContributors += 2;
            totalProgress += LoadingAllPlayerProgress + LoadingAllGlobalProgress;
        }

        if (loadingPlayerCoroutine != null) {
            totalContributors++;
            totalProgress += LoadingPlayerProgress;
        }

        if (loadingGlobalCoroutine != null) {
            totalContributors++;
            totalProgress += LoadingGlobalProgress;
        }

        float result = Mathf.Clamp01(totalProgress / totalContributors);

        //Debug.Log($"SaveLoad progress: {result}");
        Progress.Progress = float.IsNaN(result) ? 0f : result;
    }

    #endregion

    #region Helper Save Data Classes 

    public class PlayerSaveLoadFileIdent {
        [JsonProperty] public string SaveFileName { get; private set; }
        [JsonProperty] public string PlayerName { get; private set; }
        [JsonProperty] public Guid SaveGuid { get; private set; }
        [JsonProperty] public DateTime SaveDateTime { get; private set; }

        [JsonIgnore] public string FullFileName => $"{SaveFileName}{SAVE_FILE_EXTENSION}";

        public PlayerSaveLoadFileIdent() { }

        public PlayerSaveLoadFileIdent(string saveFileName, IPlayerCore player) {
            this.SaveFileName = saveFileName;
            this.PlayerName = player.Name;
            SaveGuid = player.Guid;
            SaveDateTime = DateTime.Now;
        }

        public static string GenerateSaveFileName(string playerName, string saveFileSuffix) {
            return $"{playerName}_{saveFileSuffix}";
        }

        public static string GenerateRandomSaveFileName(string playerName, string saveFileSuffix) {
            return $"{playerName}_{saveFileSuffix}_{Utils.RandomString(8)}";
        }
    }

    public class PlayerSaveFileContent {
        [JsonProperty] public PlayerSaveLoadFileIdent SaveLoadManagerFileIdent { get; private set; }
        [JsonProperty] public string SaveLoadContextSaveFileName { get; private set; }
        [JsonProperty] public object SaveLoadContextData { get; private set; }
        [JsonProperty] public object SaveData { get; private set; }

        public PlayerSaveFileContent() { }

        public PlayerSaveFileContent(PlayerSaveLoadFileIdent saveLoadManagerFileIdent, ISaveLoadPlayerContext saveLoadContext, object saveData) {
            this.SaveLoadManagerFileIdent = saveLoadManagerFileIdent;
            this.SaveLoadContextSaveFileName = saveLoadContext.SaveFileNameSuffix;
            this.SaveLoadContextData = saveLoadContext.GetContextData();
            this.SaveData = saveData;
        }
    }

    public class GlobalSaveFileContent {
        [JsonProperty] public string SaveableID { get; private set; }
        [JsonProperty] public string SaveFileName { get; private set; }
        [JsonProperty] public DateTime SaveDateTime { get; private set; }
        [JsonProperty] public object SaveData { get; private set; }

        [JsonIgnore] public string FullFileName => $"{SaveFileName}{GLOBAL_SAVE_FILE_EXTENSION}";

        public GlobalSaveFileContent() { }

        public GlobalSaveFileContent(string saveFileName, string saveableID, object saveData) {
            SaveFileName = saveFileName;
            SaveDateTime = DateTime.Now;
            SaveableID = saveableID;
            SaveData = saveData;
        }
    }

    #endregion
}
