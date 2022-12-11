using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileObject : AoEObject {

    [SerializeField, Header("Projectile Object")] bool destroyOnMaxDistanceTraveled = true;
    [SerializeField] protected AudioSource impactAudioSource;
    [SerializeField] protected ParticleSystem impactParticles;

    [SerializeField, Header("Projectile Object - End Particles")] protected ParticleSystem projectileFunctionEndParticles;
    [SerializeField] private bool scaleEndParticles = true;
    private Vector3 initialProjectileFunctionEndParticlesScale;

    private IProjectileValues projectileProperties;

    protected Dictionary<Collider, int> collectiveUniqueTargetHitsDict;
    protected int currentPierceCount = 0;

    //execute projectile behavior if can hit
    protected Action<Collider> executeIfCanHit;

    public override void Awake() {
        base.Awake();

        if (projectileFunctionEndParticles != null) {
            initialProjectileFunctionEndParticlesScale = projectileFunctionEndParticles.transform.localScale;
        }
    }

    public override void Initialize(params object[] parameters) {
        base.Initialize(parameters);
        collectiveUniqueTargetHitsDict = Utils.GetOfTypeFromArrayOfObjects<ProjectilesFireInstanceMaster>(parameters).collectiveHitDict;
    }

    public override void OnEnable() {
        base.OnEnable();
        if (CoreAbilityData == null) return;
        projectileProperties = CoreAbilityData.AbilityPropertiesValuesContainer.TryGetProjectilePropertiesValues();
    }

    public override void Update() {
        if (destroyOnMaxDistanceTraveled && Vector3.Distance(transform.position, SpawnPosition) > CoreAbilityData.AbilityPropertiesValuesContainer.HitRange.Value) {
            SkillMissed(true);
            ObjectPoolManager.PoolObjectBack(gameObject.name, gameObject);
        }
    }

    public override void OnDisable() {
        base.OnDisable();
        projectileProperties = null;
    }

    public override void EndObjectsFunction() {
        if (projectileFunctionEndParticles != null) {
            ParticleSystem ps = VfxManager.PlayOneShotParticle(projectileFunctionEndParticles, transform.position);

            if (scaleEndParticles) {
                Utils.ScaleTransform(ps.transform, initialProjectileFunctionEndParticlesScale,
                    projectileProperties.ScaleValues.Value, projectileProperties.ScaleValues.PrimaryValue);
            }
        }

        base.EndObjectsFunction();
    }

    protected override void OnTriggerEnter(Collider other) {
        base.OnTriggerEnter(other);

        bool isTerrain = other.CompareTag("Terrain");

        if (!projectileProperties.CanPierceTerrainValue && isTerrain) {
            SkillMissed(true);
            EndObjectsFunction();
            return;
        }

        if (projectileProperties.InfiniteUniqueTargetHitsValue
            || Utils.DictGetAndExecuteOrAddAndExecute(collectiveUniqueTargetHitsDict, other, (value) => ++value, out bool hadValue)
                   <= projectileProperties.MaxUniqueTargetHitsValues.Value) {

            executeIfCanHit ??= ExecuteIfCanHit; 
            executeIfCanHit.Invoke(other);

            if(impactParticles != null) {
                VfxManager.PlayOneShotParticle(impactParticles, transform.position);
            }

            if (impactAudioSource != null) {
                AudioManager.PlayOneShotAudioSource(impactAudioSource, transform.position);
            }
        } else
            return;

        if (projectileProperties.PiercesAllTargetsValue) return;
        currentPierceCount += 1;
        if (currentPierceCount > projectileProperties.MaxPierceCountValues.Value) {
            SkillMissed(false);
            EndObjectsFunction();
            return;
        }
    }

    protected virtual void ExecuteIfCanHit(Collider other) { }
}
