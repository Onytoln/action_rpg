using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct ItemIconSetup {
    public ItemRarity itemRarity;
    public Sprite border;
    public Sprite background;

    public ItemIconSetup(ItemRarity ir, Sprite border, Sprite background) {
        itemRarity = ir;
        this.border = border;
        this.background = background;
    }

    public ItemIconSetup None { get => new ItemIconSetup(ItemRarity.Common, null, null); }

}
