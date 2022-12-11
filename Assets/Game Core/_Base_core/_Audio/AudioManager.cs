using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    #region Singleton
    public static AudioManager Instance { get; private set; }

    private ObjectPoolManager objectPoolManager;
    private ObjectPoolManager ObjectPoolManager { get { if (objectPoolManager == null) objectPoolManager = ObjectPoolManager.Instance; return objectPoolManager; } }

    private readonly HashSet<AudioSource> checkedForAudioSourcePoolBacker = new HashSet<AudioSource>();

    private void Awake() {
        if (Instance == null) Instance = this;
    }

    #endregion


    void Start() {

    }

    public void UpdateMasterVolume() {

    }

    public AudioSource PlayOneShotAudioSource(AudioSource audioSource, Vector3 point) {
        if (audioSource == null) return null;

        AudioSource audioSrc = ObjectPoolManager.GetPooledAudioSource(audioSource.name, audioSource, point);

        if (!checkedForAudioSourcePoolBacker.Contains(audioSrc)) {
            if (!audioSrc.gameObject.TryGetComponent(out PoolAudioSourceBack _)) {
                audioSrc.gameObject.AddComponent<PoolAudioSourceBack>();
            }

            checkedForAudioSourcePoolBacker.Add(audioSrc);
        }

        audioSrc.Play();

        return audioSrc;
    }

    public void PlayOneShotClipAtPoint(AudioClip audioClip, Vector3 point) {

    }
}
