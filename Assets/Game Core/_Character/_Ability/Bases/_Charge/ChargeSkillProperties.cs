using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeSkillProperties : AoESkillProperties {
    [field: SerializeField] public SkillStat ChargeDuration { get; set; }
    [field: SerializeField] public SkillStat ChargeSpeed { get; set; }
    [field: SerializeField] public SkillStatInt MaxPierceCount { get; set; }
    [field: SerializeField] public BoolControlComplex PiercesAllTargets { get; set; }

    public override AbilityPropertiesValuesContainer GetValuesCopy() {
        return new ChargeSkillPropertiesValuesContainer(this);
    }

    public override void AssignReferences() {
        base.AssignReferences();
        ChargeDuration.SetTooltipDirtyMethod = SetTooltipIsDirty;
        ChargeSpeed.SetTooltipDirtyMethod = SetTooltipIsDirty;
        MaxPierceCount.setTooltipDirtyMethod = SetTooltipIsDirty;
        Scale.SetTooltipDirtyMethod = SetTooltipIsDirty;
    }
}
