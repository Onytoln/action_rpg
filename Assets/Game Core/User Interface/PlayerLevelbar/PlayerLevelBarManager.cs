using MEC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLevelBarManager : MonoBehaviour {
    [SerializeField] private Image fillImage;
    [SerializeField] private Text levelText;
    [SerializeField] private PlayerBarElementHover[] hoverElements;

    [SerializeField] private float fillImageLerpSpeed = 3;
    private CoroutineHandle currentFillLerpCoroutine;
    private float currentLvlXpProgression = 0f;

    private ExperienceManager experienceManager;
    private ExperienceManager ExperienceManager { get { if (experienceManager == null) experienceManager = ExperienceManager.Instance; return experienceManager; } }

    private void Awake() {
        for (int i = 0; i < hoverElements.Length; i++) {
            hoverElements[i].OnPointerEnterEvent += ShowLvlTooltip;
        }

        ExperienceManager.OnPlayerExperienceGained += HandleExperienceChanges;
        ExperienceManager.OnPlayerLevelUp += (level) => { 
            levelText.text = level.ToString();
            currentLvlXpProgression = 0; 
        };
    }

    private void Start() {
        LoadHandler.NotifyOnLoad(ExperienceManager.Instance, LoadXpData);
    }

    private void LoadXpData(ILoadable loadable) {
        levelText.text = ExperienceManager.CurrentPlayerLevel.ToString();
        HandleExperienceChanges(0);
    }


    private void HandleExperienceChanges(int experienceGained) {
        Timing.KillCoroutines(currentFillLerpCoroutine);
        currentFillLerpCoroutine = Timing.RunCoroutine(LerpXpFillAmount());
    }

    private void ShowLvlTooltip(bool show) {
        if (show) {
            AdvancedTooltip.Instance.ShowTooltip($"Experience required for next level: {ExperienceManager.GetRemainingExperienceForNextLevel()}" +
                $" ({ExperienceManager.CurrentPlayerExperience}/{ExperienceManager.GetTotalExperienceForNextLevel()})");
        } else {
            AdvancedTooltip.Instance.HideTooltip();
        }
    }

    private IEnumerator<float> LerpXpFillAmount() {
        float start = currentLvlXpProgression;
        float goal = ExperienceManager.CurrentPlayerExperience;
        float lerpTime = 0f;

        while (lerpTime < 1f) {
            currentLvlXpProgression = Mathf.SmoothStep(start, goal, lerpTime);
            fillImage.fillAmount =  currentLvlXpProgression / ExperienceManager.GetTotalExperienceForNextLevel();
            lerpTime += Time.deltaTime * fillImageLerpSpeed;
            yield return Timing.WaitForOneFrame;
        }
    }
}
