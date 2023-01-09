using MEC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class CombatUI : MonoBehaviour, IDisableUntilGameLoaded {
    private PlayerStats playerStats;
    [SerializeField] private Image healthFill;
    [SerializeField] private Text healthText;
    [SerializeField] private Image manaFill;
    [SerializeField] private Text manaText;

    private float healthTarget;
    private float manaTarget;

    private CoroutineHandle healthBarUpdateHandle;
    private CoroutineHandle manaBarUpdateHandle;

    private void Awake() {
        GameSceneManager.UIPhase.ExecuteSync(Initialize, null, ExecuteAmount.Once);
    }

    private void Initialize() {
        playerStats = TargetManager.PlayerComponent.CharacterStats as PlayerStats;

        healthBarUpdateHandle = Timing.RunCoroutine(UpdateHealthBarCoroutine());
        manaBarUpdateHandle = Timing.RunCoroutine(UpdateManaBarCoroutine());

        playerStats.OnCurrentHealthChange += UpdateHealthBar;
        UpdateHealthBar(0f, null);

        playerStats.OnCurrentManaChange += UpdateManaBar;
        UpdateManaBar(0f, null);
    }

    private void UpdateHealthBar(float health, CharacterStats charStats) {
        healthTarget = playerStats.CurrentHealthPercentage;
        healthText.text = health.ToString("F0") + "/" + playerStats.CoreStats.HealthValue.ToString("F0");
    }

    private void UpdateManaBar(float mana, CharacterStats charStats) {
        manaTarget = playerStats.CurrentManaPercentage;
        manaText.text = mana.ToString("F0") + "/" + playerStats.CoreStats.ManaValue.ToString("F0");
    }

    private IEnumerator<float> UpdateHealthBarCoroutine() {
        while (true) {
            healthFill.fillAmount = Mathf.Lerp(healthFill.fillAmount, healthTarget, Time.deltaTime * 5f);
            yield return Timing.WaitForOneFrame;
        }
    }

    private IEnumerator<float> UpdateManaBarCoroutine() {
        while (true) {
            manaFill.fillAmount = Mathf.Lerp(manaFill.fillAmount, manaTarget, Time.deltaTime * 5f);
            yield return Timing.WaitForOneFrame;
        }
    }
}
