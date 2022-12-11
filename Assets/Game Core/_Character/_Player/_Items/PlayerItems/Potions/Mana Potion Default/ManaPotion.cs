using UnityEngine;
using System.Text;

[CreateAssetMenu(fileName = "New Mana Potion", menuName = "Inventory/Mana Potion")]
public class ManaPotion : Potion {

    [SerializeField, Range(0f, 1f)] private float potionManaRestoration;
    
    public override void Awake() {
        base.Awake();
    }

    public override bool Use() {
        if (!CanUse()) return false;

        characterCombat = TargetManager.PlayerComponent.CharacterCombat;
        if (characterCombat.MyCharacterStats.CurrentMana < characterCombat.MyCharacterStats.CoreStats.ManaValue * (1 - potionManaRestoration)) {
            characterCombat.RestoreMana(characterCombat.MyCharacterStats.CoreStats.ManaValue * potionManaRestoration);
            _ = base.Use();
            _ = RemoveFromInventoryOrDestack();
            return true;
        } else {
            Debug.Log("Usage of this item is currently ineffective.");
            return false;
        }
    }

    public override void BuildTooltipText() {
        StringBuilder sb = new StringBuilder();

        ItemExtensions.AppendCoreItemDetails(sb, this);

        sb.AppendLine();
        sb.AppendLine($"Restores {potionManaRestoration.StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage, 0)} of your maximum mana points.");
        sb.AppendLine();

        sb.Append($"Cooldown: {itemCooldown.StatValueToStringByStatStringType(StatStringType.Absolute)} s");

        itemTooltip = sb;
    }
}
