using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Frost Debuff Properties", menuName = "Status Effects/Debuff Properties/Frost Debuff Properties")]
public class FrostDebuffProperties : StatusEffectProperties {
    public StatFloat attackSpeedSlowAmount;
    public StatFloat movementSpeedSlowAmount;
    public StatFloat healingEffectivityDecrease;
    public StatFloat iceResistanceProtectionModifier = new StatFloat(.3f, 0f, 1f);

    protected override void BuildTooltipText() {
        base.BuildTooltipText();

        StringBuilder sb = new StringBuilder();
        sb.Append($"<size={DataStorage.DefaultStatusEffectNameFontSize}><b>").Append($"{name}").AppendLine("</b></size>");
        sb.AppendLine($"<size={DataStorage.DefaultStatusEffectDescriptionFontSize}>");
        sb.Append($"Reduces target attack speed by {attackSpeedSlowAmount.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage)} (relative) " +
            $", movement speed by {movementSpeedSlowAmount.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage)} (relative) " +
            $"and healing effectivity by {healingEffectivityDecrease.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage)} (absolute) per stack.");
        sb.AppendLine();
        sb.Append($"Lasts for {duration.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.PerSecond)}, refreshed upon re-application" +
            $" and can be stacked up to {maxStacks.GetValue()} stacks.");
        sb.AppendLineMultipleTimes();
        sb.AppendLine("Debuff strenght/protection affects attack speed, movement speed and healing effectivity reduction amount.");
        sb.Append($"Attack speed and movement speed reductions are also decreased by Ice resistance by" +
            $" {iceResistanceProtectionModifier.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage)} of it's value.");

        sb.Append("</size>");
        abilityTooltip = sb;
    }

    protected override void BuildCombatTooltip(StatusEffect se) {
        base.BuildCombatTooltip(se);

        StringBuilder sb = new StringBuilder();
        sb.Append($"<size={DataStorage.DefaultStatusEffectNameFontSize}><b>").Append($"{name}").AppendLine("</b></size>");
        sb.AppendLine($"<size={DataStorage.DefaultStatusEffectDescriptionFontSize}>");
        FrostDebuff fd = se as FrostDebuff;
        sb.Append($"Reducing your Attack Speed by {fd.AddedAttackSpeedSlow.StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage)} (relative)," +
            $" Movement Speed by {fd.AddedMovementSpeedSlow.StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage)} (relative) and" +
            $" Healing Effectivity by {fd.AddedHealingEffectivityReduction.StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage)} (absolute).");
        sb.Append("</size>");
        combatTooltip = sb;
    }
}
