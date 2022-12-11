using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour {
    #region Singleton
    public static Inventory Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        
        Items = new Item[space];
        ItemCategoriesInCooldown = new List<ItemCategoryInCooldown>();
    }
    #endregion

    public delegate void OnItemChanged(int arrayIndexChanged);
    public event OnItemChanged OnItemChangedCallback;

    [SerializeField] private int space = 40;

    public Item[] Items { get; private set; }
    public int ItemCount { get; private set; }

    //public List<Item> items { get; private set; }

    public List<ItemCategoryInCooldown> ItemCategoriesInCooldown { get; private set; }

    public bool Add(Item item) {
        if (item == null) return false;

        if (item is IConsumedOnPickup) {
            item.Use();
            return true;
        }

        int changeIndex = -1;

        Item stackableItem = Array.Find<Item>(Items, x => x != null && x.itemID == item.itemID && x.StackSize < x.stackAmountMax);

        if (stackableItem != null) {
            int totalStackCount = stackableItem.StackSize + item.StackSize;
            if (totalStackCount > stackableItem.stackAmountMax) {
                if (IsFull()) {
                    Debug.Log("Not enough room for more items.");
                    return false;
                }

                int extraStacks = totalStackCount - stackableItem.stackAmountMax;
                stackableItem.StackSize = stackableItem.stackAmountMax;

                Item overStackedItem = Instantiate(stackableItem);
                overStackedItem.StackSize = extraStacks; //set stacks before adding the copy of item with overstacked stack amount - possible infinite loop !!!

                _ = Add(overStackedItem);

            } else {
                stackableItem.StackSize += item.StackSize;
            }

            changeIndex = Array.FindIndex(Items, x => x == stackableItem);

        } else if (IsFull()) {
            Debug.Log("Not enough room for more items.");
            return false;
        } else {
            var result = Utils.AddToArrayOnNull(Items, item);
            changeIndex = result.arrayIndexChanged;
            ItemCount++;
            ApplyCooldownToItemInItemCooldownCategory(item.ApplyCooldown, item.ItemCooldownCategory);
            item.OnCooldownStart += SetItemsOfSameCooldownTypeIntoCooldown;
        }

        OnItemChangedCallback?.Invoke(changeIndex);

        return true;
    }

    /// <summary>
    /// Adds item into certain empty slot
    /// </summary>
    /// <param name="item"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool Add(Item item, int index) {
        bool added = false;
        int changeIndex = -1;

        if (Items[index] == null) {
            Items[index] = item;
            ItemCount++;

            ApplyCooldownToItemInItemCooldownCategory(item.ApplyCooldown, item.ItemCooldownCategory);
            item.OnCooldownStart += SetItemsOfSameCooldownTypeIntoCooldown;

            changeIndex = index;
            added = true;
        }

        if (Items[changeIndex] != null) AdvancedTooltip.Instance.ShowTooltip(Items[changeIndex].GetTooltip());

        OnItemChangedCallback?.Invoke(changeIndex);
        
        return added;
    }

    /// <summary>
    /// Swaps items in inventory based on their index in array, calls Merge that tries to merge the item stacks
    /// </summary>
    /// <param name="indexFrom">Index of Item in items array that is dragged into another slot</param>
    /// <param name="indexTo">Index of slot that the item is dropped at</param>
    /// <returns>true if swapped, false if could not swap</returns>
    public bool Swap(int indexFrom, int indexTo) {

        if (Items[indexFrom] == null) return false;

        Item swapItem = Items[indexTo];

        if (!Merge(indexFrom, indexTo)) {
            Items[indexTo] = Items[indexFrom];

            Items[indexFrom] = swapItem;
        }

        if (Items[indexTo] != null) AdvancedTooltip.Instance.ShowTooltip(Items[indexTo].GetTooltip());

        OnItemChangedCallback?.Invoke(indexFrom);
        OnItemChangedCallback?.Invoke(indexTo);
        return true;
    }

    public bool Merge(int mergableIndex, int mergeToIndex) {
        Item mergable = Items[mergableIndex];
        Item mergeTo = Items[mergeToIndex];

        if (mergable == null || mergeTo == null) return false;

        if (mergable.itemID == mergeTo.itemID && mergeTo.StackSize < mergeTo.stackAmountMax) {
            int totalStackCount = mergeTo.StackSize + mergable.StackSize;
            if (totalStackCount > mergeTo.stackAmountMax) {
                int remainingStacks = totalStackCount - mergeTo.stackAmountMax;
                mergeTo.StackSize = mergeTo.stackAmountMax;
                mergable.StackSize = remainingStacks;
            } else {
                mergeTo.StackSize += mergable.StackSize;
                mergable.StackSize = 0;
            }

            if(mergable.StackSize <= 0) {
                Remove(mergable);
            }

            return true;
        } 

        return false;
    }

    public bool Remove(Item item, int destackAmount = 0) {
        bool removed = false;

        Item itemToDestack = Array.Find(Items, x => x != null && x.GetInstanceID() == item.GetInstanceID());

        if (itemToDestack == null) return removed;

        if (itemToDestack.StackSize > 1) {
            if (destackAmount > 0 && itemToDestack.StackSize >= destackAmount) {
                //destacking item if destackAmount is greater than 0
                itemToDestack.StackSize -= destackAmount;

                if (itemToDestack.StackSize <= 0) {
                    Remove();
                } else {
                    //only callback invoke if destacked item still has StackSize greater than 0
                    OnItemChangedCallback?.Invoke(Array.FindIndex(Items, x => x == itemToDestack));

                    removed = false;
                }
            } else if (destackAmount <= 0) {
                //destacking item if destackAmount is not specified therefore 0
                itemToDestack.StackSize -= 1;
                OnItemChangedCallback?.Invoke(Array.FindIndex(Items, x => x == itemToDestack));

                removed = false;
            }
        } else {
            //removing item from inventory if StackSize is not greater than 1
            Remove();
        }

        return removed;

        void Remove() {
            item.OnCooldownStart -= SetItemsOfSameCooldownTypeIntoCooldown;
            var (wasRemoved, arrayIndexChanged) = Utils.RemoveFromArray(Items, item);
            ItemCount--;
            OnItemChangedCallback?.Invoke(arrayIndexChanged);

            removed = true;
        }
    }

    public bool Remove(int index, int destackAmount = 0) {
        bool removed = false;

        Item item = Items[index];

        if (item == null) return removed;

        if (destackAmount > 0 && item.StackSize >= destackAmount) {
            item.StackSize -= destackAmount;

            if (item.StackSize <= 0) {
                Remove();
            } else {
                //only callback invoke if destacked item still has StackSize greater than 0
                OnItemChangedCallback?.Invoke(index);

                removed = false;
            }

        } else if (item.StackSize > 1 && destackAmount == 0) {
            //destacking item if destackAmount is not specified therefore 0
            item.StackSize -= 1;
            OnItemChangedCallback?.Invoke(index);

            removed = false;
        } else {
            //removing item from inventory if StackSize is not greater than 1
            Remove();
        }

        return removed;

        void Remove() {
            item.OnCooldownStart -= SetItemsOfSameCooldownTypeIntoCooldown;
            _ = Utils.RemoveFromArray(Items, item);
            ItemCount--;
            OnItemChangedCallback?.Invoke(index);

            removed = true;
        }
    }

    public bool IsFull() {
        if (ItemCount == space) {
            return true;
        }
        return false;
    }

    public int GetItemInventoryIndex(Item item) {

        for (int i = 0; i < Items.Length; i++) {
            if (Items[i] == item) {
                return i;
            }
        }

        //if is not in inventory return -1
        return -1;
    }

    private void SetItemsOfSameCooldownTypeIntoCooldown(ICooldown iCd) {
        Item item = iCd as Item;
        if (item == null) return;
        if (item.ItemCooldownCategory == ItemCooldownCategory.None) return;

        if (!ItemCategoriesInCooldown.Any(x => x.cooldownCategory == item.ItemCooldownCategory)) {
            ItemCategoriesInCooldown.Add(new ItemCategoryInCooldown(item.ItemCooldownCategory, item.CurrentCooldown));
            ItemCategoriesInCooldown[ItemCategoriesInCooldown.Count - 1].OnCooldownEnd += RemoveItemCategoryInCooldown;
        }

        Item[] itemsOfSameCooldownType = Array.FindAll(Items, x => x != null && x.ItemCooldownCategory == item.ItemCooldownCategory);

        for (int i = 0; i < itemsOfSameCooldownType.Length; i++) {
            itemsOfSameCooldownType[i].ApplyCooldown(item.CurrentStartingCooldown);
        }
    }

    private void ApplyCooldownToItemInItemCooldownCategory(Action<float, float> applyCooldownMethod, ItemCooldownCategory cooldownCategory) {
        ItemCategoryInCooldown temp = ItemCategoriesInCooldown.Find(x => x != null && x.cooldownCategory == cooldownCategory);
        applyCooldownMethod(temp == null ? 0 : temp.CurrentCooldown, temp == null ? 0 : temp.CurrentStartingCooldown);
    }

    private void RemoveItemCategoryInCooldown(ICooldown icd) {
        icd.OnCooldownEnd -= RemoveItemCategoryInCooldown;
        ItemCategoriesInCooldown.Remove(icd as ItemCategoryInCooldown);
    }
}

[System.Serializable]
public class ItemCategoryInCooldown : ICooldown {
    public ItemCooldownCategory cooldownCategory;

    public Action<ICooldown> OnCooldownStart { get; set; }
    public Action<ICooldown> OnCooldownChanged { get; set; }
    public Action<ICooldown> OnCooldownEnd { get; set; }

    public float CurrentCooldown { get; set; }

    public float CurrentStartingCooldown { get; set; }
    [field: SerializeField] public bool InCooldownUnusable { get; set; } = true;

    public ItemCategoryInCooldown(ItemCooldownCategory cooldownCategory, float cooldown) {
        this.cooldownCategory = cooldownCategory;
        ApplyCooldown(cooldown);
    }

    public void ApplyCooldown(float cooldownTime, float startingCooldown = 0f) {
        if (cooldownTime <= CurrentCooldown) { return; }
        CurrentCooldown = cooldownTime;
        CurrentStartingCooldown = cooldownTime;
        CooldownManager.Instance.ProcessCooldown(this);
    }

    public bool HandleCooldown(float deltaTime) {
        CurrentCooldown -= deltaTime;
        if (CurrentCooldown < 0f) {
            CurrentCooldown = 0f;
            CurrentStartingCooldown = 0f;
            OnCooldownEnd?.Invoke(this);
            return true;
        }
        return false;
    }
}
