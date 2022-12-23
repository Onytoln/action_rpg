using MEC;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour, IUiWindow {
    [field: SerializeField] public KeyCode OpenWindowKey { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private Text levelText;

    private bool isEnabled;

    private CoroutineHandle playerStatsFade;

    private InputManager inputManager;
    private InputManager InputManager { get { if (inputManager == null) inputManager = InputManager.Instance; return inputManager; } }

    private PlayerStats playerStats;

    [SerializeField] private GameObject statsParentObject;

    private StatElement[] statElements;

    void Awake() {
        statElements = statsParentObject.GetComponentsInChildren<StatElement>();
    }

    void Start() {
        LoadHandler.NotifyOnLoad(InputManager.Instance,(loadable) =>
        LoadHandler.NotifyOnLoad(TargetManager.Instance, (loadable) => 
        Initialize()));
    }

    private void Initialize() {
        playerStats = TargetManager.Player.GetComponent<PlayerStats>();

        ChracterStat[] stats = playerStats.CoreStats.Stats;

        for (int i = 0; i < statElements.Length; i++) {
            for (int j = 0; j < stats.Length; j++) {
                if (statElements[i].statTypeToAssign == stats[j].statType) {
                    statElements[i].AddStat(stats[j]);
                    break;
                }
            }
        }

        isEnabled = false;
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        playerStats.OnCharacterStatChange += UpdateStats;

        LoadHandler.NotifyOnLoad(ExperienceManager.Instance, LoadPlayerLevel);

        ExperienceManager.Instance.OnPlayerLevelUp += UpdatePlayerLvl;
    }

    void Update() {
        if (InputManager.IsKeyDown(OpenWindowKey)) {
            if (isEnabled) {
                isEnabled = false;
                playerStatsFade = Utils.FadeCanvasGroup(canvasGroup, false, playerStatsFade);
            } else {
                isEnabled = true;
                playerStatsFade = Utils.FadeCanvasGroup(canvasGroup, true, playerStatsFade);
            }
        }
    }

    void UpdateStats(ChracterStat stat) {
        for (int i = 0; i < statElements.Length; i++) {
            if (statElements[i].statTypeToAssign == stat.statType) {
                statElements[i].RefreshStatValue();
                break;
            }
        }
    }

    void LoadPlayerLevel(ILoadable loadable) {
        levelText.text = (loadable as ExperienceManager).CurrentPlayerLevel.ToString();
    }

    void UpdatePlayerLvl(int lvl) {
        levelText.text = lvl.ToString();
    }
}
