using CustomOutline;
using UnityEngine;

public class ItemPickUp : Interactable {
    [field: SerializeField] public Item Item { get; set; }

    [SerializeField] private Outline outline;

    bool callPostItemDrop = true;

    public override void Awake() {
        base.Awake();

        if (Item != null) {
            Item = Item.GetCopy();
            Item.PostItemWorldDrop();
            callPostItemDrop = false;
        }

        if (outline == null)
            if (TryGetComponent<Outline>(out outline)) outline.enabled = false;
    }

    public override void OnValidate() {
        base.OnValidate();

        Outline outline = GetComponent<Outline>();
        if (outline != null) {
            outline.OutlineColor = DataStorage.DefaultLootHolderOutlineColor;
            outline.OutlineWidth = DataStorage.DefaultLootHolderOutlineWidth;
        }
    }

    public override void OnEnable() {
        base.OnEnable();
        if (Item != null) {
            if (callPostItemDrop) Item.PostItemWorldDrop();
        }
    }

    public override void OnDisable() {
        base.OnDisable();
        callPostItemDrop = true;
    }

    public override void Interact() {
        PickUp();
    }

    void PickUp() {
        if (Inventory.Instance.Add(Item)) {
            ObjectPoolManager.Instance.PoolObjectBack(name, gameObject);
            Tooltip.HideTooltip();
            Item = null;
        }
    }

    public void LoadItem(Item item) {
        this.Item = item;
    }

    public override void OnSelected() {
        if (outline != null) outline.enabled = true;

        if (Tooltip == null || Item == null) return;
        Tooltip.ShowTooltip(Item.name, Item.StackSize, Item.itemRarity.ItemRarityToColor32());
    }

    public override void OnDeselected() {
        if (outline != null) outline.enabled = false;

        if (Tooltip == null) return;
        Tooltip.HideTooltip();
    }
}
