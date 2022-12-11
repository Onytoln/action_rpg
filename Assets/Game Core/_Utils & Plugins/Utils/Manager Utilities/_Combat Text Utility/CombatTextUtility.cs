using System.Collections.Generic;
using UnityEngine;

public class CombatTextUtility : MonoBehaviour, IDisableUntilGameLoaded {

    [SerializeField] private CombatText combatTextPrefab;
    [SerializeField] private Vector3 spawnOffset;

    internal static int combatTextSortOrder = 0;

    private readonly List<CombatText> activeCombatTexts = new List<CombatText>() { Capacity = 500 };

    private ObjectPoolManager objectPoolManager;

    #region Singleton
    public static CombatTextUtility Instance { get; private set; }
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }

        GameSceneManager.LatePostSceneLoadPhase.ExecuteSync(() => {
            objectPoolManager = ObjectPoolManager.Instance;
            objectPoolManager.PrePoolObjects(combatTextPrefab.name, combatTextPrefab.gameObject, 100, 200);
        }, null, ExecuteAmount.Always);
    }
    #endregion

    void Update() {
        Quaternion camRotation = Utils.MainCam.transform.rotation;

        for (int i = 0; i < activeCombatTexts.Count; i++) {
            activeCombatTexts[i].transform.rotation = camRotation;
            activeCombatTexts[i].UpdateCombatText();
        }
    }

    public void SpawnCombatText(Vector3 spawnPosition, string text, Color32 textColor, Color32? outlineColor = null, Transform followedTransform = null, int fontSize = 0,
        Vector3? combatTextScale = null, float disappearTime = 0, float speed = 0, float speedDecrement = 0, float scaleRate = 0, Vector3? direction = null, Color32? textColor2 = null) {

        CombatText combatText = objectPoolManager.GetPooledObject(combatTextPrefab.name, combatTextPrefab.gameObject, spawnPosition + spawnOffset, Utils.MainCam.transform.rotation)
           .GetComponent<CombatText>();

        combatTextSortOrder++;
        combatText.SetCombatText(text, textColor, outlineColor, followedTransform, fontSize, combatTextScale, disappearTime, speed, speedDecrement, scaleRate, direction, textColor2);
        activeCombatTexts.Add(combatText);
        combatText.OnTextEnd += OnTextEnd;

        combatText.gameObject.SetActive(true);

        if (combatTextSortOrder > 1000) {
            combatTextSortOrder = 0;
        }
    }

    private void OnTextEnd(CombatText combatText) {
        combatText.OnTextEnd -= OnTextEnd;
        objectPoolManager.PoolObjectBack(combatText.name, combatText.gameObject);
        activeCombatTexts.Remove(combatText);
    }

}
