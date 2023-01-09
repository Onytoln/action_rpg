using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharge : IAoE {
    public StatFloat ChargeDuration { get; }
    public StatFloat ChargeSpeed { get; }
    public StatInt MaxPierceCount { get; }
    public BoolControlComplex PiercesAllTargets { get; }
}

public interface IChargeValues : IAoEValues {
    public SkillStatContainer ChargeDurationValues { get; }
    public SkillStatContainer ChargeSpeedValues { get; }
    public SkillStatIntContainer MaxPierceCountValues { get; }
    public bool PiercesAllTargetsValues { get; }
}