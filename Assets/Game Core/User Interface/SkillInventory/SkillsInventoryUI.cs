using MEC;
using System.Collections.Generic;
using UnityEngine;

public class SkillsInventoryUI : MonoBehaviour, IUiWindow, IDisableUntilGameLoaded {
    [field: SerializeField] public KeyCode OpenWindowKey { get; private set; }
    
    [SerializeField]
    private SkillInventorySlot skillSlotPrefab;
    [SerializeField]
    private Transform skillSlotsParent;

    [SerializeField]
    private CanvasGroup canvasGroup;
    [SerializeField]
    private Canvas canvas;

    private bool isEnabled;

    private Player playerDataComponent;

    private SkillInventorySlot[] skillSlots;

    CoroutineHandle skillsInventoryFade;

    private InputManager _inputManager;
    private InputManager InputManager { get { if (_inputManager == null) _inputManager = InputManager.Instance; return _inputManager; } }


    void Awake() {
        GameSceneManager.UIPhase.ExecuteSync(Initialize, null, ExecuteAmount.Once);
    }

    private void Initialize() {
        playerDataComponent = TargetManager.PlayerComponent;

        PlayerSkillTemplate[] playerSkills = playerDataComponent.PlayerSkills;

        skillSlots = new SkillInventorySlot[playerSkills.Length];
        for (int i = 0; i < playerSkills.Length; i++) {
            skillSlots[i] = Instantiate(skillSlotPrefab, skillSlotsParent);
            skillSlots[i].AddSkill(playerSkills[i]);
            skillSlots[i].SkillDragComponent.parentCanvas = canvas;
        }

        isEnabled = false;
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void Update()
    {
        if (InputManager.IsKeyDown(OpenWindowKey)) {
            if (isEnabled) {
                isEnabled = false;
                skillsInventoryFade = Utils.FadeCanvasGroup(canvasGroup, false, skillsInventoryFade);
            } else {
                isEnabled = true;
                skillsInventoryFade = Utils.FadeCanvasGroup(canvasGroup, true, skillsInventoryFade);
            }
        }
    }
}
