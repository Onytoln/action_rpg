using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ObstacleCarveDisabler : MonoBehaviour {

    [SerializeField] private SphereCollider sphereCollider;
    [SerializeField] private LayerMask hitLayerMask;

    private GameObject deactivator;
    private NavMeshAgent deactivatorAgent;
    private ObstacleAvoidanceType avoidanceType;

    void Awake()
    {
        sphereCollider.enabled = false;
    }

    public void PreloadObstacleCarver(NavMeshAgent deactivatorAgent) {
        this.deactivator = deactivatorAgent.gameObject;

        this.deactivatorAgent = deactivatorAgent;
        avoidanceType = this.deactivatorAgent.obstacleAvoidanceType;
        this.deactivatorAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        sphereCollider.enabled = true;
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.TryGetComponent(out NavMeshObstacle obstacle)) {
            obstacle.carving = false;
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject == deactivator) {
            sphereCollider.enabled = false;
            Collider[] results = Physics.OverlapSphere(transform.position, sphereCollider.radius, hitLayerMask);
            for (int i = 0; i < results.Length; i++) {
                if (results[i].gameObject.TryGetComponent(out NavMeshObstacle obstacle)) {
                    obstacle.carving = true;
                }
            }

            this.deactivatorAgent.obstacleAvoidanceType = avoidanceType;
            if(this.deactivatorAgent.TryGetComponent(out NavMeshObstacle deactivatorObstacle)) {
                deactivatorObstacle.carving = true;
            }

            Destroy(this.gameObject);
        } else {
            if (other.gameObject.TryGetComponent(out NavMeshObstacle obstacle)) {
                obstacle.carving = true;
            }
        }
    }

}
