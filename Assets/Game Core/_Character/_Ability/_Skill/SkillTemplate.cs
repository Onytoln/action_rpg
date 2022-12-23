using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillTemplate : MonoBehaviour, ICooldown, ILoadable {

    public event Action<ILoadable> OnLoad;
    public bool IsLoaded { get; private set; } = false;

    public event Action<SkillTemplate> OnSkillStartedCasting;
    public event Action<SkillTemplate> OnSkillFired;
    public event Action<SkillTemplate> OnFullCastDone;

    public Action<ICooldown> OnCooldownStart { get; set; }
    public Action<ICooldown> OnCooldownChanged { get; set; }
    public Action<ICooldown> OnCooldownEnd { get; set; }
    public float CurrentCooldown { get; private set; }
    public float CurrentStartingCooldown { get; private set; }
    public bool InCooldownUnusable { get; set; } = false;

    //From where fire 
    public Transform releasePoint;
    [field: SerializeField] public Character CharacterComponent { get; private set; }
    public IHitLayers CasterHitLayers { get; private set; }
    [Header("Skill properties MUST be assigned from the inspector")]
    public SkillProperties skillProperties;

    public SkillCastType SkillCastType => skillProperties.SkillCastType;

    [SerializeField, Header("Visuals and audio")] protected ParticleSystem particlesOnSkillFired;
    [SerializeField] protected Transform fireOnSkillFiredParticlesFrom;

    public Vector3 CastPoint { get; protected set; }
    public GameObject Target { get; protected set; }

    public virtual Vector3 CurrentDesiredCastPoint => throw new NotImplementedException("CurrentDesiredCastPoint must be overriden!");
    public virtual GameObject CurrentDesiredTarget => throw new NotImplementedException("CurrentDesiredTarget must be overriden!");

    #region References
    //main game camera
    private Camera mainCam;
    public Camera MainCam { get { if (mainCam == null) mainCam = Camera.main; return mainCam; } }

    //target manager
    private TargetManager targetManager;
    public TargetManager TargetManager { get { if (targetManager == null) targetManager = TargetManager.Instance; return targetManager; } }

    //object pooler
    private ObjectPoolManager objectPoolManager;
    public ObjectPoolManager ObjectPoolManager { get { if (objectPoolManager == null) objectPoolManager = ObjectPoolManager.Instance; return objectPoolManager; } }

    //event manager
    private EventManager eventManager;
    public EventManager EventManager { get { if (eventManager == null) eventManager = EventManager.Instance; return eventManager; } }

    //cooldown manager
    private CooldownManager coolDownManager;
    public CooldownManager CoolDownManager { get { if (coolDownManager == null) coolDownManager = CooldownManager.Instance; return coolDownManager; } }

    //ability object manager
    private AbilityObjectManager abilityObjectManager;
    public AbilityObjectManager AbilityObjectManager { get { if (abilityObjectManager == null) abilityObjectManager = AbilityObjectManager.Instance; return abilityObjectManager; } }

    private AudioManager audioManager;
    public AudioManager AudioManager { get { if (audioManager == null) audioManager = AudioManager.Instance; return audioManager; } }

    private VfxManager vfxManager;
    public VfxManager VfxManager { get { if (vfxManager == null) vfxManager = VfxManager.Instance; return vfxManager; } }

    //auto-property 
    //animator
    public Animator Animator { get; private set; }
    #endregion

    protected bool IsAwakePhase { get; private set; } = true;

    protected static readonly int fullCastDoneHash = Animator.StringToHash("FullCastDone");
    //trigger of current skill casting
    protected int currentSkillTriggerHash;

    public virtual void OnDestroy() { }

    public virtual void Awake() {
        AcquireComponents();
        InitializeSkillProperties();
        InitializeSkillPropertiesFunctions();

        GameTimeStarted();

        IsLoaded = true;
        OnLoad?.Invoke(this);
    }

    public virtual void OnValidate() { }

    public virtual void OnEnable() {
        IsAwakePhase = false;
    }

    public virtual void Start() { }

    private void GameTimeStarted() {
        Action<GameStage> gameInitialized = null;
        gameInitialized = (gameStage) => {
            if (gameStage == GameStage.GameTime) {
                OnGameStageGameTime();
            } else {
                GameSceneManager.OnGameStageChanged -= gameInitialized;
            }
        };

        GameSceneManager.OnGameStageChanged += gameInitialized;
    }

    protected virtual void OnGameStageGameTime() {
        skillProperties.PropertiesUserStartInitialized();
    }

    /// <summary>
    /// Gets references for crucial components
    /// </summary>
    private void AcquireComponents() {
        Animator = GetComponent<Animator>();
        if (CharacterComponent == null) {
            CharacterComponent = GetComponent<Character>();
        }
        CasterHitLayers = GetComponent<IHitLayers>();

        releasePoint = releasePoint == null ? Utils.FindChildObjectInParentByTag(gameObject, "SkillReleasePoint").transform : releasePoint;
    }

    /// <summary>
    /// Sets up properties in scriptable object to recalculate, assign references to skill property object and setup listener to rebuild strings when stats change
    /// </summary>
    private void InitializeSkillProperties() {
        PrepareProperties();
        skillProperties.Initialize();
    }

    protected virtual void PrepareProperties() {
        skillProperties.IsCopy = false;
        skillProperties = skillProperties.GetCopy<SkillProperties>();
        skillProperties.IsCopy = false;
        skillProperties.CharacterComponent = CharacterComponent;
    }

    private void InitializeSkillPropertiesFunctions() {
        StatusEffectsManager stf = CharacterComponent.CharacterStatusEffectsManager;

        stf.OnCompleteControlDisabled += OnCompleteControlDisabled;
        stf.OnIsStunnedChanged += OnStunned;
        stf.OnIsFrozenChanged += OnFrozen;
        stf.OnIsRootedChanged += OnRooted;
        stf.OnIsDeadChanged += OnDeath;
        stf.OnIsDeadChangedEarlyInvoke += OnDeathEarly;
        CharacterComponent.OnCompleteDeath += OnCompleteDestroy;
    }

    #region Cooldown

    public void ApplyCooldown() {
        if (skillProperties.chargeSystem.ChargeSystemBeingUsed()) {
            if (skillProperties.chargeSystem.ConsumeCharges() && CurrentCooldown <= 0) {
                ApplyCooldown(skillProperties.cooldown.GetValue());
            }
        } else {
            ApplyCooldown(skillProperties.cooldown.GetValue());
        }
    }

    public virtual void ApplyCooldown(float cooldownTime, float startingCooldown = 0) {
        if (cooldownTime <= 0f) {
            skillProperties.chargeSystem.ReplenishCharges();
            return;
        }

        if (cooldownTime <= CurrentCooldown) { return; }
        CurrentCooldown = cooldownTime;
        CurrentStartingCooldown = cooldownTime;
        CoolDownManager.ProcessCooldown(this);
        OnCooldownStart?.Invoke(this);
    }

    public virtual bool HandleCooldown(float deltaTime) {
        CurrentCooldown -= deltaTime;
        OnCooldownChanged?.Invoke(this);
        if (CurrentCooldown <= 0f) {
            if (skillProperties.chargeSystem.ReplenishCharges()) {
                CurrentCooldown = 0f;
                CurrentStartingCooldown = 0f;
                OnCooldownEnd?.Invoke(this);
                return true;
            } else {
                CurrentCooldown = skillProperties.cooldown.GetValue();
            }
        }
        return false;
    }

    #endregion

    public virtual void TurnCharacterTowardsCastPoint(Vector3 point) {
        //overriden
    }

    public virtual void ConsumeResource() { }

    public virtual void TriggerOnSkillFiredEventExternally() {
        OnSkillFired?.Invoke(this);
    }

    public bool IsSkillOfType(SkillType skillType) => skillProperties.IsSkillOfType(skillType);

    #region Core Skill Casting Methods

    public virtual bool CastSkill() {
        switch (skillProperties.SkillCastType) {
            case SkillCastType.Cast:
                CharacterComponent.CharacterStatusEffectsManager.SetIsCasting(true);
                CharacterComponent.CharacterStatusEffectsManager.SetCurrentSkillCasting(this);
                break;
            case SkillCastType.Channel:
                CharacterComponent.CharacterStatusEffectsManager.SetIsChanneling(true);
                CharacterComponent.CharacterStatusEffectsManager.SetCurrentSkillCasting(this);
                break;
            case SkillCastType.Instant:
                CharacterComponent.CharacterStatusEffectsManager.SetCurrentSkillCasting(this);
                break;
        }

        OnSkillStartedCasting?.Invoke(this);
        return true;
    }

    public virtual void SkillAnimationStart() { }

    public virtual void FireSkill() {
        ApplyCooldown();

        OnSkillFired?.Invoke(this);

        if (particlesOnSkillFired != null) {
            if (fireOnSkillFiredParticlesFrom == null) {
                fireOnSkillFiredParticlesFrom = CharacterComponent.VfxSpawnPosMiddle;
            }

            VfxManager.PlayOneShotParticle(particlesOnSkillFired, fireOnSkillFiredParticlesFrom.position, fireOnSkillFiredParticlesFrom.rotation);
        }

        SkillFired(skillProperties);
    }

    public virtual void SkillFired(SkillProperties skillProperties) {
        CastPoint = Vector3.zero;
        Target = null;
    }

    public virtual bool CanCast(bool checkForTargetDistance = false) {
        return false;
    }

    protected void AllowInterrupt(SkillInterruptType skillInterruptType) {
        switch (skillInterruptType) {
            case SkillInterruptType.ByCastingAndMovement:
                CharacterComponent.CharacterStatusEffectsManager.SetIsCasting(false);
                CharacterComponent.CharacterStatusEffectsManager.SetCanInterruptAnim(true);
                break;
            case SkillInterruptType.ByMovement:
                CharacterComponent.CharacterStatusEffectsManager.SetCanInterruptAnim(true);
                break;
        }
    }

    public virtual void FullCastDone() {
        CharacterComponent.CharacterStatusEffectsManager.SetCanInterruptAnim(false);
        Animator.SetTrigger(fullCastDoneHash);
        SkillCastFinished();
    }

    public void SkillCastFinished() {
        OnFullCastDone?.Invoke(this);
    }

    public void SetAnimationTrigger(int triggerHash, int floatHash = -1, float speedValue = 0f) {
        Animator.SetTrigger(triggerHash);
        currentSkillTriggerHash = triggerHash;
        if (floatHash != -1 && speedValue != 0f) { Animator.SetFloat(floatHash, speedValue); }
    }

    public void ResetAnimationTrigger() {
        if (currentSkillTriggerHash != -1) {
            Animator.ResetTrigger(currentSkillTriggerHash);
            currentSkillTriggerHash = -1;
        }
    }

    public void SelectAndUseSpammableAnim() {
        int random = UnityEngine.Random.Range(0, skillProperties.HashedSkillTriggers.Length);
        HashedSkillTriggers skillTriggerProperty = skillProperties.HashedSkillTriggers[random];
        string triggerName = skillProperties.SkillTriggers[random].triggerName;

        switch (triggerName) {
            case "Spammable_1":
                SetAnimationTrigger(skillTriggerProperty.triggerHash, skillTriggerProperty.animationSpeedFloatHash, skillProperties.AnimationCurrentSpeedModifier);
                break;
            case "Spammable_2":
                SetAnimationTrigger(skillTriggerProperty.triggerHash, skillTriggerProperty.animationSpeedFloatHash, skillProperties.AnimationCurrentSpeedModifierSecond);
                break;
            case "Spammable_3":
                SetAnimationTrigger(skillTriggerProperty.triggerHash, skillTriggerProperty.animationSpeedFloatHash, skillProperties.AnimationCurrentSpeedModifierThird);
                break;
        }
    }

    #endregion

    public virtual void OnCompleteControlDisabled(bool state) { }
    public virtual void OnStunned(bool state) { }
    public virtual void OnFrozen(bool state) { }
    public virtual void OnRooted(bool state) { }
    public virtual void OnDeath(bool state) { }
    public virtual void OnDeathEarly(bool state) { }
    public virtual void OnCompleteDestroy(Character character) { }

    public HitOutput HitEnemy(Collider other, float damageValue, string currentHitInfoId, List<DamageTypeWeight> preferedDamageTypes = null) {
        return HitInput.HitEnemy(other, damageValue, currentHitInfoId, CharacterComponent, skillProperties, null, null, null,
            this, CastPoint, Target, preferedDamageTypes);
    }

    public HitOutput HitEnemy(Combat targetCombat, float damageValue, string currentHitInfoId, List<DamageTypeWeight> preferedDamageTypes = null) {
        return HitInput.HitEnemy(targetCombat, damageValue, currentHitInfoId, CharacterComponent, skillProperties, null, null, null,
            this, CastPoint, Target, preferedDamageTypes);
    }

    public HitOutput HitEnemyExactDamage(Collider other, float exactDamage, string currentHitInfoId, List<DamageTypeWeight> preferedDamageTypes = null) {
        return HitInput.HitEnemyExactDamage(other, exactDamage, currentHitInfoId, CharacterComponent, skillProperties, null, null, null,
            this, CastPoint, Target, preferedDamageTypes);
    }

    public HitOutput HitEnemyExactDamage(Combat targetCombat, float exactDamage, string currentHitInfoId, List<DamageTypeWeight> preferedDamageTypes = null) {
        return HitInput.HitEnemyExactDamage(targetCombat, exactDamage, currentHitInfoId, CharacterComponent, skillProperties, null, null, null,
            this, CastPoint, Target, preferedDamageTypes);
    }

    protected virtual void SetPooling(GameObject obj, int amount, int poolCapacity) {
        throw new NotImplementedException("Set Pooling must be overriden!");
    }
}
