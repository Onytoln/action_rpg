using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

public class LoadingScreenManager : MonoBehaviour
{

    [SerializeField] private GameObject loadingScreenParent;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private Image progressBarFill;
    [SerializeField] private Text progressBarText;

    CoroutineHandle fadeHandle;

    CoroutineHandle slerpProgressHandle;
    float to;

    private void Awake() {
        if (GameSceneManager.IsLoading) {
            LoadingStarted(true);
        }

        GameSceneManager.OnLoadStarted += LoadingStarted;
        GameSceneManager.OnCurrentProgressChanged += UpdateProgress;
    }

    private void LoadingStarted(bool state) {
        if (state) {
            Timing.KillCoroutines(fadeHandle);
            canvasGroup.alpha = 1f;
            loadingScreenParent.SetActive(true);
        } else {
            fadeHandle = Utils.FadeCanvasGroup(canvasGroup, false, fadeHandle);
        }
    }

    private void UpdateProgress(float currentProgress) {
        to = currentProgress;
        EndSlerpProgressCoroutine();
        slerpProgressHandle = Timing.RunCoroutine(SlerpProgressCoroutine());
    }

    private void EndSlerpProgressCoroutine() {
        Timing.KillCoroutines(slerpProgressHandle);
        SetVisuals(to);
    }

    private void SetVisuals(float progress) {
        progressBarFill.fillAmount = progress;
        progressBarText.text = $"Loading: {progressBarFill.fillAmount * 100f:0.00}%";
    }

    private IEnumerator<float> SlerpProgressCoroutine() {
        float time = 0f;
        float from = progressBarFill.fillAmount;

        while(time < 1f) {
            SetVisuals(Mathf.SmoothStep(from, to, time));

            yield return Timing.WaitForOneFrame;

            time += Time.deltaTime * 4f;
        }
    }
}
