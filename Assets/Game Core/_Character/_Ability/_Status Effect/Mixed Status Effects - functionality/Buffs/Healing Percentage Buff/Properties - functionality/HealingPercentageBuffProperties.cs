using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Healing Percentage Buff Properties", menuName = "Status Effects/Buff Properties/Healing Percentage Buff Properties")]
public class HealingPercentageBuffProperties : StatusEffectProperties {
    public StatFloat percentHealthPerTick;
    [SerializeField] private bool affectedByHealingEffectivity = true;
    public bool AffectedByHealingEffectivity { get => affectedByHealingEffectivity; }

    public override void AssignReferences() {
        base.AssignReferences();
        percentHealthPerTick.SetTooltipDirtyMethod = SetTooltipIsDirty;
    }

    protected override void BuildTooltipText() {
        base.BuildTooltipText();

        StringBuilder sb = new StringBuilder();
        sb.Append($"<size={DataStorage.DefaultStatusEffectNameFontSize}><b>").Append($"{name}").AppendLine("</b></size>");
        sb.AppendLine($"<size={DataStorage.DefaultStatusEffectDescriptionFontSize}>");
        sb.AppendLine($"Heals you for {percentHealthPerTick.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage)}" +
            $" of your max. health per {tickRate.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.PerSecond)}.");
        sb.Append($"Lasts for {duration.GetValue()}s.");

        sb.AppendLineMultipleTimes();

        if (affectedByHealingEffectivity) {
            sb.AppendLine("Affected by healing effectivity.");
        } else {
            sb.AppendLine("Not affected by healing effectivity.");
        }

        sb.Append("Cannot be stacked, can be refreshed.");
        sb.Append("</size>");
        abilityTooltip = sb;
    }

    protected override void BuildCombatTooltip(StatusEffect se) {
        base.BuildCombatTooltip(se);

        combatTooltip = GetTooltip();
    }
}
