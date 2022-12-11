using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolParticlesBack : MonoBehaviour {

    [SerializeField] private float fadeTime = 0f;

    [SerializeField] private ParticleSystem attachedParticleSystem;

    private void Awake() {
        if(attachedParticleSystem == null) attachedParticleSystem = GetComponent<ParticleSystem>();
    }


    private void OnParticleSystemStopped() {
       ObjectPoolManager.Instance.PoolParticleSystemBack(name, attachedParticleSystem, fadeTime);
    }

}
