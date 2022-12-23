using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Fire DOT Debuff Properties", menuName = "Status Effects/Debuff Properties/Fire DOT Debuff Properties")]
public class FireDOTDebuffProperties : StatusEffectProperties {
    public StatFloat damagePerStack;

    public override void AssignReferences() {
        base.AssignReferences();
        damagePerStack.SetTooltipDirtyMethod = SetTooltipIsDirty;
    }

    protected override void BuildTooltipText() {
        base.BuildTooltipText();

        StringBuilder sb = new StringBuilder();
        sb.Append($"<size={DataStorage.DefaultStatusEffectNameFontSize}><b>").Append($"{name}").AppendLine("</b></size>");
        sb.AppendLine($"<size={DataStorage.DefaultStatusEffectDescriptionFontSize}>");
        sb.Append($"Burns enemies for {abilityDamage.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage)}" +
            $" initially + {damagePerStack.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage)} per stack");
        sb.Append(" of your base damage as ");
        AbilityExtensions.AppendDamageTypeTooltips(sb, DamageTypes);
        sb.AppendLine();
        sb.Append($"Lasts for {duration.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Seconds)}, refreshed upon re-application" +
            $" and can be stacked up to {maxStacks.GetValue()} stacks.");

        sb.AppendLineMultipleTimes();
        sb.AppendLine("Debuff strenght/protection affects tick damage.");
        AbilityExtensions.AppendBenefitTooltips(sb, this, false);

        sb.Append("</size>");
        abilityTooltip = sb;
    }

    protected override void BuildCombatTooltip(StatusEffect se) {
        base.BuildCombatTooltip(se);

        StringBuilder sb = new StringBuilder();
        sb.Append($"<size={DataStorage.DefaultStatusEffectNameFontSize}><b>").Append($"{name}").AppendLine("</b></size>");
        sb.AppendLine($"<size={DataStorage.DefaultStatusEffectDescriptionFontSize}>");
        Debuff debuff = se as Debuff;
        float value = ((abilityDamage.GetValue() + (damagePerStack.GetValue() * (se.CurrentStacks - 1))) * debuff.DebuffStrenghtModifier) * se.ApplierStatsContainer.DamageValue;
        sb.Append($"Taking {value.StatValueToStringByStatStringTypeNoSpace(StatStringType.Absolute)} damage as ");
        AbilityExtensions.AppendDamageTypeTooltips(sb, DamageTypes);
        sb.Append($" per {tickRate.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Seconds)}");
        sb.Append("</size>");
        combatTooltip = sb;
    }
}
