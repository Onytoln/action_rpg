using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class PoolAudioSourceBack : MonoBehaviour
{
    [SerializeField] private AudioSource attachedAudioSource;

    void Awake()
    {
        if (attachedAudioSource == null) attachedAudioSource = GetComponent<AudioSource>();
    }

    private void OnEnable() {
        Timing.RunCoroutine(DisposePostSound());
    }

    IEnumerator<float> DisposePostSound() {
        yield return Timing.WaitForSeconds(attachedAudioSource.clip.length);
        ObjectPoolManager.Instance.PoolAudioSourceBack(name, attachedAudioSource, 0f);
    }

}
