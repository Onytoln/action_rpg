using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Magic Missile Properties", menuName = "Skill/Magic Missile Properties")]
public class MagicMissileProperties : ProjectileSkillProperties {
    public SkillStat cooldownOnHit;

    public SkillStat damageBonusByCooldown;
    public SkillStat cooldownForMaxDamageBonus;

    public override AbilityPropertiesValuesContainer GetValuesCopy() {
        return new MagicMissilePropertiesValuesContainer(this);
    }

    public override void AssignReferences() {
        base.AssignReferences();
        cooldownOnHit.SetTooltipDirtyMethod = SetTooltipIsDirty;
        damageBonusByCooldown.SetTooltipDirtyMethod = SetTooltipIsDirty;
        cooldownForMaxDamageBonus.SetTooltipDirtyMethod = SetTooltipIsDirty;
    }


    protected override void BuildTooltipText() {
        base.BuildTooltipText();

        StringBuilder sb = new StringBuilder();
        sb.Append($"<size={DataStorage.DefaultSkillNameFontSize}><b>").Append($"{name}").AppendLine("</b></size>");
        sb.Append($"<size={DataStorage.DefaultSkillDescriptionFontSize}>{SkillCastSpeedScalingType}, {ProjectileFireType}, ");
        AbilityExtensions.AppendSkillTypes(sb, SkillTypes);
        sb.AppendLine();
        sb.AppendLine("Conjure a magical missile.");
        sb.AppendLine();
        float currentDamage = CharacterComponent.CharacterStats.CoreStats.DamageValue;
        sb.Append($"On hit applies additional {cooldownOnHit.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Seconds, 3)} cooldown and" +
            $" deals {abilityDamage.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage, 0)} " +
            $"of your base damage ({(currentDamage * abilityDamage.GetValue()).StatValueToStringByStatStringType(StatStringType.Absolute, 0)}) as ");
        AbilityExtensions.AppendDamageTypeTooltips(sb, DamageTypes);
        sb.AppendLine();
        sb.Append($"This ability gains up to {damageBonusByCooldown.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage)} (relative) bonus damage" +
            $" on maximum of {cooldownForMaxDamageBonus.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Seconds)} cooldown or more.");

        sb.AppendLine();
        AbilityExtensions.AppendBenefitTooltips(sb, this);

        AbilityExtensions.AppendChargesTooltips(sb, this.chargeSystem, cooldown.GetValue());

        AbilityExtensions.AppendBuffDescriptionNoStart(sb, buffHolder, SEApplicationStringStyle.OnHit,
            "projectile enchantment", buffHolder[0].stacksToApply.GetValue().ToString());

        sb.AppendLine();
        sb.AppendLine("Animations of this ability scale with attack speed.");
        AbilityExtensions.AppendEndCore(sb, this);

        sb.Append("</size>");
        abilityTooltip = sb;
    }
}
