using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePropertiesContainerStandAlone : AoEPropertiesContainerStandAlone, IProjectileValues {
    public ProjectileFireType ProjectileFireType{ get; private set; }
    public SkillStatIntContainer ProjectileCount { get; private set; }
    public SkillStatIntContainer MaxPierceCount { get; private set; }
    public SkillStatIntContainer MaxUniqueTargetHits { get; private set; }
    public bool CanPierceTerrain { get; private set; }
    public bool PiercesAllTargets { get; private set; }
    public bool InfiniteUniqueTargetHits { get; private set; }

    public ProjectilePropertiesContainerStandAlone(IProjectile aoe) : base(aoe) {
        ProjectileFireType = aoe.ProjectileFireType;
        ProjectileCount = new SkillStatIntContainer(aoe.ProjectileCount);
        MaxPierceCount = new SkillStatIntContainer(aoe.MaxPierceCount);
        MaxUniqueTargetHits = new SkillStatIntContainer(aoe.MaxUniqueTargetHits);
        CanPierceTerrain = aoe.CanPierceTerrain.Value;
        PiercesAllTargets = aoe.PiercesAllTargets.Value;
        InfiniteUniqueTargetHits = aoe.InfiniteUniqueTargetHits.Value;
    }

    public ProjectileFireType ProjectileFireTypeValue => ProjectileFireType;
    public SkillStatIntContainer ProjectileCountValues => ProjectileCount;
    public SkillStatIntContainer MaxPierceCountValues => MaxPierceCount;
    public SkillStatIntContainer MaxUniqueTargetHitsValues => MaxUniqueTargetHits;
    public bool CanPierceTerrainValue => CanPierceTerrain;
    public bool PiercesAllTargetsValue => PiercesAllTargets;
    public bool InfiniteUniqueTargetHitsValue => InfiniteUniqueTargetHits;
}
