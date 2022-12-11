using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Teleport Properties", menuName = "Skill/Teleport Properties")]
public class TeleportProperties : SkillProperties
{
    [Range(0, 0.99f)] public float rangeTolerance = 0.8f;

    protected override void BuildTooltipText() {
        base.BuildTooltipText();

        StringBuilder sb = new StringBuilder();
        sb.Append($"<size=35><b>").Append($"{name}").AppendLine("</b></size>");
        sb.Append($"<size=20>{SkillCastSpeedScalingType}, ");
        AbilityExtensions.AppendSkillTypes(sb, SkillTypes);
        sb.AppendLine();
        sb.AppendLine($"Teleports you to desired location within {maxCastRange.GetValue().StatValueToStringByStatStringType(StatStringType.Absolute)} meters range.");
        sb.AppendLine();
        sb.AppendLine("If you cannot be teleported to chosen location, teleport will search");
        sb.AppendLine($"for location in desired direction within 99 - {rangeTolerance.StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage, 0)} of the total range.");

        AbilityExtensions.AppendChargesTooltips(sb, this.chargeSystem, cooldown.GetValue());

        AbilityExtensions.AppendBuffDescriptionNoStart(sb, buffHolder, SEApplicationStringStyle.OnCastEnd, "movement speed", buffHolder[0].stacksToApply.GetValue().ToString());

        sb.AppendLine();
        sb.AppendLine($"Cast time: {castTime.GetValue().StatValueToStringByStatStringType(StatStringType.Absolute, 2)} s");
        sb.AppendLine($"Mana cost: {manaCost.GetValue().StatValueToStringByStatStringType(StatStringType.Absolute, 0)}");
        sb.Append($"Cooldown: {cooldown.GetValue().StatValueToStringByStatStringType(StatStringType.Absolute, 2)} s");
        sb.Append("</size>");
        abilityTooltip = sb;
    }
}
