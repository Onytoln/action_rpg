using UnityEngine;

[System.Serializable]
public class CritBenefit {
    [field: SerializeField] public CriticalStrikeBenefitType CriticalStrikeBenefitType { get; private set; }
    [field: SerializeField] public SkillStat CriticalStrikeBenefit { get; private set; }

    public CritBenefit(CriticalStrikeBenefitType csbt, SkillStat skillStat) {
        CriticalStrikeBenefitType = csbt;
        CriticalStrikeBenefit = skillStat;  
    }
}

public class CritBenefitContainer {
    public CriticalStrikeBenefitType CriticalStrikeBenefitType { get; private set; }
    public SkillStatContainer CriticalStrikeBenefit { get; private set; }

    public CritBenefitContainer(CritBenefit critBenefit) {
        CriticalStrikeBenefitType = critBenefit.CriticalStrikeBenefitType;
        CriticalStrikeBenefit = new SkillStatContainer(critBenefit.CriticalStrikeBenefit);
    }
}
