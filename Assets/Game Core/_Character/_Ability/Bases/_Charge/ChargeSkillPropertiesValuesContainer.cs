using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeSkillPropertiesValuesContainer : AoESkillPropertiesValuesContainer, IChargeValues {
    public SkillStatContainer ChargeDuration { get; private set; }
    public SkillStatContainer ChargeSpeed { get; private set; }
    public SkillStatIntContainer MaxPierceCount { get; private set; }
    public bool PiercesAllTargets { get; private set; }

    public ChargeSkillPropertiesValuesContainer(ChargeSkillProperties chargeProp) : base(chargeProp) {
        ChargeDuration = new SkillStatContainer(chargeProp.ChargeDuration);
        ChargeSpeed = new SkillStatContainer(chargeProp.ChargeSpeed);
        MaxPierceCount = new SkillStatIntContainer(chargeProp.MaxPierceCount);
        PiercesAllTargets = chargeProp.PiercesAllTargets.Value;
    }

    public override IChargeValues TryGetChargePropertiesValues() { return this; }

    public SkillStatContainer ChargeDurationValues => ChargeDuration;
    public SkillStatContainer ChargeSpeedValues => ChargeSpeed;
    public SkillStatIntContainer MaxPierceCountValues => MaxPierceCount;
    public bool PiercesAllTargetsValues => PiercesAllTargets;

}
