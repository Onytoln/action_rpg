using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Frost Charge Properties", menuName = "Skill/Frost Charge Properties")]
public class FrostChargeProperties : ChargeSkillProperties {
    public string postChargeExplosionHitInforId;

    public StatFloat postChargeExplosionRadius;
    public StatFloat postChargeExplosionDamage;
    [SerializeField]
    private List<DamageTypeWeight> postChargeExplosionDamageTypes = new List<DamageTypeWeight>();
    public List<DamageTypeWeight> PostChargeExplosionDamageTypes {
        get => postChargeExplosionDamageTypes;
    }

    public override AbilityPropertiesValuesContainer GetValuesCopy() {
        return new FrostChargePropertiesValuesContainer(this);
    }

    public override void AddDamageType(DamageType modifyTo, float value) {
        base.AddDamageType(modifyTo, value);

        CalculateAddedDamageType(PostChargeExplosionDamageTypes, modifyTo, value);
    }

    public override void RemoveDamageType(DamageType typeToRemove) {
        base.RemoveDamageType(typeToRemove);

        CalculateRemovedDamageType(PostChargeExplosionDamageTypes, typeToRemove);
    }

    public override void AssignReferences() {
        base.AssignReferences();
        postChargeExplosionRadius.SetTooltipDirtyMethod = SetTooltipIsDirty;
        postChargeExplosionDamage.SetTooltipDirtyMethod = SetTooltipIsDirty;
    }

    public override void CheckProperties() {
        base.CheckProperties();
        if (string.IsNullOrEmpty(postChargeExplosionHitInforId)) {
            Debug.LogError($"Ability {nameof(postChargeExplosionHitInforId)} of {ToString()} is null or empty!");
        }
    }

    protected override void BuildTooltipText() {
        base.BuildTooltipText();

        StringBuilder sb = new StringBuilder();
        sb.Append($"<size=35><b>").Append($"{name}").AppendLine("</b></size>");
        sb.Append($"<size=20>{SkillCastSpeedScalingType}");
        AbilityExtensions.AppendSkillTypes(sb, SkillTypes);
        sb.AppendLine();
        sb.AppendLine("Charge to a desired location enveloped in a frost aura that damages enemies.");
        sb.AppendLine();
        float currentDamage = CharacterComponent.CharacterStats.CoreStats.DamageValue;
        sb.Append($"On contact with enemy, deals {abilityDamage.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage, 0)} " +
            $"of your base damage ({(currentDamage * abilityDamage.GetValue()).StatValueToStringByStatStringType(StatStringType.Absolute, 0)}) as ");
        AbilityExtensions.AppendDamageTypeTooltips(sb, DamageTypes);
        sb.AppendLine(" and after landing explodes,");
        sb.Append($"dealing {postChargeExplosionDamage.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage, 0)}" +
            $" of your base damage ({(currentDamage * postChargeExplosionDamage.GetValue()).StatValueToStringByStatStringType(StatStringType.Absolute, 0)})" +
            $" in {postChargeExplosionRadius.GetValue()} meters radius as ");
        AbilityExtensions.AppendDamageTypeTooltips(sb, PostChargeExplosionDamageTypes);
        sb.AppendLine(".");
        sb.AppendLine();

        sb.AppendLine($"Charge speed: {ChargeSpeed.GetValue().StatValueToStringByStatStringType(StatStringType.MovementSpeed)}," +
            $" Max. charge duration: {ChargeDuration.GetValue().StatValueToStringByStatStringType(StatStringType.Absolute)} s," +
            $" Charge radius: {Scale.GetValue().StatValueToStringByStatStringType(StatStringType.Absolute)} m");
        if (PiercesAllTargets.Value) {
            sb.AppendLine("Pierces all enemies.");
        } else {
            sb.AppendLine($"After piercing {MaxPierceCount.GetValue()} enemies, charge stops.");
        }
        sb.Append("Charge stops immediately if you have unwalkable terrain in front of you or you collide with terrain.");

        sb.AppendLine();
        AbilityExtensions.AppendBenefitTooltips(sb, this);

        AbilityExtensions.AppendChargesTooltips(sb, this.chargeSystem, cooldown.GetValue());

        AbilityExtensions.AppendDebuffDescriptionNoStart(sb, this.debuffHolder, SEApplicationStringStyle.OnHitTwice,
        "frost", debuffHolder[0].stacksToApply.GetValue().ToString(), "explosion hit", debuffHolder[1].stacksToApply.GetValue().ToString());

        sb.AppendLine();
        sb.AppendLine($"Cast time: takeoff - {castTime.GetValue().StatValueToStringByStatStringType(StatStringType.Absolute)} s" +
            $", takedown - {castTime_second.GetValue().StatValueToStringByStatStringType(StatStringType.Absolute)} s");
        sb.AppendLine($"Mana cost: {manaCost.GetValue().StatValueToStringByStatStringType(StatStringType.Absolute, 0)}");
        sb.Append($"Cooldown: {cooldown.GetValue().StatValueToStringByStatStringType(StatStringType.Absolute)} s");

        sb.Append("</size>");
        abilityTooltip = sb;
    }
}
