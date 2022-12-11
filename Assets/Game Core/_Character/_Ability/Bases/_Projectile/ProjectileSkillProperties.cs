using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSkillProperties : AoESkillProperties, IProjectile {
    [field: SerializeField] public ProjectileFireType ProjectileFireType { get; private set; }
    [field: SerializeField] public SkillStatInt ProjectileCount { get; private set; }
    [field: SerializeField] public SkillStatInt MaxPierceCount { get; private set; }
    [field: SerializeField] public SkillStatInt MaxUniqueTargetHits { get; private set; }
    [field: SerializeField] public BoolControlComplex CanPierceTerrain { get; private set; } 
    [field: SerializeField] public BoolControlComplex PiercesAllTargets { get; private set; } 
    [field: SerializeField] public BoolControlComplex InfiniteUniqueTargetHits { get; private set; }

    public override AbilityPropertiesValuesContainer GetValuesCopy() {
        return new ProjectileSkillPropertiesValuesContainer(this);
    }


    public override void AssignReferences() {
        base.AssignReferences();
        ProjectileCount.setTooltipDirtyMethod = SetTooltipIsDirty;
        MaxPierceCount.setTooltipDirtyMethod = SetTooltipIsDirty;
        MaxUniqueTargetHits.setTooltipDirtyMethod = SetTooltipIsDirty;
    }
}
