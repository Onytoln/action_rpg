using System;
using UnityEngine;

public class PlayerSkillTemplate : SkillTemplate, IActionBarSlottable {
    //Skill lvl up boosts (only for player)
    [SerializeField, Header("Player Skill Template")] private SkillLvlUp skillLvlBoosts;

    public override Vector3 CurrentDesiredCastPoint => UsePoint == Vector3.zero ?
            (TargetManager.CurrentPlayersEnemyTarget == null ? TargetManager.CurrentPlayersTargetPoint : TargetManager.CurrentPlayersEnemyTarget.transform.position)
            : UsePoint;
    public override GameObject CurrentDesiredTarget => UseTarget == null ? TargetManager.CurrentPlayersEnemyTarget : UseTarget;

    public PlayerController PlayerController { get; private set; }

    public event Action<PlayerSkillTemplate, bool> OnActionBarAssigned;

    private KeyCode assignedKey;
    public KeyCode AssignedKey {
        get => assignedKey;
        set {
            assignedKey = value;

            if(assignedKey == KeyCode.None) {
                OnActionBarAssigned?.Invoke(this, false);
            } else {
                OnActionBarAssigned?.Invoke(this, true);
            }
        }
    }

    public bool CheckForDistance { get; set; }
    public Vector3 UsePoint { get; set; }
    public GameObject UseTarget { get; set; }
    public Action<int> OnUsesAmountChanged { get; set; }

    public override void Awake() {
        base.Awake();
        PlayerController = GetComponent<PlayerController>();
    }

    public override void Start() {
        base.Start();
        skillProperties.chargeSystem.OnChargesAmountChanged += UseAmountChanged;
    }

    #region Standalone Cast

    public void StandaloneCast(Vector3 position, GameObject target, params StandaloneCastParameter[] standaloneCastParameters) {
        var prevCastPoint = CastPoint;
        var prevTarget = Target;

        CastPoint = position;
        Target = target;

        Transform releasePoint =  ObjectPoolManager.GetPooledObject(
            DataStorage.ReleasePointObject.name,
            DataStorage.ReleasePointObject)
            .transform;

        releasePoint.SetParent(transform, false);
        releasePoint.localPosition = new Vector3(0f, DataStorage.AOBJ_RELEASE_Y_DEFAULT_OFFSET, 0f);
        releasePoint.rotation = Utils.GetXZLookRotation(position + (Vector3.up * releasePoint.localPosition.y), releasePoint.position);

        if (standaloneCastParameters != null) {
            foreach (var standaloneCastParameter in standaloneCastParameters) {
                standaloneCastParameter.PreCast(this, releasePoint);
            }
        }

        StandaloneCast(releasePoint);

        if (standaloneCastParameters != null) {
            foreach (var standaloneCastParameter in standaloneCastParameters) {
                standaloneCastParameter.PostCast(this, releasePoint);
            }
        }

        ObjectPoolManager.PoolObjectBack(releasePoint.name, releasePoint.gameObject);
        CastPoint = prevCastPoint;
        Target = prevTarget;
    }

    protected virtual void StandaloneCast(Transform releasePoint) {
        throw new NotImplementedException("This skill does not support standalone cast.");
    }

    #endregion

    public override bool CanCast(bool checkForTargetDistance = false) {
        if (CheckForDistance) {
            if (Vector3.Distance(UsePoint == Vector3.zero ? TargetManager.CurrentPlayersTargetPoint : UsePoint, transform.position)
                > skillProperties.maxCastRange.GetValue()) return false; 
        }

        return CharacterComponent.CharacterStatusEffectsManager.CanCast(this)
            && (skillProperties.chargeSystem.HasCharges() || CurrentCooldown <= 0)
            && CharacterComponent.CharacterStats.CurrentMana >= skillProperties.manaCost.GetValue();
    }

    public override void SkillAnimationStart() {
        base.SkillAnimationStart();
        ConsumeResource();
        //on skill cast event
    }

    public override void ConsumeResource() {
        CharacterComponent.CharacterStats.AddCurrentMana(-skillProperties.manaCost.GetValue());
    }

    public override bool CastSkill() {
        _ = base.CastSkill();
        CastPoint = CurrentDesiredCastPoint;
        Target = CurrentDesiredTarget;
        return true;
    }

    public override void SkillFired(SkillProperties skillProperties) {
        EventManager.OnPlayerSkillFire?.Invoke(skillProperties);
        base.SkillFired(skillProperties);
    }

    public override void TurnCharacterTowardsCastPoint(Vector3 point) {
        PlayerController.FacePointQuickly(point);
    }

    public override void TriggerOnSkillFiredEventExternally() {
        base.TriggerOnSkillFiredEventExternally();
        EventManager.OnPlayerSkillFire?.Invoke(skillProperties);
    }

    public bool UseSlottable() {
        return CastSkill();
    }

    /// <summary>
    /// Returns true if you can use current skill.
    /// By default this method overrides distance check, meaning the result does not include if the target point of the skill is in range. 
    /// Can send false to not override distance check, meaning the distance check will happen if CheckForDistance is set to true
    /// </summary>
    /// <param name="overrideDistanceCheck"></param>
    /// <returns></returns>
    public bool CanUse(bool overrideDistanceCheck = true) {
        bool prevState = CheckForDistance;
        bool result = false;

        if (overrideDistanceCheck) CheckForDistance = false;

        if (CanCast()) result = true;

        CheckForDistance = prevState;

        return result;
    }

    public void UseAmountChanged(ChargeSystem chargeSystem) {
        OnUsesAmountChanged?.Invoke(chargeSystem.CurrentCharges);
    }
   
    public int GetUsesAmount() {
        return skillProperties.chargeSystem.CurrentCharges;
    }

    public int GetMaxUsesAmount() {
        return skillProperties.chargeSystem.MaxCharges.GetValue();
    }

    public float GetUseRange() {
        return skillProperties.maxCastRange.GetValue();
    }

    public System.Text.StringBuilder GetTooltip() {
        return skillProperties.GetTooltip();
    }

    protected override void SetPooling(GameObject obj, int amount, int poolCapacity = 0) {
        if (!IsAwakePhase) {
            Debug.LogError("SetPooling in SkillTemplate must be called during Awake phase only!");
            return;
        }

        GameSceneManager.LatePostSceneLoadPhase.ExecuteSync(() => {
            ObjectPoolManager.PrePoolObjects(obj.name, obj, amount, poolCapacity);
        }, null, ExecuteAmount.Always);
    }
}
