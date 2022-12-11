using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Fire DOT Debuff", menuName = "Status Effects/Debuffs/Fire DOT Debuff")]
public class FireDOTDebuff : Debuff {

    private FireDOTDebuffProperties fireDOTDebuffProperties;

    [SerializeField] private ParticleSystem burningParticlesPrefab;
    private ParticleSystem burningParticles;
    private Vector3 defaultBurningParticlesScale;

    [SerializeField] private AudioSource audioPrefab;
    private AudioSource audio;

    public override void Awake() {
        base.Awake();

        if (DebuffTypes == null || DebuffTypes.Length == 0) {
            DebuffTypes = new DebuffType[2] { DebuffType.DamageOverTime, DebuffType.FireDamageOverTime };
        }

        if (burningParticlesPrefab != null) {
            defaultBurningParticlesScale = burningParticlesPrefab.transform.localScale;
        }
    }

    public override void Apply(int stacksCount, HitOutput hitOutput) {
        base.Apply(stacksCount, hitOutput);

        fireDOTDebuffProperties = statusEffectProperties as FireDOTDebuffProperties;

        UpdateDebuffStrengthModifiers(ApplierStatsContainer);
    }

    public override void Refresh(CoreStatsValuesContainer applierStatsContainer, int stacksCount, HitOutput hitOutput) {
        base.Refresh(applierStatsContainer, stacksCount, hitOutput);

        CompareAndSetStats(applierStatsContainer, StatsCompareType.Offensive);

        UpdateDebuffStrengthModifiers(applierStatsContainer);
    }

    public override void Tick(float deltaTime) {
        base.Tick(deltaTime);
        if (CurrentTickTime >= fireDOTDebuffProperties.tickRate.GetValue()) {
            CurrentTickTime = 0;
            _ = HitEnemy((fireDOTDebuffProperties.abilityDamage.GetValue() + (fireDOTDebuffProperties.damagePerStack.GetValue() * (CurrentStacks - 1))) * DebuffStrenghtModifier,
                statusEffectProperties.hitInfoId);
        }
    }

    public override void OnStartVfx() {
        ObjectPoolManager objectPoolManager = ObjectPoolManager.Instance;

        if (burningParticlesPrefab != null) {
            burningParticles = objectPoolManager.GetPooledParticleSystem(burningParticlesPrefab.name, burningParticlesPrefab);
            burningParticles.transform.SetParent(AppliedToCharacterComponent.VfxSpawnPosMiddle, false);
            Utils.ScaleTransform(burningParticles.transform, defaultBurningParticlesScale, AppliedToCharacterComponent.VfxScaleModifier, 1f);
            burningParticles.Play(true);
        }

        if (audioPrefab != null) {
            audio = objectPoolManager.GetPooledAudioSource(audioPrefab.name, audioPrefab);
            audio.transform.SetParent(AppliedToCharacterComponent.VfxSpawnPosMiddle, false);
            audio.Play();
        }
    }

    public override void OnEndVfx() {
        ObjectPoolManager objectPoolManager = ObjectPoolManager.Instance;

        if (burningParticles != null) objectPoolManager.PoolParticleSystemBack(burningParticles.name, burningParticles);

        if (audio != null) objectPoolManager.PoolAudioSourceBack(audio.name, audio);
    }
}
