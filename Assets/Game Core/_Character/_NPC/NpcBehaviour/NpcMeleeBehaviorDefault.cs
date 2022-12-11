using UnityEngine;

public class NpcMeleeBehaviorDefault : NPCBehavior {

    private void Update() {
        float deltaTime = Time.deltaTime;
        float unscaledDeltaTime = Time.unscaledDeltaTime;

        if (currentActionBlockTime > 0) {
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

        currentDistanceFromSpawnPointTime += unscaledDeltaTime;
        if (currentDistanceFromSpawnPointTime >= 4f) {
            currentDistanceFromSpawnPointTime = 0f;
            if (Vector3.Distance(spawnPos, transform.position) > MaxDistanceFromSpawnPoint && NPCState != NpcState.WalkingBackToSpawnPoint) {
                WalkBackToBehaviourRetreatPointReset();
            }
        }

        if (NPCState == NpcState.Attacking || NPCState == NpcState.WalkingBackToSpawnPoint) return;

        CurrentTargetTimeoutTime -= deltaTime;
        if(CurrentTargetTimeoutTime <= 0f) {
            if (!GetTarget()) {
                WalkBackToBehaviourRetreatPointNonReset();
            }
        }

        currentDistanceFromTargetTime += deltaTime;
        if (currentDistanceFromTargetTime >= 0.25f) {
            currentDistanceFromTargetTime = 0f;
            _ = CalculateDistanceFromTarget();

            if (UseSkill()) return;
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
}

