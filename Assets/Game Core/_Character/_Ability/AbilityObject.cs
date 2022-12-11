using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(Rigidbody))]
public class AbilityObject : MonoBehaviour {
    public event Action<bool> OnSkillMissed;

    //Properties of the skill
    public CoreAbilityData CoreAbilityData { get; private set; }

    //easy accessors
    public Character CharacterComponent => CoreAbilityData.CharacterComponent;
    public IHitLayers HitLayers => CoreAbilityData.CharacterHitLayers;
    public AbilityProperties AbilityProperties => CoreAbilityData.AbilityProperties;
    public AbilityPropertiesValuesContainer APropValues => CoreAbilityData.AbilityPropertiesValuesContainer;
    public CoreStatsValuesContainer CoreStatsValues => CoreAbilityData.CoreStatsValues;
    public SkillTemplate SkillLogic => CoreAbilityData.SkillLogic;
    public Vector3 CastPoint => CoreAbilityData.CastPoint;
    public GameObject Target => CoreAbilityData.Target;
    //----------------------------------------------------------------------------------------------------------------------//
    private ObjectPoolManager objectPoolManager;
    public ObjectPoolManager ObjectPoolManager { get { if (objectPoolManager == null) objectPoolManager = ObjectPoolManager.Instance; return objectPoolManager; } }

    private AudioManager audioManager;
    public AudioManager AudioManager { get { if (audioManager == null) audioManager = AudioManager.Instance; return audioManager; } }

    private VfxManager vfxManager;
    public VfxManager VfxManager { get { if (vfxManager == null) vfxManager = VfxManager.Instance; return vfxManager; } }

    public Vector3 SpawnPosition { get; protected set; }
    public bool CanTravel { get; protected set; }

    public SphereCollider objectCollider;

    public GameObject onEnableSetLayer;

    [SerializeField] protected ParticleSystem mainObjectParticles;
    [SerializeField] protected AudioSource objectFunctionEndAudio;

    public virtual void OnValidate() {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        if (!gameObject.CompareTag(DataStorage.AbilityObjectTag)) gameObject.tag = DataStorage.AbilityObjectTag;
    }

    public void Initialize(CoreAbilityData coreAbilityData) {
        CoreAbilityData = coreAbilityData;

        if (coreAbilityData.AbilityProperties == null || coreAbilityData.CharacterComponent == null) {
            Debug.LogError("Ability Properties or Character Component is null for AbilityObject Initialize method.");
        }
    }

    public virtual void Initialize(params object[] parameters) { }

    public virtual void Awake() {
        if (!gameObject.CompareTag(DataStorage.AbilityObjectTag)) gameObject.tag = DataStorage.AbilityObjectTag;
    }

    public virtual void Start() { }

    public virtual void OnEnable() {
        SpawnPosition = transform.position;
        EnableObjectFunctionality();
        if (CoreAbilityData == null) return;
        SetLayerOnSelectedObj();

        if(mainObjectParticles != null) {
            mainObjectParticles.Play(true);
        }
    }

    public virtual void Update() {

    }

    public virtual void OnDisable() {
        SpawnPosition = Vector3.zero;

#if !UNITY_EDITOR
        if (CoreAbilityData != null) {
            CoreAbilityData.AbilityProperties.DestroyIfCopy();
        }
#endif

        CoreAbilityData = null;
    }

    public virtual void EndObjectsFunction() {
        DisableObjectFunctionality();
       
        if (mainObjectParticles != null) {
            mainObjectParticles.Stop(true);
        }

        if (objectFunctionEndAudio != null) {
            AudioManager.PlayOneShotAudioSource(objectFunctionEndAudio, transform.position);
        }

        ObjectPoolManager.PoolObjectBack(gameObject.name, gameObject);
    }

    protected virtual void SetLayerOnSelectedObj() {
        if (onEnableSetLayer != null) {
            int layer = Utils.LayerMaskToLayer(CoreAbilityData.CharacterHitLayers.GetAbilityObjectHitLayer());
            if (layer != onEnableSetLayer.layer) {
                Debug.Log("Ability object has incorrect layer, setting layer now.");
                onEnableSetLayer.SetLayerRecursively(layer);
            }
        }
    }

    public void RemoveEventsSubscribers() {
        OnSkillMissed = null;
    }

    public void DisableObjectFunctionality() {
        CanTravel = false;
        objectCollider.enabled = false;
        objectCollider.isTrigger = false;
    }

    public void EnableObjectFunctionality() {
        CanTravel = true;
        objectCollider.enabled = true;
        objectCollider.isTrigger = true;
    }

    public void SkillMissed(bool missed) {
        OnSkillMissed?.Invoke(missed);
        RemoveEventsSubscribers();
    }

    public HitOutput HitEnemy(Collider other, float damageValue, string currentHitInfoId, List<DamageTypeWeight> preferedDamageTypes = null) {
        return HitInput.HitEnemy(other, damageValue, currentHitInfoId, CharacterComponent, AbilityProperties, APropValues, CoreStatsValues, this,
            SkillLogic, CastPoint, Target, preferedDamageTypes);
    }

    public HitOutput HitEnemy(Combat targetCombat, float damageValue, string currentHitInfoId, List<DamageTypeWeight> preferedDamageTypes = null) {
        return HitInput.HitEnemy(targetCombat, damageValue, currentHitInfoId, CharacterComponent, AbilityProperties, APropValues, CoreStatsValues, this,
            SkillLogic, CastPoint, Target, preferedDamageTypes);
    }

    public HitOutput HitEnemyExactDamage(Collider other, float exactDamage, string currentHitInfoId, List<DamageTypeWeight> preferedDamageTypes = null) {
        return HitInput.HitEnemyExactDamage(other, exactDamage, currentHitInfoId, CharacterComponent, AbilityProperties, APropValues, CoreStatsValues, this,
            SkillLogic, CastPoint, Target, preferedDamageTypes);
    }

    public HitOutput HitEnemyExactDamage(Combat targetCombat, float exactDamage, string currentHitInfoId, List<DamageTypeWeight> preferedDamageTypes = null) {
        return HitInput.HitEnemyExactDamage(targetCombat, exactDamage, currentHitInfoId, CharacterComponent, AbilityProperties, APropValues, CoreStatsValues, this,
            SkillLogic, CastPoint, Target, preferedDamageTypes);
    }
}
