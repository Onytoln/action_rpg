using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour, ICharacterControllerKeyFunc {

    public Action onStuck;
    public Action onConsecutiveStucks;
    private int stuckCount;
    private float stuckTime;

    private Animator animator;

    public Transform target { get; private set; }
    private Vector3 lastTargetPoint;

    public float TargetDistanceTraveledThisFrame { get; private set; }
    private Vector3 lastTargetPointPerFrame;

    public NavMeshAgent agent { get; private set; }
    private bool waitingForPath = false;
    public bool AgentEnabled => agent.enabled;

    public float AgentStoppingDistance {
        get => agent.stoppingDistance;
        set => agent.stoppingDistance = value < DataStorage.MIN_STOPPING_DISTANCE ? DataStorage.MIN_STOPPING_DISTANCE : value;
    }

    private NavMeshObstacle obstacle;
    public bool ObstacleEnabled => obstacle.enabled;

    private BoolControlSourced obstacleDisabledExternally = new BoolControlSourced(BoolType.False, true);
    private BoolControlSourced npcControllerDisabledExternally = new BoolControlSourced(BoolType.False);

    private bool nextFrame;

    private StatusEffectsManager statusEffectsManager;

    private CoroutineHandle currentFacePointCoroutine;
    private bool facingTarget;
    private bool canRotate;

    public Vector3 lastPos { get; private set; }

    [SerializeField]
    private float defaultAnimationRunSpeed = 1;
    [SerializeField]
    private float animationRunSpeedMax = 1.8f;
    private float characterMovementSpeedDefault;
    private float characterMovementSpeedMax;

    private int movingAnimHash = Animator.StringToHash("IsMoving");
    private bool prevMovingState = false;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        animator = GetComponent<Animator>();
        statusEffectsManager = gameObject.GetComponent<StatusEffectsManager>();
        canRotate = false;
        waitingForPath = false;
        lastPos = transform.position;
        /*UnityEditorInternal.ComponentUtility.CopyComponent(chargeSkill);
        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(ComponentAddTest.instance.gameObject);*/
    }

    void Start() {
        CharacterStats npcStats = GetComponent<CharacterStats>();
        npcStats.OnCharacterStatChange += UpdateAgentSpeed;
        ChracterStat movementSpeedStat = npcStats.GetStat(StatType.MovementSpeed);
        characterMovementSpeedDefault = movementSpeedStat.GetPrimaryValue();
        characterMovementSpeedMax = movementSpeedStat.GetMaxPossibleValue();
        UpdateAgentSpeed(movementSpeedStat);
    }

    void Update() {
        if (target != null) {
            TargetDistanceTraveledThisFrame = Vector3.Distance(lastTargetPointPerFrame, lastTargetPointPerFrame);
            lastTargetPointPerFrame = target.position;
        }

        if (npcControllerDisabledExternally.Value) return;

        nextFrame = true;
        if (!statusEffectsManager.IsStationary && Vector3.Distance(lastPos, transform.position) < agent.speed * Time.deltaTime * 0.4f) {
            stuckTime += Time.deltaTime;
            if (stuckTime >= 1.5f) {
                StopMovement();
                stuckTime = 0;
                onStuck?.Invoke();

                stuckCount += 1;
                if (stuckCount == 4) {
                    stuckCount = 0;
                    onConsecutiveStucks?.Invoke();
                }
            }
        } else {
            stuckTime = 0;
            stuckCount = 0;
        }

        if (transform.position != lastPos) {
            SetMovingAnim(true);
            statusEffectsManager.SetIsStationary(false);
        } else {
            SetMovingAnim(false);
            statusEffectsManager.SetIsStationary(true);
        }
        lastPos = transform.position;

        if (!obstacleDisabledExternally.Value) {
            EnableObstacle();
        } else {
            DisableObstacle();

            if (!agent.enabled && nextFrame) {
                agent.enabled = true;
            }
        }

        if (canRotate) {
            FaceTargetDirection();
        }
    }

    private void EnableObstacle() {
        if (agent.enabled) {
            agent.enabled = false;
            obstacle.enabled = true;
        }
    }

    private void DisableObstacle() {
        if (obstacle.enabled) {
            obstacle.enabled = false;
            nextFrame = false;
        }
    }

    public void SetTarget(Transform target) {
        if (target == null) return;
        this.target = target;
    }

    public bool MoveTowardsTarget(Transform target) {
        if (waitingForPath) return false; 
        SetTarget(target);
        if (!agent.enabled || !agent.isOnNavMesh) return false;
        if (agent.pathPending) return false;
        if (!statusEffectsManager.CanMove()) return false;
        if (agent.hasPath && target.position == lastTargetPoint) return true;

        waitingForPath = true;

        Scheduler.ExecuteDelayed(() => {
            _ = Scheduler.EnqueueAction(() => {
                if (agent.enabled && agent.isOnNavMesh) {
                    agent.SetDestination(target.position);
                    agent.updateRotation = true;
                }

                waitingForPath = false;
            }, ActionQueueType.NavMeshAgentSetDestination);
        }, 2);

        lastTargetPoint = target.position;
        return true;
    }

    public bool MoveToPoint(Vector3 point) {
        if (point == Vector3.zero) return false;
        if (waitingForPath) return false;
        if (!agent.enabled || !agent.isOnNavMesh) return false;
        if (agent.pathPending) return false;
        if (!statusEffectsManager.CanMove()) return false;
        if (agent.hasPath && point == agent.destination) return true;

        waitingForPath = true;

        Scheduler.ExecuteDelayed(() => { 
            _ = Scheduler.EnqueueAction(() => {
                if (agent.enabled && agent.isOnNavMesh) {
                    agent.SetDestination(point);
                    agent.updateRotation = true;
                }

                waitingForPath = false;
            }, ActionQueueType.NavMeshAgentSetDestination);
        }, 2);

        return true;
    }

    public void ResetTarget() {
        target = null;
        lastTargetPoint = Vector3.zero;
    }

    public void AllowRotation() {
        canRotate = true;
    }

    public void ForbidRotation() {
        canRotate = false;
    }

    /// <summary>
    /// External is anything that isn't npc behaviour
    /// </summary>
    /// <param name="source"></param>
    /// <param name="disable"></param>
    public void ExternalObstacleDisable(bool disable, object source) {
        obstacleDisabledExternally.Set(disable, source);

        if (obstacleDisabledExternally.Value) {
            DisableObstacle();
        }
    }

    /// <summary>
    /// External is anything that isn't npc behaviour
    /// </summary>
    /// <param name="source"></param>
    /// <param name="disable"></param>
    public void ExternalControllerDisable(bool disable, object source) {
        npcControllerDisabledExternally.Set(disable, source);
    }

    public void ExternalObstacleAndControllerDisable(bool disable, object source) {
        ExternalObstacleDisable(disable, source);
        ExternalControllerDisable(disable, source);
    }

    public void StopMovement() {
        //stop movement implementation
        if (agent.enabled)
            agent.ResetPath();
    }

    void UpdateAgentSpeed(ChracterStat stat) {
        if (stat.statType != StatType.MovementSpeed) return;

        agent.speed = stat.GetValue();

        float defaultSpeedDifference = (stat.GetValue() - characterMovementSpeedDefault) / (characterMovementSpeedMax - characterMovementSpeedDefault);
        float animationSpeedFinal = defaultAnimationRunSpeed + (animationRunSpeedMax - defaultAnimationRunSpeed) * defaultSpeedDifference;
        animator.SetFloat("runSpeed", animationSpeedFinal);
    }

    public void SetMovingAnim(bool state) {
        if (prevMovingState == state) return;
        prevMovingState = state;

        animator.SetBool(movingAnimHash, state);
    }

    private void FaceTargetDirection() {
        if (facingTarget) return;

        Timing.KillCoroutines(currentFacePointCoroutine);
        currentFacePointCoroutine = Timing.RunCoroutine(FaceTargetDirectionCoroutine().CancelWith(gameObject));
    }

    IEnumerator<float> FaceTargetDirectionCoroutine() {
        facingTarget = true;

        while (canRotate && target != null) {
            FaceTarget();
            yield return Timing.WaitForOneFrame;
        }

        facingTarget = false;
    }

    void FaceTarget() {
        if (target == null) return;
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    void StopFacingTarget() {
        Timing.KillCoroutines(currentFacePointCoroutine);
        facingTarget = false;
    }

    public void FacePointQuickly(Vector3 point, float fullRotationInSeconds = 0.1f) {
        if (point != Vector3.zero) {
            StopFacingTarget();
            currentFacePointCoroutine = Timing.RunCoroutine(FacePointQuicklyCoroutine(point, fullRotationInSeconds).CancelWith(gameObject));
        }
    }

    private IEnumerator<float> FacePointQuicklyCoroutine(Vector3 point, float fullRotationInSeconds) {
        float time = 0f;
        float multiplier = 1f / fullRotationInSeconds;

        Vector3 direction = (point - transform.position).normalized;
        Quaternion initialRotation = transform.rotation;

        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));

        while (Vector3.Angle(transform.forward, direction) > 10f && time < 1f) {
            transform.rotation = Quaternion.Slerp(initialRotation, lookRotation, time);

            time += Time.deltaTime * multiplier;
            yield return Timing.WaitForOneFrame;
        }

        transform.rotation = lookRotation;
    }
}
