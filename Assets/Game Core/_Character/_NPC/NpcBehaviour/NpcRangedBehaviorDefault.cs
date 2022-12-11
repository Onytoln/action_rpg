using UnityEngine;

public class NpcRangedBehaviorDefault : NPCBehavior {

    [SerializeField, Header("Ranged Behavior Settings")] protected float LoSCheckTime = 3f;
    [SerializeField] protected float LoSCheckRadius = 0.5f;
    protected float currentLineOfSightCheckTime;
    bool LoSCheckScheduled;
    bool noLoS;
    [SerializeField] protected float noLoSMinWalkTime = 2f;
    [SerializeField] protected float noLoSMaxWalkTime = 4f;
    float currentLoSWalkTime;

    [SerializeField] protected float targetMovedMetersBreakNoLoSWalk = 6f;
    float targetMovedMeters;

    public override void Start() {
        base.Start();
        noLoS = false;
        LoSCheckScheduled = false;
    }

    private void Update() {
        float deltaTime = Time.deltaTime;
        float unscaledDeltaTime = Time.unscaledDeltaTime;

        if (currentActionBlockTime >= 0) {
            currentActionBlockTime -= unscaledDeltaTime;
            return;
        }

        if (NPCState == NpcState.Interrupted) {
            return;
        }

        currentNoTargetRefreshTime += deltaTime;
        if (currentNoTargetRefreshTime >= noTargetRefreshTime) {
            currentNoTargetRefreshTime = 0;
            if (Target == null && !GetTarget()) {
                WalkBackToBehaviourRetreatPointNonReset();
                return;
            }
        }

        if (Target == null) return;

        if (!noLoS) {
            if (!LoSCheckScheduled) {
                currentLineOfSightCheckTime += deltaTime;
                if (currentLineOfSightCheckTime >= LoSCheckTime) {
                    currentLineOfSightCheckTime = 0f;
                    CheckLoS();
                }
            }
        } else {
            currentLoSWalkTime -= deltaTime;
            targetMovedMeters += TargetDistanceTraveledThisFrame;
            if (currentLoSWalkTime <= 0f || targetMovedMeters >= targetMovedMetersBreakNoLoSWalk) {
                GainedLoS();
                currentLineOfSightCheckTime = LoSCheckTime;
                return;
            }
        }

        currentDistanceFromSpawnPointTime += unscaledDeltaTime;
        if (currentDistanceFromSpawnPointTime > 4) {
            currentDistanceFromSpawnPointTime = 0;
            if (Vector3.Distance(spawnPos, transform.position) > MaxDistanceFromSpawnPoint && NPCState != NpcState.WalkingBackToSpawnPoint) {
                WalkBackToBehaviourRetreatPointReset();
            }
        }

        if (NPCState == NpcState.Attacking || NPCState == NpcState.WalkingBackToSpawnPoint) return;

        CurrentTargetTimeoutTime -= deltaTime;
        if (CurrentTargetTimeoutTime <= 0) {
            if (!GetTarget()) {
                WalkBackToBehaviourRetreatPointNonReset();
            }
        }

        currentDistanceFromTargetTime += deltaTime;
        if (currentDistanceFromTargetTime >= 0.25f) {
            currentDistanceFromTargetTime = 0f;
            _ = CalculateDistanceFromTarget();

            if (!noLoS) {
                if (UseSkill()) return;
            }
        }

        currentPathRefreshTime += deltaTime;
        if (currentPathRefreshTime >= pathRefreshTime && StatusEffectsManager.CanMove() && DistanceFromTarget <= LookRadius
            && DistanceFromTarget > NpcController.agent.stoppingDistance) {
            MoveTowardsTarget(Target);
            currentPathRefreshTime = 0f;
        } else if (currentPathRefreshTime >= 0.4f && DistanceFromTarget > LookRadius) {
            currentPathRefreshTime = 0f;
            WalkBackToBehaviourRetreatPointReset();
        }
    }

    protected override void GetMinMaxSkillDistances() {
        base.GetMinMaxSkillDistances();
        NpcController.AgentStoppingDistance = MaxSkillDistance;
    }

    private void CheckLoS() {
        if (LoSCheckScheduled) return;
        Scheduler.ScheduleSphereCast(GetOrigin, LoSCheckRadius, GetDirection, CalculateDistanceFromTarget(), DataStorage.GroundTerrainLayerBitMask, SphereCastFinished);
        LoSCheckScheduled = true;
    }

    private Vector3 GetOrigin() => transform.position + (Vector3.up * DataStorage.AOBJ_RELEASE_Y_DEFAULT_OFFSET);

    private Vector3 GetDirection() => DirectionToTargetNormalized;

    private void SphereCastFinished(RaycastHit raycastHit) {
        if (raycastHit.collider == null) {
            GainedLoS();
        } else {
            LostLoS();
        }

        LoSCheckScheduled = false;
    }

    private void LostLoS() {
        noLoS = true;
        currentLoSWalkTime = Random.Range(noLoSMinWalkTime, noLoSMaxWalkTime);
        targetMovedMeters = 0f;
        NpcController.AgentStoppingDistance = DataStorage.MIN_STOPPING_DISTANCE;
    }

    private void GainedLoS() {
        noLoS = false;
        NpcController.AgentStoppingDistance = MaxSkillDistance;
    }
}
