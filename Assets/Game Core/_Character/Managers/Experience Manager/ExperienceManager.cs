using System;
using UnityEngine;

public class ExperienceManager : MonoBehaviour, ILoadable {
    public static ExperienceManager Instance { get; private set; }

    public bool IsLoaded { get; private set; } = false;
    public event Action<ILoadable> OnLoad;

    private int currentPlayerExperience;
    public int CurrentPlayerExperience { get => currentPlayerExperience; }

    private int currentPlayerLevel;
    public int CurrentPlayerLevel { get => currentPlayerLevel; }

    [SerializeField] private int[] experienceRequired;
    [SerializeField] private float[] experienceMultiplier;

    private EventManager eventManager;
    private EventManager EventManager { get { if (eventManager == null) eventManager = EventManager.Instance; return eventManager; } }

    public event Action<int> OnPlayerLevelUp;
    public event Action<int> OnPlayerExperienceGained;
    public delegate void PrePlayerExperienceGainedDelegate(ref int experienceGained);
    /// <summary>
    /// Send experience gained via reference, modifiable.
    /// </summary>
    public event PrePlayerExperienceGainedDelegate PrePlayerExperienceGained;

    private void Awake() {
        if (Instance == null) Instance = this;
        currentPlayerExperience = 0;
        currentPlayerLevel = 1;

        if(experienceRequired.Length != experienceMultiplier.Length) {
            throw new Exception("Experience required and experience multipliers are not equal in length!");
        }

        EventManager.OnExperienceSourceTriggered += AddExperienceFromTriggeredSource;
    }

    void Start() {
        IsLoaded = true;
        OnLoad?.Invoke(this);
    }

    private void AddExperienceFromTriggeredSource(IExperience experience) {
        if (!experience.CanGiveExperience) return;

        AddExperience((int)(experience.DefaultExperienceGain * experienceMultiplier[experience.GetCurrentLevel() - 1]));
    }
    

    public void AddExperience(int experienceGain) {
        if (experienceGain <= 0 || CurrentPlayerLevel - 1 == experienceRequired.Length) return;

        PrePlayerExperienceGained?.Invoke(ref experienceGain);

        int overlappedXp = GetRemainingExperienceForNextLevel() - experienceGain;
        overlappedXp = overlappedXp < 0 ? overlappedXp : 0;

        currentPlayerExperience += experienceGain;
        if(currentPlayerExperience >= GetTotalExperienceForNextLevel()) {
            currentPlayerExperience = 0;
            currentPlayerLevel += 1;
            OnPlayerLevelUp?.Invoke(currentPlayerLevel);
        }

        OnPlayerExperienceGained?.Invoke(experienceGain + overlappedXp);

        AddExperience(-overlappedXp);
    }

    public int GetTotalExperienceForNextLevel() {
        return experienceRequired[currentPlayerLevel - 1];
    }

    public int GetRemainingExperienceForNextLevel() {
        return experienceRequired[currentPlayerLevel - 1] - currentPlayerExperience;
    }

    public int GetMaxPossibleLevel() {
        return experienceRequired.Length;
    }

}
