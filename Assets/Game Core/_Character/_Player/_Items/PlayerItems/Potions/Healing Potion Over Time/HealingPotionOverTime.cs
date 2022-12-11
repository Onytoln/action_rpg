using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "New Healing Potion Over Time", menuName = "Inventory/Healing Potion Over Time")]
public class HealingPotionOverTime : Potion {

    [SerializeField, Range(0f, 1f)] private float potionHealthRestoration;
    [SerializeField] private BuffHolder healingBuffHolder;

    public override void PostItemWorldDrop() {
        base.PostItemWorldDrop();
        healingBuffHolder.Initialize(SetTooltipDirty);
    }

    public override void OnDestroy() {
        base.OnDestroy();
        healingBuffHolder.OnDestroy();
    }

    public override bool Use() {
        if (!CanUse()) return false;

        characterCombat = TargetManager.PlayerComponent.CharacterCombat;
        if (characterCombat.MyCharacterStats.CurrentHealth < characterCombat.MyCharacterStats.CoreStats.HealthValue * (1 - potionHealthRestoration)) {
            characterCombat.RestoreHealthPercentage(potionHealthRestoration, true);
            _ = characterCombat.GetStatusEffectApplied(healingBuffHolder.buffToApply, TargetManager.PlayerComponent, 
                TargetManager.PlayerComponent.CharacterStats.CoreStats.GetStatsValuesCopy(), healingBuffHolder.stacksToApply.GetValue());
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
        sb.AppendLine($"Heals you for {potionHealthRestoration.StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage, 0)}" +
            $" of your maximum health points instantly.");
        sb.AppendLine($"Then applies a healing buff: ");

        AbilityExtensions.AppendBuffDescription(sb, healingBuffHolder);

        sb.AppendLine();

        sb.Append($"Cooldown: {itemCooldown.StatValueToStringByStatStringType(StatStringType.Absolute)} s");

        itemTooltip = sb;
    }
}
