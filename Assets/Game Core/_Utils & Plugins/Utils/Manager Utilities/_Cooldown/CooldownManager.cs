using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class CooldownManager : MonoBehaviour {
    #region Singleton
    public static CooldownManager Instance { get; private set; }
    void Awake() {
        if (Instance == null) {
            Instance = this;
        }
    }
    #endregion

    private readonly HashSet<ICooldown> objectsInCooldown = new HashSet<ICooldown>();
    private List<ICooldown> toRemove = new List<ICooldown>();

    void Update() {
        float deltaTime = Time.deltaTime;

        foreach (var cooldown in objectsInCooldown) {
            if (cooldown != null) {
                if (cooldown.HandleCooldown(deltaTime)) toRemove.Add(cooldown);
            } 
        }

        if (toRemove.Count > 0) {
            for (int i = 0; i < toRemove.Count; i++) {
                objectsInCooldown.Remove(toRemove[i]);
            }
            objectsInCooldown.Remove(null);

            toRemove.Clear();
        }
    }

    public void ProcessCooldown<T>(T cooldownObject) {
        ICooldown obj = cooldownObject as ICooldown;
        if (obj == null) return;
        if (objectsInCooldown.Contains(obj)) return;
        objectsInCooldown.Add(obj);
    }
}
