using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class VfxManager : MonoBehaviour
{
    #region Singleton
    public static VfxManager Instance { get; private set; }

    private void Awake() {
        if (Instance == null) Instance = this;
    }

    #endregion

    private ObjectPoolManager objectPoolManager;
    private ObjectPoolManager ObjectPoolManager { get { if (objectPoolManager == null) objectPoolManager = ObjectPoolManager.Instance; return objectPoolManager; } }

    private readonly HashSet<ParticleSystem> checkedForParticleSystemPoolBacker = new HashSet<ParticleSystem>();

    public ParticleSystem PlayOneShotParticle(ParticleSystem particlesPrefab, Vector3 positon, Quaternion rotation = default, bool playWithAllChildren = true) {
        if (particlesPrefab == null) return null;

        ParticleSystem ps = ObjectPoolManager.GetPooledParticleSystem(particlesPrefab.name, particlesPrefab, positon, rotation);

        return InitializeOneShotParticle(ps, playWithAllChildren);
    }

    public ParticleSystem PlayOneShotParticle(ParticleSystem particlesPrefab, Transform parent, bool playWithAllChildren = true) {
        if (particlesPrefab == null || parent == null) return null;

        ParticleSystem ps = ObjectPoolManager.GetPooledParticleSystem(particlesPrefab.name, particlesPrefab);
        ps.transform.SetParent(parent, false);

        return InitializeOneShotParticle(ps, playWithAllChildren);
    }

    private ParticleSystem InitializeOneShotParticle(ParticleSystem ps, bool playWithAllChildren) {
        if (!checkedForParticleSystemPoolBacker.Contains(ps)) {
            ps.SetParticleSystemStopAction(ParticleSystemStopAction.Callback);

            if (!ps.gameObject.TryGetComponent(out PoolParticlesBack _)) {
                ps.gameObject.AddComponent<PoolParticlesBack>();
            }

            checkedForParticleSystemPoolBacker.Add(ps);
        }

        ps.Play(playWithAllChildren);

        return ps;
    }

}
