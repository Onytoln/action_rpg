using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Charge : MonoBehaviour {
    Camera cam;
    public LayerMask groundMask;
    public LayerMask terrainMask;
    NavMeshAgent agent;
    IEnumerator chargeCouroutine;

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        cam = Camera.main;
    }

    void Update() {
        if (Input.GetKey(KeyCode.Space)) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            NavMeshHit navHit;

            if (Physics.Raycast(ray, out hit, 100, groundMask) && NavMesh.SamplePosition(hit.point, out navHit, 0.1f, NavMesh.AllAreas)) {
                if (chargeCouroutine == null) {
                    chargeCouroutine = ChargeToLocation(hit.point);
                    StartCoroutine(chargeCouroutine);
                }
            }
        }
    }

    public IEnumerator ChargeToLocation(Vector3 position) {
        agent.enabled = false;
        Debug.Log(position);
        RaycastHit hit;
        //transform.LookAt(position);
        Vector3 direction = (position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        Vector3 startingPosition = transform.position;
        while (Vector3.Distance(transform.position, position) > 0.3) {
            transform.Translate(direction * Time.deltaTime * 6f, Space.World);
            Debug.DrawRay(transform.position + Vector3.up, direction * 10f, Color.yellow);
            if (Physics.SphereCast(transform.position + Vector3.up, 0.3f, direction, out hit, 0.6f, terrainMask)) {
                StopAllCoroutines();
                break;
            }
            if (Vector3.Distance(startingPosition, transform.position) >= 10) {
                StopAllCoroutines();
                break;
            }
            yield return null;
        }
        chargeCouroutine = null;
        transform.hasChanged = true;
        agent.enabled = true;
    }
}

/*if (Physics.Raycast(ray, out hit, 100, groundMask) && Vector3.Distance(transform.position, hit.point) <= 10) {
    if (NavMesh.SamplePosition(hit.point, out navHit, 1f, NavMesh.AllAreas)) {
        transform.LookAt(hit.point);
        while (Vector3.Distance(transform.position, hit.point) > 0.5) {
            transform.position = Vector3.MoveTowards(transform.position, hit.point, 1f * Time.deltaTime);
            yield return null;
        }
    } else {
        StopAllCoroutines();
        agent.enabled = true;
    }
} else {
    StopAllCoroutines();
    agent.enabled = true;
}

 if (Vector3.Distance(transform.position, hit.point) < 0.5) {
            StopAllCoroutines();
            agent.enabled = true;
        }

 */