using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MonoDisabler : MonoBehaviour {

    private static List<MonoDisabler> gameLoadCompDisablers = new List<MonoDisabler>();

    private IDisableUntilGameLoaded[] disableUntilGameLoadedMonos;

    private bool thisStageDisabled = false;

    private void Awake() {
        gameLoadCompDisablers.Add(this);
        GameSceneManager.OnLoadStarted += OnLoadStarted;
    }

    private void OnDestroy() {
        GameSceneManager.OnLoadStarted -= OnLoadStarted;
        gameLoadCompDisablers.Remove(this);
    }

    private void Start() {
        DisableMonos();
    }

    private void OnLoadStarted(bool state) {
        if (state) {
            DisableMonos();
        } else {
            EnableMonos();
        }
    }

    public void DisableMonos() {
        if (thisStageDisabled) return;
        thisStageDisabled = true;

        disableUntilGameLoadedMonos = Utils.GetTypeFromSceneMonos<IDisableUntilGameLoaded>(gameObject.scene);

        for (int i = 0; i < disableUntilGameLoadedMonos.Length; i++) {
            MonoBehaviour mono = disableUntilGameLoadedMonos[i] as MonoBehaviour;
            if (mono != null) {
                mono.enabled = false;
            }
        }

        if(!gameObject.TryGetComponent(out CanvasGroup _)) {
            var canvGroup = gameObject.AddComponent<CanvasGroup>();
            canvGroup.blocksRaycasts = true;
            canvGroup.alpha = 0f;
        }
    }

    public void EnableMonos() {
        for (int i = 0; i < disableUntilGameLoadedMonos.Length; i++) {
            MonoBehaviour mono = disableUntilGameLoadedMonos[i] as MonoBehaviour;
            if (mono != null) {
                mono.enabled = true;
            }
        }

        if (gameObject.TryGetComponent(out CanvasGroup canvasGroup)) {
            Destroy(canvasGroup);
        }

        thisStageDisabled = false;
    }

    public static MonoDisabler GetMonoDisablerByUnityScene(Scene scene) {
        for (int i = 0; i < gameLoadCompDisablers.Count; i++) {
            if (gameLoadCompDisablers[i].gameObject.scene == scene) return gameLoadCompDisablers[i];
        }

        return null;
    }

    public static MonoDisabler GetMonoDisablerByMyScene(Scenes scene) {
        for (int i = 0; i < gameLoadCompDisablers.Count; i++) {
            if (gameLoadCompDisablers[i].gameObject.scene == GameSceneManager.UnitySceneByMyScene(scene)) return gameLoadCompDisablers[i];
        }

        return null;
    }
}
