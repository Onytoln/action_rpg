using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcSkillTemplate : SkillTemplate
{

    public override Vector3 CurrentDesiredCastPoint => NpcBehaviour.Target.position;
    public override GameObject CurrentDesiredTarget => NpcBehaviour.Target.gameObject;

    [field: SerializeField, Header("Npc Skill Template")] public NPCController NpcController { get; private set; }
    [field: SerializeField] public NPCBehavior NpcBehaviour { get; private set; }

    public float postAttackActionBlockTime;
    public float postAttackActionBlockTimeChargeNotEmpty;

    public override void Awake() {
        base.Awake();
        if(NpcController == null) NpcController = gameObject.GetComponent<NPCController>();
        if(NpcBehaviour == null) NpcBehaviour = gameObject.GetComponent<NPCBehavior>();
    }

    public override void OnValidate() {
        base.OnValidate();

        NpcController = GetComponent<NPCController>();
        NpcBehaviour = GetComponent<NPCBehavior>();

        if(skillProperties.SkillCastType == SkillCastType.Instant) {
            Debug.LogError("NPC's cannot have Instant cast skills.");
        }
    }

    public override bool CanCast(bool checkForTargetDistance = false) {
        if(checkForTargetDistance) {
            if (NpcBehaviour.Target == null || NpcBehaviour.DistanceFromTarget > skillProperties.maxCastRange.GetValue() 
                || NpcBehaviour.DistanceFromTarget < skillProperties.minCastRange.GetValue()) return false; 
        }

        return CharacterComponent.CharacterStatusEffectsManager.CanCast(this)
            && (skillProperties.chargeSystem.HasCharges() || CurrentCooldown <= 0);
    }

    public override bool CastSkill() {
        _ = base.CastSkill();
        CastPoint = CurrentDesiredCastPoint;
        Target = CurrentDesiredTarget;
        return true;
    }

    public override void SkillAnimationStart() {
        base.SkillAnimationStart();
        NpcController.FacePointQuickly(CastPoint);
    }

    public override void FullCastDone() {
        base.FullCastDone();
    }

    public override void TurnCharacterTowardsCastPoint(Vector3 point) {
        NpcController.FacePointQuickly(point);
    }

    protected override void SetPooling(GameObject obj, int amount, int poolCapacity = 0) {
        if (!IsAwakePhase) {
            Debug.LogError("SetPooling in SkillTemplate must be called during Awake phase only!");
            return;
        }

        GameSceneManager.LatePostSceneLoadPhase.ExecuteSync(() => {
            ObjectPoolManager.PrePoolObjects(obj.name, obj, amount, poolCapacity);
        }, null, ExecuteAmount.Once);
    }
}
