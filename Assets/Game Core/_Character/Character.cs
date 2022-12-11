using CustomOutline;
using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour, IHitLayers, ISelected {

    [field: SerializeField] public string CharacterName { get; private set; } = "name";

    public CharacterStats CharacterStats { get; private set; }
    public Combat CharacterCombat { get; private set; }
    public StatusEffectsManager CharacterStatusEffectsManager { get; private set; }


    public Collider CharacterObjectCollider { get; private set; }
    public NavMeshAgent CharacterNavMeshAgent { get; private set; }
    public NavMeshObstacle CharacterNavMeshObstacle { get; private set; }

    public Animator CharacterAnimator { get; private set; }

    protected Outline CharacterOutline { get; private set; }

    public Renderer CharacterRenderer { get; private set; }
    public Renderer[] AllCharacterRenderers { get; private set; }

    [SerializeField] protected Material _dissolveMaterial;

    [SerializeField] private Transform vfxSpawnPosBottom;
    public Transform VfxSpawnPosBottom { get => vfxSpawnPosBottom; }

    [SerializeField] private Transform vfxSpawnPosMiddle;
    public Transform VfxSpawnPosMiddle { get => vfxSpawnPosMiddle; }

    [SerializeField] private float vfxScaleModifier = 1;
    public float VfxScaleModifier { get => vfxScaleModifier; }


    public event Action<Character> OnCharacterDeath;
    public event Action<Character> OnCompleteDeath;
    public event Action<Character, bool> OnCharacterUntargetable;
    private IEnumerator<float> respawnCoroutine;
    [SerializeField] AnimationClip deathReversedClip;
    private float _deathAnimDuration;
    [SerializeField] protected int animatorRespawnLayerIndex = 0;
    [SerializeField] protected bool respawnable = false;
    [SerializeField] protected float respawnableTime = 30f;
    protected CoroutineHandle? postDeathDissolveCoroutine = null;

    [SerializeField]
    private ObjectVisibleStateChange objectVisibleStateChange;

    #region HitLayers
    protected LayerMask hitLayerBitMask;
    public LayerMask SkillHitLayer {
        get => hitLayerBitMask;
    }
    protected string hitLayerName;

    protected LayerMask abilityObjectHitLayerBitMask;
    public LayerMask SkillAbilityObjectHitLayer {
        get => abilityObjectHitLayerBitMask;
    }
    protected string abilityObjectHitLayerName;
    #endregion

    [SerializeField]
    private CharacterRank characterRank;
    public CharacterRank CharacterRank { get => characterRank; }

    protected int characterLevel;
    public int CharacterLevel {
        get => characterLevel;
    }

    [SerializeField] protected StatType[] otherStatsToScaleWithLevel;

    [field: SerializeField, TextArea]
    public string[] AdditionalInfo { get; protected set; }

    private EventManager eventManager;
    public EventManager EventManager {
        get {
            if (eventManager == null) {
                eventManager = EventManager.Instance;
            }
            return eventManager;
        }
    }

    public bool IsActiveTarget { get; protected set; }

    public virtual void Awake() {
        CharacterStats = GetComponent<CharacterStats>();
        CharacterCombat = GetComponent<Combat>();
        CharacterStatusEffectsManager = GetComponent<StatusEffectsManager>();

        CharacterObjectCollider = GetComponent<CapsuleCollider>();
        CharacterNavMeshAgent = GetComponent<NavMeshAgent>();
        CharacterNavMeshObstacle = GetComponent<NavMeshObstacle>();

        CharacterAnimator = GetComponent<Animator>();

        CharacterOutline = GetComponent<Outline>();
        if (CharacterOutline != null) {
            CharacterOutline.enabled = false;
        }

        CharacterRenderer = GetComponentInChildren<Renderer>();
        AllCharacterRenderers = GetComponentsInChildren<Renderer>();

        if (objectVisibleStateChange != null) {
            objectVisibleStateChange.onBecameVisible += ObjectBecameVisible;
            objectVisibleStateChange.onBecameInvisible += ObjectBecameInvisible;
        } else {
            Debug.LogError($"{name} has no ObjectVisibleStateChange assinged, this must be assigned!");
        }

        CharacterStats.OnDeathInternal += ProcessDeath;
        CharacterStatusEffectsManager.OnIsUntargetableChanged += CharacterUntargetableHandler;
        if (CharacterStatusEffectsManager.IsUntargetable) CharacterUntargetableHandler(true);

        if (deathReversedClip != null)
            _deathAnimDuration = deathReversedClip.length;

        if (_deathAnimDuration == 0f) {
            _deathAnimDuration = Utils.GetClipLengthFromAnimator(CharacterAnimator, "DeathReversed");
        }
    }

    public virtual void OnValidate() {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public virtual void Start() {
        LoadHandler.NotifyOnLoad(ExperienceManager.Instance, LoadLvlData);
    }

    public LayerMask GetDirectHitLayer() {
        return hitLayerBitMask;
    }

    public string GetDirectHitLayerName() {
        return hitLayerName;
    }

    public LayerMask GetAbilityObjectHitLayer() {
        return abilityObjectHitLayerBitMask;
    }

    public string GetAbilityObjectHitLayerName() {
        return abilityObjectHitLayerName;
    }

    protected virtual void LoadLvlData(ILoadable loadable) {
        characterLevel = (loadable as ExperienceManager).CurrentPlayerLevel;
        if (CharacterStats.CharacterStatsFullyLoaded) {
            ApplyLevelStatModifier(characterLevel);
        } else {
            CharacterStats.OnCharacterStatsFullyLoaded += () => { ApplyLevelStatModifier(characterLevel); };
        }
    }

    protected virtual void ApplyLevelStatModifier(int level) {
        DataStorage dataStorage = DataStorage.Instance;

        CharacterStats.SetPrimaryValue(StatType.Damage, dataStorage.GetLevelStatMultiplier(StatType.Damage, level));
        CharacterStats.SetPrimaryValue(StatType.Health, dataStorage.GetLevelStatMultiplier(StatType.Health, level));
        CharacterStats.SetCurrentHealthToMax();

        if (otherStatsToScaleWithLevel == null || otherStatsToScaleWithLevel.Length == 0) return;

        for (int i = 0; i < otherStatsToScaleWithLevel.Length; i++) {
            CharacterStats.SetPrimaryValue(otherStatsToScaleWithLevel[i], dataStorage.GetLevelStatMultiplier(otherStatsToScaleWithLevel[i], level));
        }
    }

    public virtual SkillTemplate[] GetCharacterSkills() {
        return null;
    }

    public virtual SkillTemplate GetSkillById(string id) {
        return null;
    }

    public virtual SkillProperties GetSkillPropertiesById(string id) {
        return null;
    }

    public virtual void SetHitLayer(string layerName) { }

    public virtual void SetObjectHitLayer(string layerName) { }

    public virtual void ActivateTarget() { }

    public virtual void DeactivateTarget() { }

    public virtual void DisableCharacter() { }

    public virtual void EnableCharacter() { }

    public virtual void OnSelected() { }

    public virtual void OnDeselected() { }

    public virtual void CharacterUntargetableHandler(bool state) {
        OnCharacterUntargetable?.Invoke(this, state);
        EventManager.OnNpcUntargetable?.Invoke(this, state);
    }

    public virtual void ProcessDeath() {
        respawnCoroutine = Respawn();

        DeactivateTarget();

        CharacterStatusEffectsManager.SetIsDead(true);
        CharacterStats.StopHealthRegenerationCoroutine();

        OnCharacterDeath?.Invoke(this);
        CharacterStatusEffectsManager.SetIsUntargetable(true);

        postDeathDissolveCoroutine = Timing.RunCoroutine(PostDeathCoroutine());
    }

    public virtual void ProcessRespawn() {
        if (!respawnable) return;
        respawnable = false;

        if (respawnCoroutine != null) {
            Timing.RunCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }
    }

    private IEnumerator<float> Respawn() {
        if (!CharacterStatusEffectsManager.IsDead || CharacterStatusEffectsManager.IsRespawning) yield break;

        if (postDeathDissolveCoroutine != null)
            Timing.KillCoroutines((CoroutineHandle)postDeathDissolveCoroutine);

        ResetCoreStatusEffectParameters(true);
        CharacterStatusEffectsManager.IsRespawning = true;

        yield return Timing.WaitForSeconds(0.26f);

        AnimatorStateInfo stateInfo = CharacterAnimator.GetCurrentAnimatorStateInfo(animatorRespawnLayerIndex);
        if (stateInfo.IsName("Death") && stateInfo.normalizedTime < 1) {
            yield return Timing.WaitForSeconds(stateInfo.length - stateInfo.length * stateInfo.normalizedTime);
        }

        CharacterStatusEffectsManager.SetIsDead(false);

        yield return Timing.WaitForSeconds(_deathAnimDuration);

        ResetCoreStatusEffectParameters(false);
        ActivateTarget();

        CharacterStats.RunHealthRegenerationCoroutine();

        CharacterStatusEffectsManager.SetIsUntargetable(false);
        CharacterStats.SetCurrentHealth(CharacterStats.CoreStats.Stats[8].GetValue());
        CharacterStatusEffectsManager.IsRespawning = false;
        CharacterStats.OnDeathInternal += ProcessDeath;
        eventManager.OnNpcRespawn?.Invoke(this);
    }

    public void ResetCoreStatusEffectParameters(bool resetTriggers) {
        CharacterStatusEffectsManager.SetIsCasting(false);
        CharacterStatusEffectsManager.SetIsChanneling(false);
        //CharacterStatusEffectsManager.CurrentSkillFullCastFinished();

        //if (resetTriggers) StaticUtils.ResetAllAnimatorTriggers(CharacterAnimator);
    }

    protected void ObjectBecameVisible() { }

    protected void ObjectBecameInvisible() { }

    private IEnumerator<float> PostDeathCoroutine() {
        if (respawnable)
            yield return Timing.WaitForSeconds(respawnableTime);

        respawnable = false;

        if (_dissolveMaterial != null) {
            float time = 0f;
            float multiplier = 1f / _deathAnimDuration;

            for (int i = 0; i < AllCharacterRenderers.Length; i++) {
                Utils.ReplaceMainMaterial(AllCharacterRenderers[i], _dissolveMaterial);
                AllCharacterRenderers[i].material.SetFloat("_Dissolve", 0f);
            }

            yield return Timing.WaitForSeconds(_deathAnimDuration * 0.5f);

            while (time < 1f) {
                for (int i = 0; i < AllCharacterRenderers.Length; i++) {
                    AllCharacterRenderers[i].material.SetFloat("_Dissolve", Mathf.SmoothStep(0f, 1f, time));
                }
                time += Time.deltaTime * multiplier;
                yield return Timing.WaitForOneFrame;
            }
        } else {
            Debug.LogError($"{name} has no dissolve material attached, this should be attached!");
        }

        CharacterAnimator.enabled = false;
        gameObject.SetActive(false);
        OnCompleteDeath?.Invoke(this);
    }
}
