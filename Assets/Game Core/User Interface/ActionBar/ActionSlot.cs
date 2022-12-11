using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler {
    public Action onSlotChanged;

    //core
    [SerializeField]
    private Image slottedIcon;
    public Image SlottedImage { get => slottedIcon; set => slottedIcon = value; }
    [SerializeField]
    private Text slotKeyText;

    //cooldown
    [SerializeField] private GameObject cooldownVisualObj;
    [SerializeField] private Image cooldownFill;
    [SerializeField] private Text cooldownText;

    //charges / use amount
    [SerializeField] private Image usesCountImg;
    [SerializeField] private Text usesCountText;
    [SerializeField] private GameObject chargeCooldownVisualObj;
    [SerializeField] private Image chargeCooldownFill;

    //drag
    [SerializeField]
    private ActionBarObjectDrag actionBarDraggable;
    public ActionBarObjectDrag ActionBarObjectDrag { get => actionBarDraggable; }

    //slot trigger settings
    [SerializeField] private KeyCode slotKey;
    public KeyCode SlotKey { get => slotKey; }

    [SerializeField] private ActionSlotTriggerType actionSlotTriggerType;
    [SerializeField] private bool walkInRangeThenCast;

    private GameObject targetM1;

    public PlayerController playerController;
    public StatusEffectsManager statusEffectsManager;

    public IActionBarSlottable SlottedObject { get; private set; }
    public ICooldown SlottedObjectCooldown { get; private set; }

    void Awake() {
        ClearSlot();
        cooldownVisualObj.SetActive(false);
        cooldownFill.fillAmount = 1;
        chargeCooldownVisualObj.SetActive(false);
        chargeCooldownFill.fillAmount = 1;
        usesCountImg.enabled = false;
        usesCountText.enabled = false;

        GameSceneManager.UIPhase.ExecuteSync(Initialize, null, ExecuteAmount.Once);
    }

    private void Initialize() {
        OnSlotKeyChanged(slotKey);
    }

    public void OnSlotKeyChanged(KeyCode keyCode) {
        slotKey = keyCode;
        slotKeyText.text = InputManager.Instance.GetKeyCodeAsFormattedString(slotKey);
    }

    public bool FillSlot(IActionBarSlottable slottable, Sprite icon) {
        if (slottable == null) return false;

        ClearSlot();

        SlottedObject = slottable;

        slottedIcon.enabled = true;
        slottedIcon.sprite = icon;

        if(SlottedObject.GetMaxUsesAmount() > 1) {
            usesCountImg.enabled = true;
            usesCountText.text = SlottedObject.GetUsesAmount().ToString();
            usesCountText.enabled = true;
        }
        SlottedObject.OnUsesAmountChanged += UpdateUsesAmount;

        SlottedObject.AssignedKey = slotKey;
        SlottedObject.CheckForDistance = walkInRangeThenCast;

        SlottedObjectCooldown = slottable;
        if (SlottedObjectCooldown != null) {
            SlottedObjectCooldown.OnCooldownStart += OnCooldownStart;
            SlottedObjectCooldown.OnCooldownChanged += OnCooldownChanged;
            SlottedObjectCooldown.OnCooldownEnd += OnCooldownEnd;
        }

        if (actionSlotTriggerType == ActionSlotTriggerType.KeyDown) {
            InputManager.Instance.OnKeyDown += OnKeyEvent;
        } else if (actionSlotTriggerType == ActionSlotTriggerType.KeyHold) {
            InputManager.Instance.OnKeyDown += OnKeyEvent;
            InputManager.Instance.OnKeyHeld += OnKeyEvent;
        }

        ActionBar.Instance.onActionSlotStateChanged?.Invoke(this, SlotStateChanged.ObjectAdded);
        return true;
    }

    public void ClearSlot() {
        if (SlottedObject == null) { return; }

        CancleCastWalk(Vector3.zero);
        playerController.CanM1Move = true;

        if (SlottedObjectCooldown != null) {
            SlottedObjectCooldown.OnCooldownStart -= OnCooldownStart;
            SlottedObjectCooldown.OnCooldownChanged -= OnCooldownChanged;
            SlottedObjectCooldown.OnCooldownEnd -= OnCooldownEnd;
        }

        SlottedObject.OnUsesAmountChanged -= UpdateUsesAmount;

        SlottedObject.AssignedKey = KeyCode.None;
        SlottedObject.CheckForDistance = false;

        usesCountImg.enabled = false;
        usesCountText.enabled = false;

        SlottedObject = null;
        SlottedObjectCooldown = null;

        slottedIcon.enabled = false;
        slottedIcon.sprite = null;
       
        usesCountText.text = "";

        OnCooldownEnd(null);

        InputManager.Instance.OnKeyHeld -= OnKeyEvent;
        InputManager.Instance.OnKeyDown -= OnKeyEvent;

        ActionBar.Instance.onActionSlotStateChanged?.Invoke(this, SlotStateChanged.ObjectRemoved);
    }

    /// <summary>
    /// Clears slot only if the current slotted object is the same as object sent by parameter
    /// </summary>
    /// <param name="slottable">Object to clear</param>
    public void ClearSlot(IActionBarSlottable slottable) {
        if (SlottedObject == slottable) ClearSlot();
    }

    public void OnKeyEvent(KeyCode keyCode) {
        if (keyCode != slotKey) return;

        if (slotKey == KeyCode.Mouse0) {
            if (InputManager.Instance.IsKeyDown(KeyCode.Mouse0)) {
                targetM1 = TargetManager.CurrentPlayersEnemyTarget;
            }

            if (TargetManager.CurrentPlayersEnemyTarget == null || TargetManager.CurrentPlayersEnemyTarget != targetM1) {
                targetM1 = null;
                playerController.CanM1Move = true;
                return;
            }

            if (targetM1 == null) return;

            if (walkInRangeThenCast && SlottedObject.GetUseRange() > 0f) {
                if (statusEffectsManager.CanCast(null) && !SlottedObject.CanUse()) {
                    targetM1 = null;
                    return;
                }

                WalkInRangeThenUseCoroutine();
            } else {
                UseSlottable();
            }

        } else {
            if (walkInRangeThenCast && SlottedObject.GetUseRange() > 0f && SlottedObject.CanUse()) {
                WalkInRangeThenUseCoroutine();
            } else {
                UseSlottable();
            }
        }

        void WalkInRangeThenUseCoroutine() {
            Timing.KillCoroutines("WalkInRangeThenCastCoroutine");
            Timing.RunCoroutine(WalkInRangeThenUse(), "WalkInRangeThenCastCoroutine");
            playerController.OnDestinationChanged += CancleCastWalk;
        }

        void UseSlottable() {
            if (!SlottedObject.UseSlottable()) {
                //Debug.Log("You cannot use this right now.");
            }
        }
    }

    public void OnDrop(PointerEventData eventData) {
        if (eventData.selectedObject == null) return;
        DraggedObjectReference draggedObjectReference = eventData.selectedObject.GetComponent<DraggedObjectReference>();
        _ = FillSlot(draggedObjectReference?.draggedObject as IActionBarSlottable, draggedObjectReference?.draggedObjectImage.sprite);
        eventData.selectedObject = null;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if(SlottedObject != null) {
            AdvancedTooltip.Instance.ShowTooltip(SlottedObject.GetTooltip());
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        AdvancedTooltip.Instance.HideTooltip();
    }

    private void UpdateUsesAmount(int amount) {
        usesCountText.text = amount.ToString();

        if (usesCountImg.enabled == false && SlottedObject.GetMaxUsesAmount() > 1) {
            usesCountImg.enabled = true;
            usesCountText.enabled = true;
        }
    }

    private void OnCooldownStart(ICooldown iCd) {
        if (SlottedObjectCooldown.InCooldownUnusable || SlottedObject.GetUsesAmount() < 1) {
            cooldownVisualObj.SetActive(true);
            cooldownText.text = SlottedObjectCooldown.CurrentCooldown.ToString("N1");

            if (chargeCooldownVisualObj.activeSelf) chargeCooldownVisualObj.SetActive(false);
        } else {
            chargeCooldownVisualObj.SetActive(true);
            
            if(cooldownVisualObj.activeSelf) cooldownVisualObj.SetActive(false);
        }
    }

    private void OnCooldownChanged(ICooldown iCd) {
        if (SlottedObjectCooldown.InCooldownUnusable || SlottedObject.GetUsesAmount() < 1) {
            cooldownFill.fillAmount = SlottedObjectCooldown.CurrentCooldown / SlottedObjectCooldown.CurrentStartingCooldown;
            cooldownText.text = SlottedObjectCooldown.CurrentCooldown.ToString("N1");

            if (!cooldownVisualObj.activeSelf) cooldownVisualObj.SetActive(true);
            if (chargeCooldownVisualObj.activeSelf) chargeCooldownVisualObj.SetActive(false);
        } else {
            chargeCooldownFill.fillAmount = SlottedObjectCooldown.CurrentCooldown / SlottedObjectCooldown.CurrentStartingCooldown;

            if (!chargeCooldownVisualObj.activeSelf) chargeCooldownVisualObj.SetActive(true);
            if (cooldownVisualObj.activeSelf) cooldownVisualObj.SetActive(false);
        }
    }

    private void OnCooldownEnd(ICooldown iCd) {
        cooldownVisualObj.SetActive(false);
        cooldownFill.fillAmount = 1;
        chargeCooldownVisualObj.SetActive(false);
        chargeCooldownFill.fillAmount = 1;
    }

    private void CancleCastWalk(Vector3 newDestination) {
        Timing.KillCoroutines("WalkInRangeThenCastCoroutine");
        playerController.OnDestinationChanged -= CancleCastWalk;
        playerController.CanM1Move = true;
        if (SlottedObject == null) return;
        SlottedObject.UsePoint = Vector3.zero;
        SlottedObject.UseTarget = null;
    }

    private IEnumerator<float> WalkInRangeThenUse() {
        if (!statusEffectsManager.CanCast(null)) yield break;

        playerController.CanM1Move = false;

        GameObject target = TargetManager.CurrentPlayersEnemyTarget;
        if (target != null) {
            SlottedObject.UseTarget = target;
        } else {
            target = TargetManager.CurrentPlayersAllyTarget;
            if (target != null) {
                SlottedObject.UseTarget = target;
            }
        }

        Vector3 targetPoint = TargetManager.CurrentPlayersTargetPoint;

        SlottedObject.UsePoint = targetPoint;

        while (!SlottedObject.UseSlottable()) {
            if (target != null) SlottedObject.UsePoint = target.transform.position;

            if (SlottedObject.GetUseRange() <= 0f ||
                Vector3.Distance(playerController.transform.position, target == null ? targetPoint : target.transform.position) <= SlottedObject.GetUseRange()) {
                playerController.StopMovement();
            } else {
                playerController.MoveToPointExternal(target == null ? targetPoint : target.transform.position);
            }

            yield return Timing.WaitForOneFrame;
        }

        if (SlottedObject != null) {
            SlottedObject.UsePoint = Vector3.zero;
            SlottedObject.UseTarget = null;
        }

        playerController.OnDestinationChanged -= CancleCastWalk;
        playerController.CanM1Move = true;
    }
}
