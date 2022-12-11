using MEC;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DefaultNPCJump : NpcSkillTemplate
{

    [SerializeField] private Transform objectToAnimateJump;

    private ValueWrapper<bool> pause = new ValueWrapper<bool>(false);
    private ValueWrapper<bool> kill = new ValueWrapper<bool>(false);
    private CoroutineHandle? jumpCoroutine;
    private Vector3 jumpPoint;

    [SerializeField] private float jumpAnnulusMin = .5f;
    [SerializeField] private float jumpAnnulusMax = 1f;

    public override void Awake() {
        base.Awake();
    }

    public override bool CanCast(bool checkForTargetDistance = false) {
        if (!base.CanCast(checkForTargetDistance)) return false;

        Vector3 desiredPoint;
        if (NpcBehaviour.DistanceFromTarget <= skillProperties.hitRange.GetValue()) {
            desiredPoint = Utils.GetRandomPointInAnnulusXZ(NpcBehaviour.Target.position, jumpAnnulusMin, jumpAnnulusMax);
        } else {
            desiredPoint = Utils.GetRandomPointInAnnulusXZ(transform.position + (NpcBehaviour.Target.position - transform.position).normalized * skillProperties.hitRange.GetValue(),
                jumpAnnulusMin, jumpAnnulusMax);
        }
       
        jumpPoint = Utils.GetWalkablePoint(desiredPoint);
        if (jumpPoint == Vector3.zero) return false;

        return true;
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
        TurnCharacterTowardsCastPoint(jumpPoint);
        NpcController.ExternalObstacleAndControllerDisable(true, this);
    }

    public override void FireSkill() {
        pause.Value = false;
        kill.Value = false;

        float jumpTime = (1f - CharacterComponent.CharacterAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime) * skillProperties.castTime.GetValue();
        jumpCoroutine = Utils.JumpCharacterToLocation(NpcController.agent, jumpPoint, jumpTime, objectToAnimateJump, jumpCoroutine, pause: pause, kill: kill);

        base.FireSkill();
    }

    public override void FullCastDone() {
        base.FullCastDone();

        kill.Value = true;
        NpcController.ExternalObstacleAndControllerDisable(false, this);
    }

    public override void OnFrozen(bool state) {
        base.OnFrozen(state);

        if (state) {
            pause.Value = true;
        } else {
            pause.Value = false;
        }
    }

    public override void OnCompleteDestroy(Character character) {
        base.OnCompleteDestroy(character);
    }
}
