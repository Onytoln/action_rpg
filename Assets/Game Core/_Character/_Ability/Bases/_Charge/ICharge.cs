using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharge : IAoE {
    public SkillStat ChargeDuration { get; }
    public SkillStat ChargeSpeed { get; }
    public SkillStatInt MaxPierceCount { get; }
    public BoolControlComplex PiercesAllTargets { get; }
}

public interface IChargeValues : IAoEValues {
    public SkillStatContainer ChargeDurationValues { get; }
    public SkillStatContainer ChargeSpeedValues { get; }
    public SkillStatIntContainer MaxPierceCountValues { get; }
    public bool PiercesAllTargetsValues { get; }
}