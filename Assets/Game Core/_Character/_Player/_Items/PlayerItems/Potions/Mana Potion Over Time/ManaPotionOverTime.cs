using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mana Restoration Potion Over Time", menuName = "Inventory/Mana Restoration Potion Over Time")]
public class ManaPotionOverTime : Potion
{
    [SerializeField, Range(0f, 1f)] private float potionManaRestoration;
    [SerializeField] private BuffHolder manaRestorationBuffHolder;

    public override void PostItemWorldDrop() {
        base.PostItemWorldDrop();
        manaRestorationBuffHolder.Initialize(SetTooltipDirty);
    }

    public override void OnDestroy() {
        base.OnDestroy();
        manaRestorationBuffHolder.OnDestroy();
    }
    public override bool Use() {
        if (!CanUse()) return false;

        characterCombat = TargetManager.PlayerComponent.CharacterCombat;
        if (characterCombat.MyCharacterStats.CurrentMana < characterCombat.MyCharacterStats.CoreStats.ManaValue * (1 - potionManaRestoration)) {
            characterCombat.RestoreManaPercetage(potionManaRestoration);
            _ = characterCombat.GetStatusEffectApplied(manaRestorationBuffHolder.buffToApply,
                TargetManager.PlayerComponent, TargetManager.PlayerComponent.CharacterStats.CoreStats.GetCurrentStatsValuesCopy(), manaRestorationBuffHolder.stacksToApply.GetValue());
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
        sb.AppendLine($"Restores {potionManaRestoration.StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage, 0)}" +
            $" of your maximum mana instantly.");
        sb.AppendLine($"Then applies a mana restoration buff: ");

        AbilityExtensions.AppendBuffDescription(sb, manaRestorationBuffHolder);

        sb.AppendLine();

        sb.Append($"Cooldown: {itemCooldown.StatValueToStringByStatStringType(StatStringType.Absolute)} s");

        itemTooltip = sb;
    }
}
