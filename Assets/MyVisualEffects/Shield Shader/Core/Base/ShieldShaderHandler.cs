using MEC;
using System.Collections.Generic;
using UnityEngine;

public class ShieldShaderHandler : MonoBehaviour {
    private Camera cam;
    [SerializeField] Renderer _renderer;
    [SerializeField] AnimationCurve _DisplacementCurve;
    [SerializeField] float _DisplacementMagnitude = 0.4f;
    private float disolveSpeed;

    private CoroutineHandle dissolveCoroutine;

    private void Awake() {
        cam = Camera.main;
        if (_renderer == null) GetComponent<Renderer>();

        _renderer.material.SetFloat("_Disolve", 1f);
        enabled = false;
        _renderer.enabled = false;
    }

    void Update() {
        transform.forward = cam.transform.position - transform.position;
    }

    public void ShowShield(bool show, float dissolveDuration) {
        disolveSpeed = 1 / dissolveDuration;

        Timing.KillCoroutines(dissolveCoroutine);
        dissolveCoroutine = Timing.RunCoroutine(ShieldDissolveCoroutine(show));
    }

    private IEnumerator<float> ShieldDissolveCoroutine(bool show) {
        float initial = _renderer.material.GetFloat("_Disolve"); ;
        float goal = 1;

        float lerpTime = 0;

        if (show) {
            goal = 0;
        }

        enabled = true;
        _renderer.enabled = true;

        while (lerpTime < 1) {
            _renderer.material.SetFloat("_Disolve", Mathf.Lerp(initial, goal, lerpTime));
            lerpTime += Time.deltaTime * disolveSpeed;
            yield return Timing.WaitForOneFrame;
        }

        if (!show) {
            enabled = false;
            _renderer.enabled = false;
        }
    }
}
