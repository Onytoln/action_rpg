using MEC;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour, IProgress {

    #region Singleton
    public static ObjectPoolManager Instance { get; private set; }

    public bool ReportsProgress => true;
    public float Progress { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }

        _ = Timing.RunCoroutine(TrackPoolingState());
        GameSceneManager.FinishingPhase.ExecuteCoroutineConcurrently(WaitForPooling, this, ExecuteAmount.Always, GameStage.AnyStage);
    }
    #endregion

    private const int DEFAULT_GAMEOBJECT_CAPACITY = 50;
    private const int DEFAULT_PARTICLE_CAPACITY = 100;
    private const int DEFAULT_AUDIO_CAPACITY = 100;

    [SerializeField] private int poolPerFrame = 2;

    private readonly Dictionary<string, CountConstrainedQueue<GameObject>> objectPool = new Dictionary<string, CountConstrainedQueue<GameObject>>();
    private readonly HashSet<GameObject> gameObjectPoolItems = new HashSet<GameObject>();

    private readonly Dictionary<string, CountConstrainedQueue<ParticleSystem>> particleSystemPool = new Dictionary<string, CountConstrainedQueue<ParticleSystem>>();
    private readonly HashSet<ParticleSystem> particlePoolItems = new HashSet<ParticleSystem>();

    private readonly Dictionary<string, CountConstrainedQueue<AudioSource>> audioSourcePool = new Dictionary<string, CountConstrainedQueue<AudioSource>>();
    private readonly HashSet<AudioSource> audioPoolItems = new HashSet<AudioSource>();

    private readonly List<CoroutineHandle> currentPoolingCoroutines = new List<CoroutineHandle>();
    private bool isPooling;

    private void RegisterPoolingHandle(CoroutineHandle handle) {
        currentPoolingCoroutines.Add(handle);
    }

    private IEnumerator<float> TrackPoolingState() {
        while (true) {
            if (currentPoolingCoroutines.Count == 0) {
                isPooling = false;
                yield return Timing.WaitForOneFrame;
            } else {
                isPooling = true;

                for (int i = currentPoolingCoroutines.Count; i-- > 0;) {
                    yield return Timing.WaitUntilDone(currentPoolingCoroutines[i], false);
                    currentPoolingCoroutines.RemoveAt(i);
                }
            }
        }
    }

    private IEnumerator<float> WaitForPooling() {
        while (isPooling) {
            yield return Timing.WaitForOneFrame;
        }
    }

    #region All Game Objects

    //ALL GAME OBJECTS
    public void PrePoolObjects(string name, GameObject obj, int poolCount, int capacity = DEFAULT_GAMEOBJECT_CAPACITY) {
        if (obj == null) return;

        if (!objectPool.TryGetValue(name, out var queue)) {
            queue = new CountConstrainedQueue<GameObject>(capacity <= 0 ? DEFAULT_GAMEOBJECT_CAPACITY : capacity);
            objectPool.Add(name, queue);
        }

        if (poolCount <= 0) return;

        if (poolCount > 1) {
            RegisterPoolingHandle(Timing.RunCoroutine(PoolObjectsOverTime(name, obj, poolCount, queue)));
        } else {
            _ = EnqueueNewObject(name, obj, queue);
        }
    }

    private bool EnqueueNewObject(string name, GameObject obj, CountConstrainedQueue<GameObject> queue) {
        if (queue.AtMaxCapacity) return false;

        GameObject prePooledObj = Instantiate(obj);
        prePooledObj.name = name;
        prePooledObj.SetActive(false);

        _ = queue.Enqueue(prePooledObj);

        gameObjectPoolItems.Add(prePooledObj);

        return true;
    }

    public void PoolObjectBack(string name, GameObject obj) {
        if (obj == null) { return; }

        PrePoolObjects(name, obj, 0);

        if (gameObjectPoolItems.Contains(obj)) return;

        if (!objectPool[name].Enqueue(obj)) {
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(null);

        gameObjectPoolItems.Add(obj);
    }

    /// <summary>
    /// Get pooled object back from pool. If no queue of name identifier exists, new queue is created with one object.
    /// If queue is empty, one objects gets instantiated.
    /// !!!OBJECTS ARE NOT ACTIVATED WHEN DE-POOLED!!!
    /// </summary>
    /// <param name="name">Name of object - use prefabs names to avoid creating additional queues for same type of object</param>
    /// <param name="obj">Object to instantiate in case no queue for object exists or is empty</param>
    /// <param name="spawnPos">Spawn position of object in Vector3</param>
    /// <param name="spawnRot">Spawn rotation of object in Quaternion</param>
    /// <returns>Not active pooled GameObject</returns>
    public GameObject GetPooledObject(string name, GameObject obj, Vector3 spawnPos = default, Quaternion spawnRot = default) {
        if (obj == null) return null;

        if (!objectPool.TryGetValue(name, out var queue) || queue.Count == 0) {
            PrePoolObjects(name, obj, 1);
        }

        GameObject spawnedObject = objectPool[name].Dequeue();
        spawnedObject.transform.SetPositionAndRotation(spawnPos, spawnRot);

        gameObjectPoolItems.Remove(spawnedObject);

        return spawnedObject;
    }

    private IEnumerator<float> PoolObjectsOverTime(string name, GameObject obj, int poolCount, CountConstrainedQueue<GameObject> queue) {
        for (int i = 0; i < poolCount; i++) {
            if (!EnqueueNewObject(name, obj, queue))
                yield break;

            if ((i + 1) % poolPerFrame == 0) {
                yield return Timing.WaitUntilDone(Utils.WaitFrames(1));
            }
        }
    }

    #endregion

    #region Particle Systems
    //PARTICLE SYSTEMS
    public void PrePoolParticleSystem(string name, ParticleSystem particleSystem, int poolCount, int capacity = DEFAULT_PARTICLE_CAPACITY) {
        if (particleSystem == null) { return; }

        if (!particleSystemPool.TryGetValue(name, out var queue)) {
            queue = new CountConstrainedQueue<ParticleSystem>(capacity <= 0 ? DEFAULT_PARTICLE_CAPACITY : capacity);
            particleSystemPool.Add(name, queue);
        }

        if (poolCount > 0) {
            if (poolCount > 1) {
                RegisterPoolingHandle(Timing.RunCoroutine(PoolParticleSystemsOverTime(name, particleSystem, poolCount, queue)));
            } else {
                _ = EnqueueNewParticleSystem(name, particleSystem, queue);
            }
        }
    }

    private bool EnqueueNewParticleSystem(string name, ParticleSystem particleSystem, CountConstrainedQueue<ParticleSystem> queue) {
        if (queue.AtMaxCapacity) return false;

        ParticleSystem prePooledParticleSystem = Instantiate(particleSystem);
        prePooledParticleSystem.name = name;
        prePooledParticleSystem.gameObject.SetActive(false);
        prePooledParticleSystem.Stop(true);

        _ = queue.Enqueue(prePooledParticleSystem);

        particlePoolItems.Add(prePooledParticleSystem);

        return true;
    }

    public void PoolParticleSystemBack(string name, ParticleSystem particleSystem, float fadeTime = 1f) {
        if (particleSystem == null) { return; }

        PrePoolParticleSystem(name, particleSystem, 0);

        Timing.RunCoroutine(PoolParticleBackOverTime(name, particleSystem, fadeTime));
    }

    /// <summary>
    /// Get pooled object back from pool. If no queue of name identifier exists, new queue is created with one object.
    /// If queue is empty, one objects gets instantiated.
    /// !!!PARTICLE SYSTEMS ARE ACTIVATED WHEN DE-POOLED!!!
    /// </summary>
    /// <param name="name">Name of object - use prefabs names to avoid creating additional queues for same type of object</param>
    /// <param name="particleSystem">Object to instantiate in case no queue for object exists or is empty</param>
    /// <param name="spawnPos">Spawn position of object in Vector3</param>
    /// <param name="spawnRot">Spawn rotation of object in Quaternion</param>
    /// <returns>Not active pooled GameObject</returns>
    public ParticleSystem GetPooledParticleSystem(string name, ParticleSystem particleSystem, Vector3 spawnPos = default, Quaternion spawnRot = default) {
        if (particleSystem == null) return null;

        if (!particleSystemPool.TryGetValue(name, out var queue) || queue.Count == 0) {
            PrePoolParticleSystem(name, particleSystem, 1);
        }

        ParticleSystem spawnedParticleSystem = particleSystemPool[name].Dequeue();
        spawnedParticleSystem.transform.SetPositionAndRotation(spawnPos, spawnRot);
        spawnedParticleSystem.gameObject.SetActive(true);

        particlePoolItems.Remove(spawnedParticleSystem);

        return spawnedParticleSystem;
    }

    private IEnumerator<float> PoolParticleSystemsOverTime(string name, ParticleSystem particleSystem, int poolCount, CountConstrainedQueue<ParticleSystem> queue) {
        for (int i = 0; i < poolCount; i++) {
            if (!EnqueueNewParticleSystem(name, particleSystem, queue))
                yield break;

            if ((i + 1) % poolPerFrame == 0) {
                yield return Timing.WaitUntilDone(Utils.WaitFrames(1));
            }
        }
    }

    private IEnumerator<float> PoolParticleBackOverTime(string name, ParticleSystem particleSystem, float fadeTime) {
        particleSystem.Stop(true);

        yield return Timing.WaitForSeconds(fadeTime);

        if (particlePoolItems.Contains(particleSystem)) yield break;

        if (!particleSystemPool[name].Enqueue(particleSystem)) {
            Destroy(particleSystem);
            yield break;
        }

        particleSystem.gameObject.SetActive(false);
        particleSystem.transform.SetParent(null);

        particlePoolItems.Add(particleSystem);
    }

    #endregion

    #region Audio Sources
    //AUDIO SOURCES
    public void PrePoolAudioSource(string name, AudioSource audioSource, int poolCount, int capacity = DEFAULT_AUDIO_CAPACITY) {
        if (audioSource == null) return;

        if (!audioSourcePool.TryGetValue(name, out var queue)) {
            queue = new CountConstrainedQueue<AudioSource>(capacity <= 0 ? DEFAULT_AUDIO_CAPACITY : capacity);
            audioSourcePool.Add(name, queue);
        }

        if (poolCount > 0) {
            if (poolCount > 1) {
                RegisterPoolingHandle(Timing.RunCoroutine(PoolAudioSourcesOverTime(name, audioSource, poolCount, queue)));
            } else {
                _ = EnqueueNewAudioSource(name, audioSource, queue);
            }
        }
    }

    private bool EnqueueNewAudioSource(string name, AudioSource audioSource, CountConstrainedQueue<AudioSource> queue) {
        if (queue.AtMaxCapacity) return false;

        AudioSource prePooledObj = Instantiate(audioSource);
        prePooledObj.name = name;
        prePooledObj.gameObject.SetActive(false);

        _ = queue.Enqueue(prePooledObj);

        audioPoolItems.Add(prePooledObj);

        return true;
    }

    public void PoolAudioSourceBack(string name, AudioSource audioSource, float fadeTime = 0.5f) {
        if (audioSource == null) { return; }

        PrePoolAudioSource(name, audioSource, 0);

        Timing.RunCoroutine(PoolAudioSourceBackOverTime(name, audioSource, fadeTime));
    }

    /// <summary>
    /// Get pooled object back from pool. If no queue of name identifier exists, new queue is created with one object.
    /// If queue is empty, one objects gets instantiated.
    /// !!!OBJECTS ARE ACTIVATED WHEN DE-POOLED!!!
    /// </summary>
    /// <param name="name">Name of object - use prefabs names to avoid creating additional queues for same type of object</param>
    /// <param name="audioSource">Object to instantiate in case no queue for object exists or is empty</param>
    /// <param name="spawnPos">Spawn position of object in Vector3</param>
    /// <param name="spawnRot">Spawn rotation of object in Quaternion</param>
    /// <returns>Not active pooled GameObject</returns>
    public AudioSource GetPooledAudioSource(string name, AudioSource audioSource, Vector3 spawnPos = default, Quaternion spawnRot = default) {
        if (audioSource == null) { return null; }

        if (!audioSourcePool.TryGetValue(name, out var queue) || queue.Count == 0) {
            PrePoolAudioSource(name, audioSource, 1);
        }

        AudioSource spawnedAudioSource = audioSourcePool[name].Dequeue();
        spawnedAudioSource.transform.SetPositionAndRotation(spawnPos, spawnRot);
        spawnedAudioSource.gameObject.SetActive(true);

        audioPoolItems.Remove(spawnedAudioSource);

        return spawnedAudioSource;
    }

    private IEnumerator<float> PoolAudioSourcesOverTime(string name, AudioSource audioSource, int poolCount, CountConstrainedQueue<AudioSource> queue) {
        for (int i = 0; i < poolCount; i++) {
            if (!EnqueueNewAudioSource(name, audioSource, queue))
                yield break;

            if ((i + 1) % poolPerFrame == 0 && i != 0) {
                yield return Timing.WaitUntilDone(Utils.WaitFrames(1));
            }
        }
    }

    private IEnumerator<float> PoolAudioSourceBackOverTime(string name, AudioSource audioSource, float fadeTime) {
        if (fadeTime <= 0f) {
            Finish();
        } else {
            float fadeSpeed = 1f / fadeTime;
            float time = 0f;

            while (time < 1) {
                audioSource.volume = Mathf.Lerp(1, 0, time);
                time += Time.deltaTime * fadeSpeed;
                yield return Timing.WaitForOneFrame;
            }

            Finish();
        }

        void Finish() {
            if (audioPoolItems.Contains(audioSource)) return;

            if (!audioSourcePool[name].Enqueue(audioSource)) {
                Destroy(audioSource);
                return;
            }

            audioSource.gameObject.SetActive(false);
            audioSource.transform.SetParent(null);
            audioSource.volume = 1f;

            audioPoolItems.Add(audioSource);
        }
    }

    #endregion
}
