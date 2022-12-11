using MEC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

public class PlayerMotor : MonoBehaviour {
    public NavMeshAgent agent { get; private set; }
    private Transform target;
    private RaycastHit hit;
    //public LayerMask interactableMask;
    private CoroutineHandle currentFacePointCoroutine;
    private Animator animator;
    private float defaultAnimationRunSpeed = 0.83f;
    private float animationRunSpeedMax = 1.6f;
    private float characterMovementSpeedDefault;
    private float characterMovementSpeedMax;

    private NavMeshPath path;

    void Start() {
        path = new NavMeshPath();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        PlayerStats playerStats = GetComponent<PlayerStats>();
        playerStats.OnCharacterStatChange += UpdateAgentSpeed;
        Stat movementSpeedStat = playerStats.GetStat(StatType.MovementSpeed);
        characterMovementSpeedDefault = movementSpeedStat.GetPrimaryValue();
        characterMovementSpeedMax = movementSpeedStat.GetMaxPossibleValue();
        UpdateAgentSpeed(movementSpeedStat);
    }

    void Update() {
        if (target != null && agent.enabled) {
            MoveToPoint(target.position);
        }
    }

    public void MoveToPoint(Vector3 point) {
        if (agent.enabled) {
            SetPath(point);
        }
    }

    private void SetPath(Vector3 destination) {
        if (agent.destination == destination) return;
        NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
        agent.SetPath(path);
        agent.updateRotation = true;
    }

    public void FollowTarget(Interactable newTarget) {
        if (agent.enabled) {
            agent.stoppingDistance = newTarget.radiusItem;
            agent.updateRotation = false;
            target = newTarget.interactionTransform;
            Timing.KillCoroutines(currentFacePointCoroutine);
            currentFacePointCoroutine = Timing.RunCoroutine(FaceTargetCoroutine());
        }
    }

    public void StopFollowingTarget() {
        if (agent.enabled) {
            agent.stoppingDistance = 0.3f;
            agent.updateRotation = true;
            target = null;
            Timing.KillCoroutines(currentFacePointCoroutine);
        }
    }

    public void FullStop() {
        StopFollowingTarget();
        if (agent.hasPath) {
            agent.ResetPath();
        }
    }

    private void UpdateAgentSpeed(Stat stat) {
        if (stat.statType != StatType.MovementSpeed) return;

        agent.speed = stat.GetValue();

        float defaultSpeedDifference = (stat.GetValue() - characterMovementSpeedDefault) / (characterMovementSpeedMax - characterMovementSpeedDefault);
        float animationSpeedFinal = defaultAnimationRunSpeed + (animationRunSpeedMax - defaultAnimationRunSpeed) * defaultSpeedDifference;
        animator.SetFloat("runSpeed", animationSpeedFinal);
    }

    private void FaceTarget() {
        Vector3 direction = (target.position - transform.position).normalized;
        if (direction == Vector3.zero) return;

        Quaternion lookRoation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRoation, Time.deltaTime * 12f);
    }

    public IEnumerator<float> FaceTargetCoroutine() {
        Vector3 lastTargetPos = Vector3.zero;

        while (target != null) {
            if (Vector3.Distance(target.position, transform.position) < 4) {
                FaceTarget();
                lastTargetPos = target.position;
            }

            yield return Timing.WaitForOneFrame;
        }

        FacePointQuickly(lastTargetPos);
    }

    public void FacePointQuickly(Vector3 point, float fullRotationInSeconds = 0.1f) {
        if (point != Vector3.zero) {
            Timing.KillCoroutines(currentFacePointCoroutine);
            currentFacePointCoroutine = Timing.RunCoroutine(FacePointQuicklyCoroutine(point, fullRotationInSeconds));
        }
    }

    public IEnumerator<float> FacePointQuicklyCoroutine(Vector3 point, float fullRotationInSeconds) {
        float time = 0f;
        float multiplier = 1f / fullRotationInSeconds;

        Vector3 direction = (point - transform.position).normalized;
        if (direction == Vector3.zero) yield break;

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
