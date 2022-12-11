using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability Stat Boost Indicator Properties", menuName = "Status Effects/Status Effect Indicators/Ability Stat Boost Indicator Properties")]
public class AbilityStatBoostIndicatorProperties : StatusEffectProperties {

    public override void OnValidate() {
        base.OnValidate();

        Permanent = true;
        SurvivesDeath = true;
        SurvivesSceneLoad = true;
    }

    protected override void BuildCombatTooltip(StatusEffect se) {
        base.BuildCombatTooltip(se);

        AbilityStatBoostIndicator asbi = se as AbilityStatBoostIndicator;

        StringBuilder sb = new StringBuilder();
        sb.Append($"<size={DataStorage.DefaultStatusEffectNameFontSize}><b>").Append($"{name}").AppendLine("</b></size>");
        sb.AppendLine($"<size={DataStorage.DefaultStatusEffectDescriptionFontSize}>");
        sb.Append(string.Format(asbi.BuffText, asbi.BoostStatName, asbi.BoostValue.StatValueToStringByStatStringTypeNoSpace(asbi.StatStringType, 3)));
        sb.Append("</size>");
        combatTooltip = sb;
    }
}
