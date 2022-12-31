using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "SkillProperties", menuName = "Skill/Skill Properties Base")]
public class SkillProperties : AbilityProperties {
    [Header("Skill Properties")]
    //--------Skill-stats--------
    public StatFloat manaCost;
    [field: SerializeField] public SkillCastType SkillCastType { get; private set; }
    [field: SerializeField] public SkillCastSpeedScalingType SkillCastSpeedScalingType { get; private set; }
    [field: SerializeField] public SkillType[] SkillTypes { get; private set; }
    public ChargeSystem chargeSystem;

    //buffs and debuffs
    public BuffHolder[] buffHolder;
    public DebuffHolder[] debuffHolder;

    //***************************************************************************************//
    //first anim - scaling with attack speed is based on skill cast type (must be spammable)
    [Header("Animation duration settings")]
    public StatFloat castTime;
    [SerializeField] private AnimationClip animationClip;
    protected float animationDefaultSpeedModifier;

    protected float animationCurrentSpeedModifier;
    public float AnimationCurrentSpeedModifier => animationCurrentSpeedModifier;

    protected float castTimeAttackSpeedOldValue;
    //***************************************************************************************//
    //second anim - usually for spammable abilities - must set if scales with attack speed - !!!use only animationDurationDefaultValue if spammable!!!
    [Header("Second anim settings")]
    [Header("Animation duration settings - only for skills that use multiple animations")]
    public StatFloat castTime_second;
    [SerializeField] private AnimationClip animationClipSecond;
    protected float animationDefaultSpeedModifierSecond;

    protected float animationCurrentSpeedModifierSecond;
    public float AnimationCurrentSpeedModifierSecond => animationCurrentSpeedModifierSecond;

    protected float castTimeAttackSpeedOldValueSecond;
    //***************************************************************************************//
    //third anim - usually for spammable abilities - must set if scales with attack speed - !!!use only animationDurationDefaultValue if spammable!!!
    [Header("Third anim settings")]
    public StatFloat castTime_third;
    [SerializeField] private AnimationClip animationClipThird;
    protected float animationDefaultSpeedModifierThird;

    protected float animationCurrentSpeedModifierThird;
    public float AnimationCurrentSpeedModifierThird => animationCurrentSpeedModifierThird;

    protected float castTimeAttackSpeedOldValueThird;
    //***************************************************************************************//

    [SerializeField] private SkillTriggersProperties[] skillTriggers;
    public SkillTriggersProperties[] SkillTriggers {
        get => skillTriggers;
    }

    [SerializeField] private HashedSkillTriggers[] hashedSkillTriggers;
    public HashedSkillTriggers[] HashedSkillTriggers {
        get => hashedSkillTriggers;
    }

    [field: SerializeField] public int MinLevelRequired { get; private set; } = 0;

    public override AbilityPropertiesValuesContainer GetValuesCopy() {
        return new SkillPropertiesValuesContainer(this);
    }

    public override void Awake() {
        /*benefitFromCriticalStrike = new SkillStat[4];
        benefitFromCriticalStrike[0] = new SkillStat("Crit chance benefit absolute modifier", 0, 0, 1f);
        benefitFromCriticalStrike[1] = new SkillStat("Crit chance benefit relative modifier", 0, 0, 1f);
        benefitFromCriticalStrike[2] = new SkillStat("Crit damage benefit absolute modifier", 0, 0, 3f);
        benefitFromCriticalStrike[3] = new SkillStat("Crit damage benefit relative modifier", 0, 0, 1f);
        benefitFromPenetration = new SkillStat[5];
        benefitFromPenetration[0] = new SkillStat("Physical pene benefit", 0, 0, 1f) {penetrationType = StatType.PhysicalPenetration };
        benefitFromPenetration[1] = new SkillStat("Fire pene benefit", 0, 0, 1f) { penetrationType = StatType.FirePenetration };
        benefitFromPenetration[2] = new SkillStat("Ice pene benefit", 0, 0, 1f) { penetrationType = StatType.IcePenetration };
        benefitFromPenetration[3] = new SkillStat("Lightning pene benefit", 0, 0, 1f) { penetrationType = StatType.LightningPenetration };
        benefitFromPenetration[4] = new SkillStat("Poison pene benefit", 0, 0, 1f) { penetrationType = StatType.PoisonPenetration };*/
    }

    public override void OnValidate() {
        base.OnValidate();
        SkillTriggersIntoHashes();
    }

    public override void PropertiesUserStartInitialized() {
        base.PropertiesUserStartInitialized();
        for (int i = 0; i < buffHolder.Length; i++) {
            buffHolder[i].HolderFullyInitialized();
        }

        for (int i = 0; i < debuffHolder.Length; i++) {
            debuffHolder[i].HolderFullyInitialized();
        }
    }

    public override void Initialize() {
        SkillTriggersIntoHashes();

        base.Initialize();

        for (int i = 0; i < buffHolder.Length; i++) {
            buffHolder[i].Initialize(SetTooltipIsDirty);
        }

        for (int i = 0; i < debuffHolder.Length; i++) {
            debuffHolder[i].Initialize(SetTooltipIsDirty);
        }
    }

    private void SkillTriggersIntoHashes() {
        if (hashedSkillTriggers != null && hashedSkillTriggers.Length == SkillTriggers.Length) return;

        if (skillTriggers.Length > 1 && skillTriggers[skillTriggers.Length - 1].triggerName == skillTriggers[skillTriggers.Length - 2].triggerName) return;

        Debug.Log("hashing triggers");
        hashedSkillTriggers = new HashedSkillTriggers[skillTriggers.Length];
        for (int i = 0; i < hashedSkillTriggers.Length; i++) {
            hashedSkillTriggers[i] = new HashedSkillTriggers() {
                triggerHash = Animator.StringToHash(skillTriggers[i].triggerName),
                animationSpeedFloatHash = Animator.StringToHash(skillTriggers[i].animationSpeedFloatName)
            };
        }
    }

    public override void CheckProperties() {
        base.CheckProperties();
        if(SkillTriggers.Length != HashedSkillTriggers.Length) {
            Debug.LogError("Skill triggers array lenght IS NOT EQUAL to Hashed skill triggers array length!");
        }

        if(SkillTypes == null || SkillTypes.Length == 0) {
            Debug.LogError($"Skill of type {GetType()} and name {name} does not have any skill types assigned!");
        }

        if(maxCastRange.GetValue() < DataStorage.MIN_STOPPING_DISTANCE) {
            Debug.LogError($"Max cast range is not allowed to be set on value lower than {DataStorage.MIN_STOPPING_DISTANCE + DataStorage.SKILL_DISTANCE_OFFSET}," +
                $" automatically setting it to that value now.");
            maxCastRange.SetPrimaryValue(DataStorage.MIN_STOPPING_DISTANCE + DataStorage.SKILL_DISTANCE_OFFSET);
        }
    }

    public override void SetUpListeners() {
        base.SetUpListeners();
        if (CharacterComponent.CharacterStats.CoreStats == null) return;
        InitializeAnimations();
        CalculateAttackSpeedCastTimes(CharacterComponent.CharacterStats.GetStat(StatType.AttackSpeed));
        CalculateAnimationSpeed();

        CharacterComponent.CharacterStats.OnCharacterStatChange += CalculateAttackSpeedCastTimes;

        InitializeCastTimeListeners();
    }

    public override void SetTooltipRebuildIfRequired(CharacterStat stat) {
        if (stat.StatType == StatType.Damage) SetTooltipIsDirty();
    }

    private void InitializeAnimations() {
        int animsInitialized = 0;
       
        if (animationClip != null) {
            animationDefaultSpeedModifier = Utils.CalculateDesiredAnimationSpeedModifier(animationClip.length, castTime.GetValue());
            animsInitialized++;
        }

        if(animationClipSecond != null) {
            animationDefaultSpeedModifierSecond = Utils.CalculateDesiredAnimationSpeedModifier(animationClipSecond.length,
                castTime_second.GetValue() > 0f ? castTime_second.GetValue() : castTime.GetValue());
            animsInitialized++;
        }

        if(animationClipThird != null) {
            animationDefaultSpeedModifierThird = Utils.CalculateDesiredAnimationSpeedModifier(animationClipThird.length,
                castTime_third.GetValue() > 0f ? castTime_third.GetValue() : castTime.GetValue());
            animsInitialized++;
        }

        if(animsInitialized < HashedSkillTriggers.Length) {
            Debug.LogError($"Skill {name} expects {HashedSkillTriggers.Length} animation/s, but {animsInitialized} was assigned!");
        }
    }

    private void InitializeCastTimeListeners() {
        if (animationClip != null) {
            castTime.OnChanged += (skillStat) => {
                animationCurrentSpeedModifier = Utils.CalculateDesiredAnimationSpeedModifier(animationClip.length, castTime.GetValue());
                CalculateAnimationSpeed();
            };
        }

        if (animationClipSecond != null) {
            castTime_second.OnChanged += (skillStat) => {
                animationDefaultSpeedModifierSecond = Utils.CalculateDesiredAnimationSpeedModifier(animationClipSecond.length, castTime_second.GetValue());
                CalculateAnimationSpeed();
            };
        }

        if (animationClipThird != null) {
            castTime_third.OnChanged += (skillStat) => {
                animationDefaultSpeedModifierThird = Utils.CalculateDesiredAnimationSpeedModifier(animationClipThird.length, castTime_third.GetValue());
                CalculateAnimationSpeed();
            };
        }
    }

    public virtual void CalculateAttackSpeedCastTimes(CharacterStat stat) {
        if (SkillCastSpeedScalingType == SkillCastSpeedScalingType.Scalable && stat.StatType == StatType.AttackSpeed) {
            CalculateCastTimeAttackSpeedModifier(castTime, ref castTimeAttackSpeedOldValue);
        }
    }

    protected void CalculateCastTimeAttackSpeedModifier(StatFloat castTime, ref float castTimeOldModifier) {
        //calculate cast time absolute modifier to increase attacks per second accordingly
        float modifier = -(castTime.GetPrimaryValue() -
               castTime.GetPrimaryValue() / CharacterComponent.CharacterStats.CoreStats.AttackSpeedValue);
        castTime.AddAbsoluteModifier(modifier, castTimeOldModifier);
        castTimeOldModifier = modifier;
    }

    /// <summary>
    /// Must override whole method if I want skill with multiple cast times but not all cast times are of type spammabe (scaling with attack speed)
    /// </summary>
    protected virtual void CalculateAnimationSpeed() {
        CalculateFinalAnimationSpeed(ref animationCurrentSpeedModifier, animationDefaultSpeedModifier, castTime);

        if (animationDefaultSpeedModifierSecond > 0f && castTime_second.GetValue() <= 0f) {
            CalculateFinalAnimationSpeed(ref animationCurrentSpeedModifierSecond, animationDefaultSpeedModifierSecond, castTime);
        } else if(animationDefaultSpeedModifierSecond > 0f && castTime_second.GetValue() > 0f) {
            CalculateFinalAnimationSpeed(ref animationCurrentSpeedModifierSecond, animationDefaultSpeedModifierSecond, castTime_second);
        }

        if (animationDefaultSpeedModifierThird > 0f && castTime_third.GetValue() <= 0f) {
            CalculateFinalAnimationSpeed(ref animationCurrentSpeedModifierThird, animationDefaultSpeedModifierThird, castTime);
        } else if(animationDefaultSpeedModifierSecond > 0f && castTime_third.GetValue() > 0f) {
            CalculateFinalAnimationSpeed(ref animationCurrentSpeedModifierSecond, animationDefaultSpeedModifierSecond, castTime_third);
        }
    }

    protected void CalculateFinalAnimationSpeed(ref float animationResultvalue, float animationDefaultValue, StatFloat castTime) {
        //calculate new animation speed
        animationResultvalue = animationDefaultValue * (castTime.GetPrimaryValue() / castTime.GetValue());
    }

    public bool IsSkillOfType(SkillType skillType) {
        for (int i = 0; i < SkillTypes.Length; i++) {
            if (SkillTypes[i] == skillType) return true;
        }

        return false;
    }
}

