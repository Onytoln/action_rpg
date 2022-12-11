using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FirstGearGames.SmoothCameraShaker;
using MEC;

public class CameraController : MonoBehaviour, IDisableUntilGameLoaded
{
    public Transform target;
    private Vector3 targetLastPos;
    private Vector3 targetLerpFromPos;
    float lastToNowDist;
    bool followingSmooth = false;
    [SerializeField, Range(1f, 10f)] private float camSpeed = 1;

    public Vector3 offset;
    public float zoomSpeed = 4f;
    public float minZoom = 2f;
    public float maxZoom = 5f;

    public float pitch = 1f;

    public float yawSpeed = 100f;
    private float currentYaw = 0f;

    private float currentZoom = 10f;

    //private float smoothTime = 2f;

    public ShakeData shakeData;

    private void Awake() {
        GameSceneManager.LatePostSceneLoadPhase.ExecuteSync(Initialize, null, ExecuteAmount.Once);
    }
    
    private void Initialize() {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        targetLastPos = target.position;
    }

    void Update()
    {
        if (!UItrigger.Instance.BlockedByUI) currentZoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        currentYaw -= Input.GetAxis("Horizontal") * yawSpeed * Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        lastToNowDist = Vector3.Distance(target.position, targetLastPos);

        if (lastToNowDist > 0.5f && !followingSmooth) {
            followingSmooth = true;
            targetLerpFromPos = targetLastPos;
            Timing.RunCoroutine(FollowTargetSmooth(), Segment.LateUpdate);
        }

        targetLastPos = target.position;

        if (followingSmooth) return;
        transform.position = target.position - offset * currentZoom;
        transform.LookAt(target.position + Vector3.up * pitch);
        transform.RotateAround(target.position, Vector3.up, currentYaw);
    }

    private IEnumerator<float> FollowTargetSmooth() {
        float t = 0;
        
        Vector3 targetLerpedPos; 

        while (t < 1) {
            Mathf.Clamp01(t += Time.unscaledDeltaTime * camSpeed);
            targetLerpedPos = Vector3.Slerp(targetLerpFromPos, target.position, t);
            transform.position = targetLerpedPos - offset * currentZoom;
            transform.LookAt(targetLerpedPos + Vector3.up * pitch);
            transform.RotateAround(targetLerpedPos, Vector3.up, currentYaw);
            yield return Timing.WaitForOneFrame;           
        }

        followingSmooth = false;
    }
}

