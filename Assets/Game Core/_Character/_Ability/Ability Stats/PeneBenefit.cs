using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PeneBenefit {
    [field: SerializeField] public StatType PenetrationBenefitType { get; private set; }
    [field: SerializeField] public SkillStat PenetrationBenefit { get; private set; }

    public PeneBenefit(StatType penetrationBenefitType, SkillStat penetrationBenefit) {
        PenetrationBenefitType = penetrationBenefitType;
        PenetrationBenefit = penetrationBenefit;
    }
}

public class PeneBenefitContainer {
    public StatType PenetrationBenefitType { get; private set; }
    public SkillStatContainer PenetrationBenefit { get; private set; }

    public PeneBenefitContainer(PeneBenefit peneBenefit) {
        if (!peneBenefit.PenetrationBenefitType.IsPenetration()) {
            throw new System.Exception("PeneBenefitContainer cannot take stat type values that are not penetration.");
        }

        PenetrationBenefitType = peneBenefit.PenetrationBenefitType;
        PenetrationBenefit = new SkillStatContainer(peneBenefit.PenetrationBenefit);
    }
}
