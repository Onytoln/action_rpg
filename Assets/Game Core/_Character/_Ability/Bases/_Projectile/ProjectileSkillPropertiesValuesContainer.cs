using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSkillPropertiesValuesContainer : AoESkillPropertiesValuesContainer, IProjectileValues { 
    public ProjectileFireType ProjectileFireType { get; private set; }
    public SkillStatIntContainer ProjectileCount { get; private set; }
    public SkillStatIntContainer MaxPierceCount { get; private set; }
    public SkillStatIntContainer MaxUniqueTargetHits { get; private set; }
    public bool CanPierceTerrain { get; private set; }
    public bool PiercesAllTargets { get; private set; }
    public bool InfiniteUniqueTargetHits { get; private set; }

    public ProjectileSkillPropertiesValuesContainer(ProjectileSkillProperties projProp) : base(projProp) {
        ProjectileFireType = projProp.ProjectileFireType;
        ProjectileCount = new SkillStatIntContainer(projProp.ProjectileCount);
        MaxPierceCount = new SkillStatIntContainer(projProp.MaxPierceCount);
        MaxUniqueTargetHits = new SkillStatIntContainer(projProp.MaxUniqueTargetHits);
        CanPierceTerrain = projProp.CanPierceTerrain.Value;
        PiercesAllTargets = projProp.PiercesAllTargets.Value;
        InfiniteUniqueTargetHits = projProp.InfiniteUniqueTargetHits.Value;
    }

    public override IProjectileValues TryGetProjectilePropertiesValues() { return this; }

    public ProjectileFireType ProjectileFireTypeValue => ProjectileFireType;
    public SkillStatIntContainer ProjectileCountValues => ProjectileCount;
    public SkillStatIntContainer MaxPierceCountValues => MaxPierceCount;
    public SkillStatIntContainer MaxUniqueTargetHitsValues => MaxUniqueTargetHits;
    public bool CanPierceTerrainValue => CanPierceTerrain;
    public bool PiercesAllTargetsValue => PiercesAllTargets;
    public bool InfiniteUniqueTargetHitsValue => InfiniteUniqueTargetHits;
}
