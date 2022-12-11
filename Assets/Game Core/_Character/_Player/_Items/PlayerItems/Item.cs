using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject, IActionBarSlottable, IItemCooldown {
    public bool IsCopy { get; private set; } = false;

    public event Action OnItemRemoval;
    public event Action<Item> OnItemRemovalItemData;

    public string itemID;
    new public string name = "New Item";
    public Sprite icon = null;
    public int stackAmountMax = 1;
    [SerializeField] protected int stackSize = 1;

    public Action onIdentified;
    [SerializeField] private bool isIdentified = true;

    [SerializeField] private ItemPickUp lootHolder;
    public ItemPickUp LootHolder { get => lootHolder; }

    public bool IsIdentified {
        get => isIdentified;
        set {
            if (isIdentified != value) {
                isIdentified = value;
            }

            if (isIdentified == true) SetTooltipDirty();

            onIdentified?.Invoke();
        }
    }

    public int StackSize {
        get => stackSize;
        set {
            stackSize = value;
            SetTooltipDirty();
            if (AdvancedTooltip.Instance.isActiveAndEnabled && AdvancedTooltip.Instance.CompareShownTooltip(itemTooltip)) {
                AdvancedTooltip.Instance.ShowTooltip(GetTooltip());
            }

            OnUsesAmountChanged?.Invoke(stackSize);
        }
    }

    public ItemRarity itemRarity;
    [SerializeField]
    protected float itemCooldown = 0;
    [SerializeField]
    private ItemCooldownCategory itemCooldownCategory = ItemCooldownCategory.None;

    //Action bar properties
    public KeyCode AssignedKey { get; set; }
    [field: SerializeField] public bool CheckForDistance { get; set; } = true;
    public Vector3 UsePoint { get; set; }
    public GameObject UseTarget { get; set; }

    public Action<int> OnUsesAmountChanged { get; set; }

    //Cooldown properties
    public Action<ICooldown> OnCooldownStart { get; set; }
    public Action<ICooldown> OnCooldownChanged { get; set; }
    public Action<ICooldown> OnCooldownEnd { get; set; }

    public float CurrentCooldown { get; private set; }

    public float CurrentStartingCooldown { get; private set; }
    public bool InCooldownUnusable { get; set; } = true;

    public ItemCooldownCategory ItemCooldownCategory { get => itemCooldownCategory; }

    protected StringBuilder itemTooltip;
    protected bool isDirtyTooltip = true;

    public virtual void Awake() {
        if (string.IsNullOrEmpty(itemID)) {
            Debug.LogError($"{nameof(itemID)} of type: {GetType()}, name: {name} is empty!");
        }
    }


    public virtual bool Use() {
        ApplyCooldown(itemCooldown);
        return true;
    }

    public virtual void OnItemClickOperation(ItemClickOperation itemClickOperation) { }

    public virtual void LootTableManagerInit(List<LootParameter> lootParameters) {
        if(Utils.GetOfTypeFromList(lootParameters, out ItemLootParameter result)) {
            itemRarity = result.ItemRarity;
        }
    }

    public virtual void PostItemWorldDrop() { }

    public virtual bool CanUse() {
        if (CurrentCooldown <= 0f) return true;
        return false;
    }

    public int GetUsesAmount() {
        return stackSize;
    }

    public int GetMaxUsesAmount() {
        return stackAmountMax;
    }

    public bool RemoveFromInventoryOrDestack(int destackAmount = 0) {
        if (Inventory.Instance.Remove(this, destackAmount)) {
            AdvancedTooltip.Instance.HideTooltip();
            OnItemRemoval?.Invoke();
            OnItemRemovalItemData?.Invoke(this);
            return true;
        }
        return false;
    }

    public virtual void OnDestroy() { }

    public bool DropFromInventory() {
        //on item removal event
        //drop manager call
        return false;
    }

    public Item GetCopy() {
        Item copy = Instantiate(this);
        copy.IsCopy = true;
        return copy;
    }

    public bool UseSlottable() {
        return Use();
    }

    public bool CanUse(bool overrideDistanceCheck = true) {
        bool prevState = CheckForDistance;
        bool result = false;

        if (overrideDistanceCheck) CheckForDistance = false;

        if (CanUse()) result = true;

        CheckForDistance = prevState;

        return result;
    }

    public virtual void BuildTooltipText() { }

    public System.Text.StringBuilder GetTooltip() {
        if (isDirtyTooltip) {
            isDirtyTooltip = false;
            BuildTooltipText();
        }

        return itemTooltip;
    }

    protected void SetTooltipDirty() {
        isDirtyTooltip = true;
    }

    public void ApplyCooldown(float cooldownTime, float startingCooldown = 0) {
        if (cooldownTime <= CurrentCooldown) { return; }
        CurrentCooldown = cooldownTime;
        CurrentStartingCooldown = startingCooldown == 0f ? cooldownTime : startingCooldown;
        CooldownManager.Instance.ProcessCooldown(this);
        OnCooldownStart?.Invoke(this);
    }

    public bool HandleCooldown(float deltaTime) {
        CurrentCooldown -= deltaTime;
        OnCooldownChanged?.Invoke(this);
        if (CurrentCooldown < 0) {
            CurrentCooldown = 0;
            CurrentStartingCooldown = 0;
            OnCooldownEnd?.Invoke(this);
            return true;
        }
        return false;
    }

    public virtual float GetUseRange() {
        return stackSize;
    }
}