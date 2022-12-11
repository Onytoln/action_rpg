using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "New Healing Potion", menuName = "Inventory/Healing Potion")]
public class HealingPotion : Potion {

    [SerializeField, Range(0f, 1f)] private float potionHealthRestoration;

    public override void Awake() {
        base.Awake();
    }

    public override bool Use() {
        if (!CanUse()) return false;

        characterCombat = TargetManager.PlayerComponent.CharacterCombat;
        if (characterCombat.MyCharacterStats.CurrentHealth < characterCombat.MyCharacterStats.CoreStats.HealthValue * (1 - potionHealthRestoration)) {
            characterCombat.RestoreHealth(characterCombat.MyCharacterStats.CoreStats.HealthValue * potionHealthRestoration, true);
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
        sb.AppendLine($"Heals you for {potionHealthRestoration.StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage, 0)} of your maximum health points.");
        sb.AppendLine();

        sb.Append($"Cooldown: {itemCooldown.StatValueToStringByStatStringType(StatStringType.Absolute)} s");

        itemTooltip = sb;
    }
}
