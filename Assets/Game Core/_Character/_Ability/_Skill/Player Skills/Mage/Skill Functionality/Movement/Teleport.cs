using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Teleport : PlayerSkillTemplate
{
    private NavMeshAgent agent;
    private Vector3 teleportPos;
    [SerializeField] private ParticleSystem teleportStartPointParticles;
    [SerializeField] private ParticleSystem teleportEndPointParticles;

    public override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
    }

    public override bool CanCast(bool checkForTargetDistance = false) {
        if (!base.CanCast(checkForTargetDistance)) return false;

        if (!Utils.GetMovementAbilityPosition(transform.position, TargetManager.CurrentPlayersTargetPoint, skillProperties.maxCastRange.GetValue(), 
            (skillProperties as TeleportProperties).rangeTolerance, out teleportPos, agent.radius)) return false;
         
        return true;
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

        TurnCharacterTowardsCastPoint(teleportPos);
        _ = Utils.DisableObstaclesInArea(teleportPos, CharacterComponent.CharacterNavMeshAgent);

        VfxManager.PlayOneShotParticle(teleportStartPointParticles, transform.position, Quaternion.identity);
    }

    public override void FireSkill() {
        base.FireSkill();
        _ = agent.Warp(teleportPos);

        VfxManager.PlayOneShotParticle(teleportEndPointParticles, transform.position, Quaternion.identity);
    }

    public override void FullCastDone() {
        base.FullCastDone();
        CharacterComponent.CharacterCombat.GetStatusEffectApplied(skillProperties.buffHolder[0].buffToApply, CharacterComponent,
            null, skillProperties.buffHolder[0].stacksToApply.GetValue());
    }
}
