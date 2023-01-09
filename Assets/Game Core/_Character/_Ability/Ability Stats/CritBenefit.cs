using UnityEngine;

public enum CriticalStrikeBenefitType { CritChanceAbsolute = 0, CritChanceRelative = 1, CritDamageAbsolute = 2, CritDamageRelative = 3 }

[System.Serializable]
public class CritBenefit {
    [field: SerializeField] public CriticalStrikeBenefitType CriticalStrikeBenefitType { get; private set; }
    [field: SerializeField] public StatFloat CriticalStrikeBenefit { get; private set; }

    public CritBenefit(CriticalStrikeBenefitType csbt, StatFloat skillStat) {
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
