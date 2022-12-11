
using UnityEngine;

public class DefaultNPCMeleeAttack : NpcSkillTemplate {

    [Tooltip("Assign particles that destroy themselves when they end")]
    [SerializeField] private ParticleSystem hitParticle;

    [SerializeField] private ParticleSystem weaponTrailPrefab;
    [SerializeField] private Transform weaponTrailSpawnTransform;

    private ParticleSystem currentWeaponTrail;

    public override void Awake() {
        base.Awake();

        if (currentWeaponTrail == null && weaponTrailPrefab != null) {
            currentWeaponTrail = Instantiate(weaponTrailPrefab, weaponTrailSpawnTransform);
        }
    }

    public override bool CastSkill() {
        if (!CanCast(true)) { return false; }
        _ = base.CastSkill();

        SetAnimationTrigger(skillProperties.HashedSkillTriggers[0].triggerHash, skillProperties.HashedSkillTriggers[0].animationSpeedFloatHash,
            skillProperties.AnimationCurrentSpeedModifier);
        return true;
    }

    public override void SkillAnimationStart() {
        base.SkillAnimationStart();
        TurnCharacterTowardsCastPoint(CastPoint);

        if (currentWeaponTrail != null) currentWeaponTrail.Play(true);
    }

    public override void FireSkill() {
        RaycastHit hit;

        if(Physics.SphereCast(transform.position + Vector3.up * 0.75f - transform.forward * 0.3f, 0.4f, transform.forward,
            out hit, skillProperties.hitRange.GetValue() + 0.3f, CasterHitLayers.GetDirectHitLayer())) {
            _ = HitEnemy(hit.collider, skillProperties.abilityDamage.GetValue(), skillProperties.hitInfoId);

            if(hitParticle != null) {
                VfxManager.PlayOneShotParticle(hitParticle, hit.transform.position, transform.rotation);
            }
        }

        base.FireSkill();
    }

    public override void FullCastDone() {
        base.FullCastDone();

        if (currentWeaponTrail != null) currentWeaponTrail.Stop(true);
    }

    public override void OnStunned(bool state) {
        if (!state) return;

        if (currentWeaponTrail != null) currentWeaponTrail.Stop(true);
    }

    public override void OnCompleteDestroy(Character character) {
        base.OnCompleteDestroy(character);

        if (currentWeaponTrail != null) Destroy(currentWeaponTrail.gameObject);
    }

    /*private void OnDrawGizmos() {
        Gizmos.DrawSphere(transform.position + Vector3.up * 0.75f - transform.forward * 0.3f, 0.4f);
    }*/
}
