using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Mana Percentage Restoration Buff Properties", menuName = "Status Effects/Buff Properties/Mana Percentage Restoration Buff Properties")]
public class ManaRestorePercentageBuffProperties : StatusEffectProperties
{
    public StatFloat percentManaPerTick;

    protected override void BuildTooltipText() {
        base.BuildTooltipText();

        StringBuilder sb = new StringBuilder();
        sb.Append($"<size={DataStorage.DefaultStatusEffectNameFontSize}><b>").Append($"{name}").AppendLine("</b></size>");
        sb.AppendLine($"<size={DataStorage.DefaultStatusEffectDescriptionFontSize}>");
        sb.AppendLine($"Restores {percentManaPerTick.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage)}" +
            $" of your max. mana per {tickRate.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.PerSecond)}.");
        sb.Append($"Lasts for {duration.GetValue()}s.");

        sb.AppendLineMultipleTimes();

        sb.Append("Cannot be stacked, can be refreshed.");
        sb.Append("</size>");
        abilityTooltip = sb;
    }

    protected override void BuildCombatTooltip(StatusEffect se) {
        base.BuildCombatTooltip(se);

        combatTooltip = GetTooltip();
    }
}
