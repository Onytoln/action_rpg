using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Fireball Properties", menuName = "Skill/Fireball Properties")]
public class FireballProperties : ProjectileSkillProperties {
    public string explosionHitInforId;

    public StatFloat explosionRadius;
    public StatFloat explosionDamage;
    [SerializeField]
    private List<DamageTypeWeight> explosionDamageTypes = new List<DamageTypeWeight>();
    public List<DamageTypeWeight> ExplosionDamageTypes {
        get => explosionDamageTypes;
    }

    public override AbilityPropertiesValuesContainer GetValuesCopy() {
        return new FireballPropertiesValuesContainer(this);
    }

    public override void AddDamageType(DamageType modifyTo, float value) {
        base.AddDamageType(modifyTo, value);

        CalculateAddedDamageType(ExplosionDamageTypes, modifyTo, value);
    }

    public override void RemoveDamageType( DamageType typeToRemove) {
        base.RemoveDamageType(typeToRemove);

        CalculateRemovedDamageType(ExplosionDamageTypes, typeToRemove);
    }

    public override void CheckProperties() {
        base.CheckProperties();
        if (string.IsNullOrEmpty(explosionHitInforId)) {
            Debug.LogError($"Ability {nameof(explosionHitInforId)} of {ToString()} is null or empty!");
        }
    }
    protected override void BuildTooltipText() {
        base.BuildTooltipText();

        StringBuilder sb = new StringBuilder();
        sb.Append($"<size={DataStorage.DefaultSkillNameFontSize}><b>").Append($"{name}").AppendLine("</b></size>");
        sb.Append($"<size={DataStorage.DefaultSkillDescriptionFontSize}>{SkillCastSpeedScalingType}, {ProjectileFireType}, ");
        AbilityExtensions.AppendSkillTypes(sb, SkillTypes);
        sb.AppendLine();
        sb.AppendLine("Hurl a ball of fire at the enemy.");
        sb.AppendLine();
        float currentDamage = CharacterComponent.CharacterStats.CoreStats.DamageValue;
        sb.Append($"On contact, deals {abilityDamage.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage, 0)} " +
            $"of your base damage ({(currentDamage * abilityDamage.GetValue()).StatValueToStringByStatStringType(StatStringType.Absolute, 0)}) as ");
        AbilityExtensions.AppendDamageTypeTooltips(sb, DamageTypes);
       
        sb.Append($"and then explodes, dealing {explosionDamage.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage, 0)}" +
            $" of your base damage ({(currentDamage * explosionDamage.GetValue()).StatValueToStringByStatStringType(StatStringType.Absolute, 0)})" +
            $" in {explosionRadius.GetValue()} meters radius as ");
        AbilityExtensions.AppendDamageTypeTooltips(sb, ExplosionDamageTypes);

        sb.AppendLine();
        AbilityExtensions.AppendBenefitTooltips(sb, this);

        AbilityExtensions.AppendChargesTooltips(sb, this.chargeSystem, cooldown.GetValue());

        AbilityExtensions.AppendDebuffDescriptionNoStart(sb, this.debuffHolder,SEApplicationStringStyle.OnHitTwice,
            "burn", debuffHolder[0].stacksToApply.GetValue().ToString(), "explosion hit", debuffHolder[0].stacksToApply.GetValue().ToString());

        sb.AppendLine();
        sb.AppendLine("Animations of this ability scale with attack speed.");
        AbilityExtensions.AppendEndCore(sb, this);

        sb.Append("</size>");
        abilityTooltip = sb;
    }
}
