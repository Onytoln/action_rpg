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

    public CombatText SpawnCombatText(Vector3 spawnPosition, string text) {

        CombatText combatText = objectPoolManager.GetPooledObject(combatTextPrefab.name, combatTextPrefab.gameObject, spawnPosition + spawnOffset, Utils.MainCam.transform.rotation)
           .GetComponent<CombatText>();

        combatText.Initialize(text);

        combatTextSortOrder++;
        activeCombatTexts.Add(combatText);
        combatText.OnTextEnd += OnTextEnd;

        if (combatTextSortOrder > 1000) {
            combatTextSortOrder = 0;
        }

        return combatText;
    }

    private void OnTextEnd(CombatText combatText) {
        combatText.OnTextEnd -= OnTextEnd;
        objectPoolManager.PoolObjectBack(combatText.name, combatText.gameObject);
        activeCombatTexts.Remove(combatText);
    }

}
