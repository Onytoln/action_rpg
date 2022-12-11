using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcSummonMeleeDefaultBehavior : NPCSummonBehavior {

    public Character Summoner { get; private set; }
    public Transform SummonerTransform { get; private set; }

    public override void Start() {
        base.Start();
        Summoner = (CharacterComponent as Summon)?.Summoner;

        if(Summoner == null) {
            CharacterComponent.CharacterStats.SetCurrentHealth(0);
            return;
        }

        SummonerTransform = Summoner.transform;
        currentNoTargetRefreshTime = noTargetRefreshTime;
    }

    void Update()
    {
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
                FollowMaster(SummonerTransform);
                return;
            } 
        }

        if (Target == null) return;

        if (NPCState == NpcState.Attacking || NPCState == NpcState.WalkingBackToSpawnPoint) return;

        CurrentTargetTimeoutTime -= deltaTime;
        if (CurrentTargetTimeoutTime < 0f) {
            if (!GetTarget()) {
                FollowMaster(SummonerTransform);
                return;
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
            StopFollowingMaster();
            MoveTowardsTarget(Target);
            currentPathRefreshTime = 0f;
        } else if (currentPathRefreshTime >= 0.4f && DistanceFromTarget > LookRadius) {
            currentPathRefreshTime = 0f;
            FollowMaster(SummonerTransform);
        }
    }

    public override void WalkBackToBehaviourRetreatPointNonReset() {
        FollowMaster(SummonerTransform);
    }

    public override void OnTargetDeath(Character character) {
        base.OnTargetDeath(character);
        DisableObstacle();
    }
}
