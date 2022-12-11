using UnityEngine;

public class FireProjectilesDefault : PlayerSkillTemplate {

    [SerializeField, Header("Fire Projectiles Default")] private TrailRenderer castParticlesPrefab;
    [SerializeField] private Transform handLeft;
    private TrailRenderer handLeftRenderer;
    [SerializeField] private Transform handRight;
    private TrailRenderer handRightRenderer;

    public override void Awake() {
        base.Awake();
        if (handLeft != null) {
            handLeftRenderer = Instantiate(castParticlesPrefab, handLeft);
            handLeftRenderer.emitting = false;
        }

        if (handRight != null) {
            handRightRenderer = Instantiate(castParticlesPrefab, handRight);
            handRightRenderer.emitting = false;
        }

        SetPooling(skillProperties.abilityPrefab, 5);
    }

    public override bool CastSkill() {
        if (!CanCast()) {
            return false;
        }
        _ = base.CastSkill();
        SelectAndUseSpammableAnim();
        return true;
    }

    public override void SkillAnimationStart() {
        if(handLeftRenderer != null) handLeftRenderer.emitting = true;
        if(handRightRenderer != null) handRightRenderer.emitting = true;

        base.SkillAnimationStart();
        TurnCharacterTowardsCastPoint(CastPoint);
    }

    public override void FireSkill() {
        if (handLeftRenderer != null) handLeftRenderer.emitting = false;
        if (handRightRenderer != null) handRightRenderer.emitting = false;

        FireProjectiles(releasePoint);

        base.FireSkill();
    }

    private void FireProjectiles(Transform releasePoint) {
        _ = AbilityObjectManager.FireProjectiles(skillProperties.abilityPrefab, skillProperties,
           CharacterComponent, releasePoint, CastPoint, this, Target);
    }

    protected override void StandaloneCast(Transform releasePoint) {
        FireProjectiles(releasePoint);
    }

    public override void SkillFired(SkillProperties skillProperties) {
        AllowInterrupt(SkillInterruptType.ByCastingAndMovement);
        base.SkillFired(skillProperties);
    }

    public override void OnStunned(bool state) {
        base.OnStunned(state);
        if (!state) return;

        if (handLeftRenderer != null) handLeftRenderer.emitting = false;
        if (handRightRenderer != null) handRightRenderer.emitting = false;
    }

    /*public void ListenToFiredSkill(SkillProperties skillProperties) {
        if(skillProperties.abilityId == "mage_skill_fireball") {
            AbilityObject projectile = skillProperties.skillObject.GetComponent<AbilityObject>();
            projectile.onSkillMissed += ShowMessage;
        }
    }

    public void ShowMessage(bool didMiss) {
        if (didMiss) {
            //Debug.Log("missed");
        }
    }*/
}
