using MEC;
using System.Collections.Generic;
using UnityEngine;

public class NPCSummonBehavior : NPCBehavior {
    public Transform Master { get; private set; }
    public bool FollowingMaster { get; private set; } = false;
    public CoroutineHandle FollowMasterCoroutine { get; private set; }
    [Header("Master")]
    [SerializeField] private float followMasterPathRefreshTime = 0.5f;
    [SerializeField] private float masterDistanceOffset = 3;


    public override void Start() {
        base.Start();
        FollowingMaster = false;
    }

    protected override void SetNpcState(NpcState state) {
        if (state == npcState) return;

        switch (state) {
            case NpcState.FollowingMaster:
                npcState = NpcState.FollowingMaster;
                DisableRotation();
                DisableObstacle();
                break;
            default:
                base.SetNpcState(state);
                break;
        }
    }

    protected override void OnTargetAcquiredAIResponse(Character characterComponent) {
        base.OnTargetAcquiredAIResponse(characterComponent);

        if (Vector3.Distance(transform.position, Target.transform.position) <= LookRadius) {
            StopFollowingMaster();
            killWalkBackToSpawnNonReset = true;
        }
    }

    #region Follow Master

    public void FollowMaster(Transform master) {
        if (FollowingMaster) return;
        if (master == null) return;
        Master = master;
        Timing.KillCoroutines(FollowMasterCoroutine);
        FollowMasterCoroutine = Timing.RunCoroutine(FollowMasterTarget());
    }

    public void StopFollowingMaster() {
        if (FollowingMaster) {
            Timing.KillCoroutines(FollowMasterCoroutine);
            FollowingMaster = false;
            NPCState = NpcState.Idle;
        }
    }

    private IEnumerator<float> FollowMasterTarget() {
        FollowingMaster = true;

        while (true) {
            NPCState = NpcState.FollowingMaster;
            if (Vector3.Distance(Master.position, transform.position) > masterDistanceOffset) {

                Vector3 nearMasterPos = UnityEngine.Random.insideUnitCircle * masterDistanceOffset;
                nearMasterPos.z = nearMasterPos.y;
                nearMasterPos.y = Master.position.y;
                nearMasterPos += Master.position;
                yield return Timing.WaitForOneFrame;
                MoveTowardsPoint(nearMasterPos);
            }
            yield return Timing.WaitForSeconds(followMasterPathRefreshTime);
        }
    }

    #endregion
}
