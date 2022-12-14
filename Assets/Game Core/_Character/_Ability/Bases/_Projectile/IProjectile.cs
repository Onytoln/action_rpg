using UnityEngine;

public interface IProjectile : IAoE {
    public ProjectileFireType ProjectileFireType { get; }
    public StatInt ProjectileCount { get; }
    public StatInt MaxPierceCount { get; }
    public StatInt MaxUniqueTargetHits { get; }
    public BoolControlComplex CanPierceTerrain { get; }
    public BoolControlComplex PiercesAllTargets { get; }
    public BoolControlComplex InfiniteUniqueTargetHits { get; }
}

public interface IProjectileValues : IAoEValues {
    public ProjectileFireType ProjectileFireTypeValue { get; }
    public SkillStatIntContainer ProjectileCountValues { get; }
    public SkillStatIntContainer MaxPierceCountValues { get; }
    public SkillStatIntContainer MaxUniqueTargetHitsValues { get; }
    public bool CanPierceTerrainValue { get; }
    public bool PiercesAllTargetsValue { get; }
    public bool InfiniteUniqueTargetHitsValue { get; }
}
