using UnityEngine;

public class FrostCharge : PlayerSkillTemplate {
    public static int chargeLoopStartHash = Animator.StringToHash("MovementProgressive_1_Loop_start");

    FrostChargeProperties frostChargeProperties;

    ChargeHandler chargeHandler;
    AbilityObject chargeColliderObject;
    bool functionalityDisabled;

    [SerializeField] ShieldShaderHandler shieldVFXHandlerPrefab;
    [SerializeField] ParticleSystem postChargeExplosionParticlesPrefab;
    [SerializeField] ParticleSystem chargeParticlesPrefab;
    [SerializeField] Transform shieldVFXAttachTransform;


    private ShieldShaderHandler currentShieldVFXHandler;
    private Vector3 defaultShieldVFXScale;

    private ParticleSystem currentPostChargeExplosionParticles;
    private Vector3 defaultPostChargeExplosionParticlesScale;

    private ParticleSystem currentChargeParticles;

    public override void Awake() {
        base.Awake();

        if (frostChargeProperties == null) frostChargeProperties = skillProperties as FrostChargeProperties;

        if (currentShieldVFXHandler == null && shieldVFXHandlerPrefab != null) {
            currentShieldVFXHandler = Instantiate(shieldVFXHandlerPrefab, shieldVFXAttachTransform);
            defaultShieldVFXScale = currentShieldVFXHandler.transform.localScale;
            frostChargeProperties.Scale.OnChanged += ChangeShieldScale;
        }

        if (currentPostChargeExplosionParticles == null && postChargeExplosionParticlesPrefab != null) {
            currentPostChargeExplosionParticles = Instantiate(postChargeExplosionParticlesPrefab, shieldVFXAttachTransform);
            defaultPostChargeExplosionParticlesScale = currentPostChargeExplosionParticles.transform.localScale;
            frostChargeProperties.postChargeExplosionRadius.OnChanged += ChargeExplosionScale;
        }

        if (currentChargeParticles == null && chargeParticlesPrefab != null) {
            currentChargeParticles = Instantiate(chargeParticlesPrefab, shieldVFXAttachTransform);
        }
    }

    void ChangeShieldScale(StatFloat stat) {
        currentShieldVFXHandler.transform.localScale = defaultShieldVFXScale * (stat.GetValue() / stat.GetPrimaryValue());
    }

    void ChargeExplosionScale(StatFloat stat) {
        currentPostChargeExplosionParticles.transform.localScale = defaultPostChargeExplosionParticlesScale * (stat.GetValue() / stat.GetPrimaryValue());
    }

    public override bool CastSkill() {
        if (!CanCast()) {
            return false;
        }
        _ = base.CastSkill();

        SetAnimationTrigger(skillProperties.HashedSkillTriggers[0].triggerHash,
            skillProperties.HashedSkillTriggers[0].animationSpeedFloatHash, skillProperties.AnimationCurrentSpeedModifier);

        return true;
    }

    public override void SkillAnimationStart() {
        base.SkillAnimationStart();
        TurnCharacterTowardsCastPoint(CastPoint);

        if (currentShieldVFXHandler != null) currentShieldVFXHandler.ShowShield(true, frostChargeProperties.castTime.GetValue());
    }

    public override void FireSkill() {
        SetAnimationTrigger(chargeLoopStartHash);

        chargeHandler = Utils.ChargeCharacterToLocation(CastPoint, CharacterComponent.CharacterNavMeshAgent, CasterHitLayers.GetAbilityObjectHitLayerName()
            , FinishCharge, frostChargeProperties.ChargeDuration.GetValue(), frostChargeProperties.ChargeSpeed.GetValue(), frostChargeProperties.MaxPierceCount.GetValue(), frostChargeProperties.PiercesAllTargets.Value,
            stayAboveGroundCallRate: 0f, raycastCheckRate: 0f);

        GameObject obj = ObjectPoolManager.GetPooledObject(frostChargeProperties.abilityPrefab.name, frostChargeProperties.abilityPrefab);
        obj.transform.SetParent(transform, false);
        chargeColliderObject = AbilityObjectManager.ProcessSpawnedAbilityObject(obj, skillProperties, CharacterComponent, this, CastPoint, Target);

        if (currentChargeParticles != null) currentChargeParticles.Play();

        functionalityDisabled = false;

        base.FireSkill();
    }

    private void FinishCharge() {
        if (functionalityDisabled) return;

        SetAnimationTrigger(skillProperties.HashedSkillTriggers[1].triggerHash,
            skillProperties.HashedSkillTriggers[1].animationSpeedFloatHash, skillProperties.AnimationCurrentSpeedModifierSecond);

        chargeHandler = null;
        EndCharge(skillProperties.castTime_second.GetValue() * 0.33f);
        chargeColliderObject = null;

        //explosion
        if (currentPostChargeExplosionParticles != null) currentPostChargeExplosionParticles.Play(true);

        Collider[] colliders = Physics.OverlapSphere(transform.position, frostChargeProperties.postChargeExplosionRadius.GetValue(), CasterHitLayers.GetDirectHitLayer());
        for (int i = 0; i < colliders.Length; i++) {
            _ = HitEnemy(colliders[i], frostChargeProperties.postChargeExplosionDamage.GetValue(), frostChargeProperties.postChargeExplosionHitInforId,
                frostChargeProperties.PostChargeExplosionDamageTypes);
        }
    }

    public override void FullCastDone() {
        base.FullCastDone();

        functionalityDisabled = true;
        EndCharge();
    }

    public override void OnFrozen(bool state) {
        base.OnFrozen(state);

        if (state) {
            if (chargeColliderObject != null) chargeColliderObject.DisableObjectFunctionality();
            if (chargeHandler != null) chargeHandler.PauseCharge(true);
            if (currentChargeParticles != null) currentChargeParticles.Stop();
        } else {
            if (chargeColliderObject != null) chargeColliderObject.EnableObjectFunctionality();
            if (chargeHandler != null) { 
                chargeHandler.PauseCharge(false);
                if (currentChargeParticles != null) currentChargeParticles.Play();
            }
        }
    }

    public override void OnRooted(bool state) {
        base.OnRooted(state);

        if (!state) return;

        functionalityDisabled = true;
        EndCharge();

        CharacterComponent.CharacterStatusEffectsManager.ReturnToDefaultAnimationState();
    }

    private void EndCharge(float hideShieldTime = 0.5f) {
        if (chargeColliderObject != null) chargeColliderObject.EndObjectsFunction();
        if (chargeHandler != null) chargeHandler.StopCharge();
        if (currentShieldVFXHandler != null) currentShieldVFXHandler.ShowShield(false, hideShieldTime);
        if (currentChargeParticles != null) currentChargeParticles.Stop();
    }
}
