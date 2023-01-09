using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Generic Single Stat Boost Buff Properties", menuName = "Status Effects/Buff Properties/Generic Single Stat Boost Buff Properties")]
public class GenericSingleStatBoostBuffProperties : StatusEffectProperties {
    public StatFloat statBoostValue;

    [SerializeField] private CharacterStatType statToBoost;
    public CharacterStatType StatToBoost { get => statToBoost; }

    [SerializeField] private StatValueType statAddType;
    public StatValueType StatAddType { get => statAddType; }

    protected override void BuildTooltipText() {
        base.BuildTooltipText();

        StringBuilder sb = new StringBuilder();
        sb.Append($"<size={DataStorage.DefaultStatusEffectNameFontSize}><b>").Append($"{name}").AppendLine("</b></size>");
        sb.AppendLine($"<size={DataStorage.DefaultStatusEffectDescriptionFontSize}>");
        sb.Append($"Increases your {statToBoost.StatTypeToReadableString()} by ");
        switch (statAddType) {
            case StatValueType.Absolute:
                sb.Append($"{statBoostValue.GetValue().StatValueToStringByStatStringTypeNoSpace(statToBoost.StatTypeToStatStringType())} (Absolute)");
                break;
            case StatValueType.Relative:
                sb.Append($"{(statBoostValue.GetValue() * 100f).StatValueToStringByStatStringTypeNoSpace(StatStringType.Absolute)}% (Relative)");
                break;
            case StatValueType.Collective:
                sb.Append($"{(statBoostValue.GetValue() * 100f).StatValueToStringByStatStringTypeNoSpace(StatStringType.Absolute)}% (Total)");
                break;
        }

        sb.Append($" for {duration.GetValue().StatValueToStringByStatStringType(StatStringType.Absolute)}s.");
        
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
