using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargePropertiesContainerStandAlone : AoEPropertiesContainerStandAlone, IChargeValues {
    public SkillStatContainer ChargeDuration { get; private set; }
    public SkillStatContainer ChargeSpeed { get; private set; }
    public SkillStatIntContainer MaxPierceCount { get; private set; }
    public bool PiercesAllTargets { get; private set; }

    public ChargePropertiesContainerStandAlone(ICharge aoe) : base(aoe) {
        ChargeDuration = new SkillStatContainer(aoe.ChargeDuration);
        ChargeSpeed = new SkillStatContainer(aoe.ChargeSpeed);
        MaxPierceCount = new SkillStatIntContainer(aoe.MaxPierceCount);
        PiercesAllTargets = aoe.PiercesAllTargets.Value;
    }

    public SkillStatContainer ChargeDurationValues => ChargeDuration;
    public SkillStatContainer ChargeSpeedValues => ChargeSpeed;
    public SkillStatIntContainer MaxPierceCountValues => MaxPierceCount;
    public bool PiercesAllTargetsValues => PiercesAllTargets;
}
