using System;
using UnityEngine;
using UnityEngine.AI;

public class ChargeHandler : MonoBehaviour {
    [SerializeField] private new SphereCollider collider;
    [SerializeField] private LayerMask groundLayer;

    private NavMeshAgent _agent;
    private Transform _transform;

    private Vector3 _destination;
    private float _chargeDuration;
    private float _chargeSpeed;
    private int _pierceCount;
    private bool _pierceAll;

    private float _raycastCheckRate;
    private float _stayAboveGroundRange;
    private float _stayAboveGroundCallRate;

    private Action _onCompleteAction;

    private float currentRaycastCheckTime;
    private float currentDurationPassed;
    private int currentPierceCount;
    private float currentStayAboveGroundDurationPassed;
    private ObstacleCarveDisabler attachedCarveDisabler;

    private bool paused;
    private bool chargeEnded;

    private void Awake() {
        enabled = false;
    }

    public ChargeHandler LoadChargeHandler(Vector3 destination, NavMeshAgent agent, string collisionLayerName, Action completeAction,
        float chargeDuration, float chargeSpeed, int pierceCount, bool pierceAll, float collisionRadius = 0.5f,
        float stayAboveGroundRange = 0f, float stayAboveGroundCallRate = 0.1f, float raycastCheckRate = 0.1f) {

        _destination = destination;

        _agent = agent;
        _transform = agent.transform;

        transform.SetParent(_transform, false);

        attachedCarveDisabler = Utils.DisableObstaclesInArea(_transform.position, agent, _transform);

        _onCompleteAction = completeAction;
        _chargeDuration = chargeDuration;
        _chargeSpeed = chargeSpeed;
        _pierceCount = pierceCount;
        _pierceAll = pierceAll;

        _raycastCheckRate = raycastCheckRate;
        _stayAboveGroundRange = stayAboveGroundRange;
        _stayAboveGroundCallRate = stayAboveGroundCallRate;

        gameObject.layer = LayerMask.NameToLayer(collisionLayerName);
        collider.radius = collisionRadius;
        collider.isTrigger = true;

        chargeEnded = false;
        enabled = true;
        return this;
    }

    public void StopCharge() {
        if (chargeEnded) return;
        chargeEnded = true;

        _onCompleteAction?.Invoke();
        enabled = false;

        currentDurationPassed = 0f;
        currentPierceCount = 0;
        currentStayAboveGroundDurationPassed = 0f;

        attachedCarveDisabler.transform.SetParent(null);

        _agent.enabled = true;
        Destroy(gameObject);
    }

    public void PauseCharge(bool pause) {
        paused = pause;
    }

    void Update() {
        if (paused) return;

        float deltaTime = Time.deltaTime;

        if (_agent.enabled) _agent.enabled = false;

        Vector3 moveDist = _chargeSpeed * deltaTime * _transform.forward;

        currentRaycastCheckTime += deltaTime;

        if (currentRaycastCheckTime >= _raycastCheckRate) {
            currentRaycastCheckTime = 0f;
            if (!Physics.Raycast(_transform.position + (moveDist * 3f) + Vector3.up, Vector3.down, out RaycastHit hit, 4f, groundLayer)
                 || !NavMesh.SamplePosition(hit.point, out NavMeshHit hitNav, 0.5f, NavMesh.AllAreas)) {

                StopCharge();
            }
        }

        _transform.Translate(moveDist, Space.World);

        currentStayAboveGroundDurationPassed += deltaTime;
        if (currentStayAboveGroundDurationPassed >= _stayAboveGroundCallRate) {
            currentStayAboveGroundDurationPassed = 0f;
            Utils.StayAboveGround(_transform, _stayAboveGroundRange);
        }

        currentDurationPassed += deltaTime;
        if (currentDurationPassed >= _chargeDuration) {
            StopCharge();
        }

        if (Vector3.Distance(_transform.position, _destination) < 0.15f + _stayAboveGroundRange) StopCharge();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Terrain")) {
            StopCharge();
        } else {
            if (_pierceAll) return;

            currentPierceCount++;
            if (currentPierceCount > _pierceCount) {
                StopCharge();
            }
        }
    }
}
