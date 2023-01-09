using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCBehavior : MonoBehaviour {

    //self
    public Character CharacterComponent { get; private set; }
    public NPCController NpcController { get; private set; }
    public StatusEffectsManager StatusEffectsManager { get; private set; }

    protected NpcState npcState;
    private NpcState prevState;
    public NpcState NPCState {
        get => npcState;
        set {
            NpcState prevValue = value;

            SetNpcState(value);

            if (prevValue != npcState)
                OnNpcStateChanged?.Invoke(this, npcState);
        }
    }

    public event Action<NPCBehavior, NpcState> OnNpcStateChanged;

    /// <summary>
    /// optional target point
    /// </summary>
    public Vector3 TargetPoint { get; protected set; }
    [field: SerializeField, Header("Pathing")] protected float LookRadius { get; private set; } = 20;
    [field: SerializeField] protected float MaxDistanceFromSpawnPoint { get; private set; } = 40;
    protected Vector3 spawnPos;
    protected float currentDistanceFromSpawnPointTime; //has static refresh time of 4f - depends on the behaviour
    [SerializeField] protected float pathRefreshTime = 0.8f;
    protected float currentPathRefreshTime;

    //target
    private TargetManager targetManager;
    public TargetManager TargetManager {
        get { if (targetManager == null) { targetManager = TargetManager.Instance; } return targetManager; }
    }


    public Transform Target { get; private set; }
    public Character TargetCharacterComponent { get; private set; }
    public event Action<Character> OnTargetAcquired;
    public Vector3 DirectionToTarget => Target.position - transform.position;
    public Vector3 DirectionToTargetNormalized => (Target.position - transform.position).normalized;
    public float TargetDistanceTraveledThisFrame => NpcController.TargetDistanceTraveledThisFrame;

    private float distanceFromTarget;
    public float DistanceFromTarget { get => distanceFromTarget; }
    protected float currentDistanceFromTargetTime; //has static refresh time of 0.25f - depends on the behaviour

    private bool hadTarget;
    [SerializeField, Header("Target")] protected float maxTargetTimeoutTime = 6f;
    [SerializeField] protected float targetTimeoutTime = 5f;
    private float currentTargetTimeoutTime;
    protected float CurrentTargetTimeoutTime {
        get => currentTargetTimeoutTime;
        set {
            currentTargetTimeoutTime = value;
            if (currentTargetTimeoutTime > maxTargetTimeoutTime) currentTargetTimeoutTime = maxTargetTimeoutTime;
        }
    }

    [SerializeField] protected float noTargetRefreshTime = 1f;
    protected float currentNoTargetRefreshTime;

    [Header("Action block time")]
    [SerializeField] protected float postAttackActionBlockTime = 1.5f;
    [SerializeField] protected float postAttackActionBlockTimeChargeNotEmpty = 0.3f;
    protected float currentActionBlockTime;

    public IEnumerator<float> WalkBackToSpawnNonResetCoroutine { get; private set; }
    public CoroutineHandle WalkBackToSpawnNonResetCoroutineHandle { get; private set; }
    protected bool killWalkBackToSpawnNonReset;

    public IEnumerator<float> WalkBackToSpawnReset { get; private set; }
    public CoroutineHandle WalkBackToSpawnResetCoroutineHandle { get; private set; }
    private bool killWalkBackToSpawnReset;

    //Skills
    public SkillTemplate[] Skills { get; private set; }
    private int loadedSkills = 0;
    private float lastUseSkillTime;

    public float LastUserSkillTime => lastUseSkillTime;

    private float minSkillDistanceDefault;
    public float MinSkillDistanceDefault => minSkillDistanceDefault;
    public float MinSkillDistance { get; protected set; }

    private float maxSkillDistanceDefault;
    public float MaxSkillDistancedefault => maxSkillDistanceDefault;
    public float MaxSkillDistance { get; protected set; }

    protected static readonly int onHitHash = Animator.StringToHash("OnHit");

    public virtual void Awake() {
        NpcController = GetComponent<NPCController>();
        StatusEffectsManager = GetComponent<StatusEffectsManager>();
        CharacterComponent = GetComponent<Character>();

        StatusEffectsManager.OnCompleteControlDisabled += OnCompleteControlDisabled;
        StatusEffectsManager.OnIsStationaryChanged += OnStationary;
        StatusEffectsManager.OnIsCastingChanged += OnIsCastingChanged;
        StatusEffectsManager.OnIsChannelingChanged += OnIsChannelingChanged;
        NpcController.onConsecutiveStucks += OnConsecutiveStuck;

        CharacterComponent.CharacterCombat.OnHitTaken += OnHitTaken;
        CharacterComponent.CharacterCombat.OnNonHitDamageTaken += OnStatusEffectDamageDone;

        OnTargetAcquired += OnTargetAcquiredAIResponse;
        CharacterComponent.DisableCharacter();

        Skills = CharacterComponent.GetCharacterSkills();
        for (int i = 0; i < Skills.Length; i++) {
            LoadHandler.NotifyOnLoad(Skills[i], OnSkillLoaded);
        }

        NPCState = NpcState.Idle;
    }

    public virtual void Start() {
        GetTarget();
        spawnPos = transform.position;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, LookRadius);
    }

    protected float CalculateDistanceFromTarget() {
        return distanceFromTarget = Vector3.Distance(Target.position, transform.position);
    }

    protected virtual void SetNpcState(NpcState state) {
        if (state == npcState) return;

        switch (state) {
            case NpcState.Idle:
                if (!CharacterComponent.CharacterStatusEffectsManager.IsStationary) return;
                npcState = NpcState.Idle;

                if (prevState != NpcState.Interrupted)
                    EnableRotation();

                EnableObstacle();
                break;
            case NpcState.FollowingTarget:
                if (npcState == NpcState.Attacking || npcState == NpcState.Interrupted || npcState == NpcState.WalkingBackToSpawnPoint) return;
                npcState = NpcState.FollowingTarget;
                DisableObstacle();
                break;
            case NpcState.Attacking:
                if (npcState == NpcState.Interrupted || npcState == NpcState.WalkingBackToSpawnPoint) return;
                npcState = NpcState.Attacking;
                DisableRotation();
                EnableObstacle();
                break;
            case NpcState.Interrupted:
                if (npcState == NpcState.WalkingBackToSpawnPoint) return;
                npcState = NpcState.Interrupted;
                DisableRotation();
                EnableObstacle();
                break;
            case NpcState.WalkingBackToSpawnPoint:
                npcState = NpcState.WalkingBackToSpawnPoint;
                DisableRotation();
                DisableObstacle();
                break;
        }

        prevState = npcState;
    }

    protected void EnableObstacle() {
        NpcController.ExternalObstacleDisable(false, this);
    }

    protected void DisableObstacle() {
        NpcController.ExternalObstacleDisable(true, this);
    }

    protected void EnableRotation() {
        NpcController.AllowRotation();
    }

    protected void DisableRotation() {
        NpcController.ForbidRotation();
    }

    protected bool MoveTowardsTarget(Transform target) {
        NPCState = NpcState.FollowingTarget;
        return NpcController.MoveTowardsTarget(target);
    }

    protected bool MoveTowardsPoint(Vector3 point) {
        return NpcController.MoveToPoint(point);
    }

    public bool GetTarget() {
        Transform temp = TargetManager.AcquireTarget(gameObject.tag);
        if (Target == temp) {
            CurrentTargetTimeoutTime = targetTimeoutTime;
            return true;
        }

        Target = temp;

        if (Target == null) {
            if (hadTarget) {
                hadTarget = false;
            }
            return false;
        } else {
            TargetCharacterComponent = Target.GetComponent<Character>();
            if (TargetCharacterComponent.CharacterStatusEffectsManager.IsDead || TargetCharacterComponent.CharacterStatusEffectsManager.IsRespawning) {
                Target = null;

                if (hadTarget) {
                    hadTarget = false;
                }
                return false;
            }
        }

        hadTarget = true;

        CurrentTargetTimeoutTime = targetTimeoutTime;

        TargetCharacterComponent.OnCharacterDeath += OnTargetDeath;
        TargetCharacterComponent.CharacterStatusEffectsManager.OnIsUntargetableChanged += OnTargetUntargetable;

        NpcController.SetTarget(Target);

        OnTargetAcquired?.Invoke(TargetCharacterComponent);
        return true;
    }

    //needs to change
    public virtual void OnTargetDeath(Character character) {
        if (TargetCharacterComponent == null) return;
        TargetCharacterComponent.OnCharacterDeath -= OnTargetDeath;
        TargetCharacterComponent = null;
        Target = null;
        NpcController.ResetTarget();
        GetTarget();
    }

    //needs to change
    public virtual void OnTargetUntargetable(bool state) {
        if (!state) return;
        if (TargetCharacterComponent == null || Target == null) return;
        TargetCharacterComponent.OnCharacterDeath -= OnTargetDeath;
        TargetCharacterComponent.CharacterStatusEffectsManager.OnIsUntargetableChanged -= OnTargetUntargetable;
        TargetCharacterComponent = null;
        Target = null;
    }

    public void OnSkillLoaded(ILoadable loadable) {
        loadedSkills++;
        if (loadedSkills == Skills.Length) {
            LoadHandler.NotifyOnLoad(ExperienceManager.Instance, ProcessSkills);
        }
    }

    private void ProcessSkills(ILoadable loadable) {
        if (Skills.Length > 1) {
            int levelValidSkills = 0;
            int characterLevel = CharacterComponent.CharacterLevel;

            for (int i = 0; i < Skills.Length; i++) {
                if (characterLevel >= Skills[i].skillProperties.MinLevelRequired) levelValidSkills++;
            }

            SkillTemplate[] validAbilities = new SkillTemplate[levelValidSkills];
            int validAbilIndex = 0;

            for (int i = 0; i < Skills.Length; i++) {
                if (characterLevel >= Skills[i].skillProperties.MinLevelRequired) {
                    validAbilities[validAbilIndex] = Skills[i];
                    validAbilIndex++;
                }
            }

            Skills = validAbilities;
        }

        GetMinMaxSkillDistances();
    }

    protected virtual void GetMinMaxSkillDistances() {
        MinSkillDistance = float.PositiveInfinity;
        MaxSkillDistance = float.NegativeInfinity;

        for (int i = 0; i < Skills.Length; i++) {
            float skillRange = Skills[i].skillProperties.maxCastRange.GetValue();
            //Debug.Log(Skills[i].skillProperties.castRange.GetValue());
            if (MinSkillDistance > skillRange) MinSkillDistance = skillRange;
            if (MaxSkillDistance < skillRange) MaxSkillDistance = skillRange;
        }

        MinSkillDistance = MinSkillDistance >= (DataStorage.MIN_STOPPING_DISTANCE + DataStorage.SKILL_DISTANCE_OFFSET) ?
            (MinSkillDistance - DataStorage.SKILL_DISTANCE_OFFSET) : DataStorage.MIN_STOPPING_DISTANCE;

        MaxSkillDistance = MaxSkillDistance >= (DataStorage.MIN_STOPPING_DISTANCE + DataStorage.SKILL_DISTANCE_OFFSET) ?
            (MaxSkillDistance - DataStorage.SKILL_DISTANCE_OFFSET) : MinSkillDistance;

        minSkillDistanceDefault = MinSkillDistance;
        maxSkillDistanceDefault = MaxSkillDistance;
    }

    public virtual void OnCompleteControlDisabled(bool interrupted) {
        if (interrupted) {
            NPCState = NpcState.Interrupted;
        } else {
            NPCState = NpcState.Idle;
        }
    }

    public virtual void OnStationary(bool stationary) {
        if (NPCState == NpcState.Attacking) return;

        NPCState = NpcState.Idle;
    }

    public virtual void OnIsCastingChanged(bool state) {
        if (state) {
            NPCState = NpcState.Attacking;
        } else {
            NPCState = NpcState.Idle;
        }
    }

    public virtual void OnIsChannelingChanged(bool state) { }

    public virtual void OnStuck() {
        if (enabled && currentActionBlockTime < 0.5f) {
            currentActionBlockTime = 0.5f;
            NpcController.StopMovement();
        }
    }
    public virtual void OnConsecutiveStuck() {
        if (enabled && currentActionBlockTime < 3f) {
            currentActionBlockTime = 3f;
            NpcController.StopMovement();
        }
    }

    public virtual void OnHitTaken(HitOutput hitOutput) {
        if (!enabled) CharacterComponent.ActivateTarget();

        Transform compareTransform = hitOutput.HitSourceCharacterComponent.transform;
        /*Debug.Log("whole " + hitOutput.hitInput.AbilityProperties.CharacterComponent.transform);
        Debug.Log("char comp " + hitOutput.hitInput.AbilityProperties.CharacterComponent);
        Debug.Log("ability prop" + hitOutput.hitInput.AbilityProperties);
        Debug.Log("hit input " + hitOutput.hitInput);
        Debug.Log("hit output " + hitOutput);*/
        if (compareTransform != null && compareTransform == Target) {
            CurrentTargetTimeoutTime += 1.6f;
        } else {
            CurrentTargetTimeoutTime -= 0.8f;
        }

        if (CurrentTargetTimeoutTime <= 0) GetTarget();

        StatusEffectsManager st = CharacterComponent.CharacterStatusEffectsManager;
        if (st.IsCasting || st.IsChanneling || st.IsStunned) return;
        CharacterComponent.CharacterAnimator.SetTrigger(onHitHash);
    }

    public virtual void OnStatusEffectDamageDone(HitOutput hitOutput) {
        if (!enabled) CharacterComponent.ActivateTarget();

        /*StatusEffectsManager st = CharacterComponent.CharacterStatusEffectsManager;
        if (st.IsCasting || st.IsChanneling || st.IsStunned) return;
        CharacterComponent.CharacterAnimator.SetTrigger(onHitHash);*/
    }

    public virtual bool UseSkill() {
        for (int i = 0; i < Skills.Length; i++) {
            if (Skills[i].CastSkill()) {
                Skills[i].OnFullCastDone += PostSkillCast;
                lastUseSkillTime = Time.time;
                return true;
            }
        }
        return false;
    }

    public virtual bool UseSkill(SkillType skillType) {
        return false;
    }

    public virtual void PostSkillCast(SkillTemplate skill) {
        if (skill is NpcSkillTemplate npcSkill) {
            if (npcSkill.skillProperties.chargeSystem.ChargeSystemBeingUsed() && npcSkill.skillProperties.chargeSystem.CurrentCharges > 0) {
                if (npcSkill.postAttackActionBlockTimeChargeNotEmpty > 0) {
                    currentActionBlockTime = npcSkill.postAttackActionBlockTimeChargeNotEmpty;
                } else {
                    currentActionBlockTime = postAttackActionBlockTimeChargeNotEmpty;
                }
            } else {
                if (npcSkill.postAttackActionBlockTime > 0) {
                    currentActionBlockTime = npcSkill.postAttackActionBlockTime;
                } else {
                    currentActionBlockTime = postAttackActionBlockTime;
                }
            }
        }
        skill.OnFullCastDone -= PostSkillCast;
        NPCState = NpcState.Idle;
    }

    public bool AtSpawnPoint() {
        if (Vector3.Distance(transform.position, spawnPos) <= NpcController.agent.stoppingDistance + 0.2f) {
            return true;
        }

        return false;
    }


    protected virtual void OnTargetAcquiredAIResponse(Character characterComponent) { }

    #region Walk Back To Spawn NON-RESET

    public virtual void WalkBackToBehaviourRetreatPointNonReset() {
        if (WalkBackToSpawnNonResetCoroutine != null || AtSpawnPoint()) return;
        killWalkBackToSpawnNonReset = false;
        killWalkBackToSpawnReset = true;
        WalkBackToSpawnNonResetCoroutine = WalkBackToSpawnPointNonReset();
        WalkBackToSpawnNonResetCoroutineHandle = Timing.RunCoroutine(WalkBackToSpawnNonResetCoroutine.CancelWith(gameObject));
    }

    private IEnumerator<float> WalkBackToSpawnPointNonReset() {
        bool loop = true;

        CharacterComponent.ResetCoreStatusEffectParameters(false);
        NPCState = NpcState.WalkingBackToSpawnPoint;

        while (loop && !killWalkBackToSpawnNonReset) {
            if (MoveTowardsPoint(spawnPos)) {
                NpcController.SetMovingAnim(true);
                if (Vector3.Distance(spawnPos, transform.position) <= NpcController.agent.stoppingDistance + 3f) {
                    loop = false;
                }
            }
            yield return Timing.WaitForSeconds(0.5f);
        }

        CharacterComponent.ResetCoreStatusEffectParameters(true);
        NpcController.ResetTarget();
        NpcController.SetMovingAnim(true);

        CharacterComponent.EnableCharacter();

        CharacterComponent.CharacterStatusEffectsManager.SetIsUntargetable(true);
        CharacterComponent.CharacterStatusEffectsManager.SetIsUntargetable(false);

        NPCState = NpcState.Idle;
        WalkBackToSpawnNonResetCoroutine = null;
    }

    #endregion

    #region Walk To Spawn RESET

    public virtual void WalkBackToBehaviourRetreatPointReset() {
        if (WalkBackToSpawnReset != null) return;
        killWalkBackToSpawnReset = false;
        killWalkBackToSpawnNonReset = true;
        WalkBackToSpawnReset = WalkBackToSpawnPointReset();
        WalkBackToSpawnResetCoroutineHandle = Timing.RunCoroutine(WalkBackToSpawnReset);
    }

    IEnumerator<float> WalkBackToSpawnPointReset() {
        bool loop = true;

        CharacterComponent.ResetCoreStatusEffectParameters(false);

        UnityEngine.AI.ObstacleAvoidanceType avoidance = NpcController.agent.obstacleAvoidanceType;

        CharacterComponent.CharacterStatusEffectsManager.ClearStatusEffectsOnDeath();
        CharacterComponent.CharacterStatusEffectsManager.EndSkillCasts();
        CharacterComponent.CharacterStatusEffectsManager.SetIsUntargetable(true);

        CharacterComponent.CharacterStats.AddAbsoluteStat(CharacterStatType.MovementSpeed, 6f);
        NpcController.agent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance;
        NPCState = NpcState.WalkingBackToSpawnPoint;
        while (!NpcController.AgentEnabled) {
            yield return Timing.WaitForOneFrame;
        }
        CharacterComponent.DeactivateTarget();

        float distanceOffset = 0.8f;
        while (loop && !killWalkBackToSpawnReset) {
            if (MoveTowardsPoint(spawnPos)) {
                NpcController.SetMovingAnim(true);
                if (Vector3.Distance(spawnPos, transform.position) <= NpcController.agent.stoppingDistance + distanceOffset) {
                    loop = false;
                } else {
                    distanceOffset += 0.05f;
                }
            }
            yield return Timing.WaitForSeconds(0.5f);
        }

        CharacterComponent.ResetCoreStatusEffectParameters(true);

        NpcController.ResetTarget();
        NpcController.SetMovingAnim(false);

        CharacterComponent.CharacterStats.SetCurrentHealth(CharacterComponent.CharacterStats.CoreStats.HealthValue);
        CharacterComponent.CharacterStats.RemoveAbsoluteStat(CharacterStatType.MovementSpeed, 6f);

        CharacterComponent.CharacterStatusEffectsManager.SetIsUntargetable(false);

        NpcController.agent.obstacleAvoidanceType = avoidance;

        NPCState = NpcState.Idle;
        WalkBackToSpawnReset = null;

        if (Target != null && Vector3.Distance(Target.position, transform.position) <= LookRadius) {
            CharacterComponent.ActivateTarget();
        }
    }

    #endregion
}
