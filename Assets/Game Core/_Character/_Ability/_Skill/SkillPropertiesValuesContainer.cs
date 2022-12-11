using UnityEngine;

public class SkillPropertiesValuesContainer : AbilityPropertiesValuesContainer {
    public SkillStatContainer ManaCost { get; private set; }
    public SkillCastType SkillCastType { get; private set; }
    public SkillCastSpeedScalingType SkillCastSpeedScalingType { get; private set; }
    public SkillType[] SkillTypes { get; private set; }

    /// <summary>
    /// Not a deep copy!
    /// </summary>
    public ChargeSystem ChargeSystem { get; private set; }  
    /// <summary>
    /// Not a deep copy!
    /// </summary>
    public BuffHolder[] BuffHolder { get; private set; }
    /// <summary>
    /// Not a deep copy!
    /// </summary>
    public DebuffHolder[] DebuffHolder { get; private set; }

    public SkillStatContainer CastTime { get; private set; }
    public SkillStatContainer CastTime_second { get; private set; }
    public SkillStatContainer CastTime_third { get; private set; }

    public int MinLevelRequired { get; private set; } 

    public SkillPropertiesValuesContainer(SkillProperties skillProp) : base(skillProp) {
        ManaCost = new SkillStatContainer(skillProp.manaCost);
        SkillCastType = skillProp.SkillCastType;
        SkillCastSpeedScalingType = skillProp.SkillCastSpeedScalingType;

        SkillTypes = Utils.CopyArray(skillProp.SkillTypes);

        ChargeSystem = skillProp.chargeSystem; //not deep copy
        BuffHolder = skillProp.buffHolder; //not deep copy
        DebuffHolder = skillProp.debuffHolder; //not deep copy

        CastTime = new SkillStatContainer(skillProp.castTime);
        CastTime_second = new SkillStatContainer(skillProp.castTime_second);
        CastTime_third = new SkillStatContainer(skillProp.castTime_third);

        MinLevelRequired = skillProp.MinLevelRequired;
    }

    public bool IsSkillOfType(SkillType skillType) {
        for (int i = 0; i < SkillTypes.Length; i++) {
            if (SkillTypes[i] == skillType) return true;
        }

        return false;
    }
}
