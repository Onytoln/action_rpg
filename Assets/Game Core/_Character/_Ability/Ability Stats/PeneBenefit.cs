using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PeneBenefit {
    [field: SerializeField] public CharacterStatType PenetrationBenefitType { get; private set; }
    [field: SerializeField] public StatFloat PenetrationBenefit { get; private set; }

    public PeneBenefit(CharacterStatType penetrationBenefitType, StatFloat penetrationBenefit) {
        PenetrationBenefitType = penetrationBenefitType;
        PenetrationBenefit = penetrationBenefit;
    }
}

public class PeneBenefitContainer {
    public CharacterStatType PenetrationBenefitType { get; private set; }
    public SkillStatContainer PenetrationBenefit { get; private set; }

    public PeneBenefitContainer(PeneBenefit peneBenefit) {
        if (!peneBenefit.PenetrationBenefitType.IsPenetration()) {
            throw new System.Exception("PeneBenefitContainer cannot take stat type values that are not penetration.");
        }

        PenetrationBenefitType = peneBenefit.PenetrationBenefitType;
        PenetrationBenefit = new SkillStatContainer(peneBenefit.PenetrationBenefit);
    }
}
