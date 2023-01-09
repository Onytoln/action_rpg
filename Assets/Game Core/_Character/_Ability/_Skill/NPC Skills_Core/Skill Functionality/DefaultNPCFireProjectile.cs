using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultNPCFireProjectile : NpcSkillTemplate {

    [Tooltip("Assign particles that don't destroy themselves on end")]
    [SerializeField] private ParticleSystem preFireParticlePrefab;
    [SerializeField] Transform spawnParticleAt;
    private float defaultParticleSpeed;
    private ParticleSystem instantiatedParticle;

    public override void Awake() {
        base.Awake();

        if (instantiatedParticle == null && preFireParticlePrefab != null) {
            instantiatedParticle = Instantiate(preFireParticlePrefab, spawnParticleAt.position, spawnParticleAt.rotation);
            instantiatedParticle.transform.SetParent(spawnParticleAt);
            defaultParticleSpeed = instantiatedParticle.main.simulationSpeed;
        }

        skillProperties.castTime.OnChanged += RecalculateParticleSpeed;

        SetPooling(skillProperties.abilityPrefab, 1);
    }

    private void RecalculateParticleSpeed(IStatFloatReadonly skillStat) {
        if(instantiatedParticle == null) return;

        Utils.SetParticleSimulationSpeed(skillStat.PrimaryValue, skillStat.Value, instantiatedParticle, defaultParticleSpeed);
    }

    public override bool CastSkill() {
        if (!CanCast(true)) { return false; }
        _ = base.CastSkill();

        SetAnimationTrigger(skillProperties.HashedSkillTriggers[0].triggerHash, skillProperties.HashedSkillTriggers[0].animationSpeedFloatHash,
            skillProperties.AnimationCurrentSpeedModifier);
        return true;
    }

    public override void SkillAnimationStart() {
        StartParticle();

        base.SkillAnimationStart();
        TurnCharacterTowardsCastPoint(CastPoint);
    }

    public override void FireSkill() {
        if (instantiatedParticle != null) {
            instantiatedParticle.Stop();
        }

        AbilityObjectManager.FireProjectiles(skillProperties.abilityPrefab, skillProperties, CharacterComponent, releasePoint, CastPoint, this, Target);

        base.FireSkill();
    }

    public override void OnStunned(bool state) {
        base.OnStunned(state);
        if (!state) return;

        StopParticle();
    }

    public override void OnFrozen(bool state) {
        base.OnFrozen(state);
        if (state) {
            if (instantiatedParticle != null) {
                instantiatedParticle.Pause();
            }
        } else {
            StartParticle();
        }
    }

    public override void OnDeath(bool state) {
        base.OnDeath(state);
        if(!state) return;

        StopParticle();
    }

    public override void OnCompleteDestroy(Character character) {
        base.OnCompleteDestroy(character);

        if (instantiatedParticle != null) Destroy(instantiatedParticle.gameObject);
    }

    private void StartParticle() {
        if (instantiatedParticle != null) {
            instantiatedParticle.Play(true);
        }
    }

    private void StopParticle() {
        if (instantiatedParticle != null) {
            instantiatedParticle.Stop(true);
        }
    }
}
