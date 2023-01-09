using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectsManager : MonoBehaviour {

    public event Action<bool> OnIsCastingChanged;
    public event Action<bool> OnIsChannelingChanged;
    public event Action<bool> OnIsStunnedChanged;
    public event Action<bool> OnIsFrozenChanged;
    public event Action<bool> OnIsRootedChanged;
    public event Action<bool> OnIsSlowedChanged;
    public event Action<bool> OnIsSilencedChanged;
    public event Action<bool> OnCanRegenerateChanged;
    public event Action<bool> OnMovementSkillsDisabledChanged;
    public event Action<bool> OnIsInvulnerableChanged;
    public event Action<bool> OnIsUntargetableChanged;
    public event Action<bool> OnIsStationaryChanged;
    public event Action<bool> OnCompleteControlDisabled;
    public event Action<bool> OnIsDeadChanged;
    public event Action<bool> OnIsDeadChangedEarlyInvoke;

    private event Action<bool> OnIsDeadChangedLocalLateInvoke;

    public event Action<StatusEffect> OnStatusEffectApplied;
    public event Action<StatusEffect> OnStatusEffectEnd;

    public event Action<SkillTemplate, float> OnSkillInterrupted;

    public bool IsCasting { get; private set; }
    public bool IsChanneling { get; private set; }
    public bool CanInterruptAnim { get; private set; }
    public SkillTemplate CurrentSkillCasting { get; private set; }
    public SkillTemplate NextSkillCasting { get; private set; }

    //Applied by status effects

    private readonly BoolControlSimple isStunned = new BoolControlSimple(BoolType.False);
    public bool IsStunned { get => isStunned.Value; }

    private readonly BoolControlSimple isFrozen = new BoolControlSimple(BoolType.False);
    public bool IsFrozen { get => isFrozen.Value; }

    private readonly BoolControlSimple isRooted = new BoolControlSimple(BoolType.False);
    public bool IsRooted { get => isRooted.Value; }

    private readonly BoolControlSimple isSlowed = new BoolControlSimple(BoolType.False);
    public bool IsSlowed { get => isSlowed.Value; }

    private readonly BoolControlSimple isSilenced = new BoolControlSimple(BoolType.False);
    public bool IsSilenced { get => isSilenced.Value; }

    private readonly BoolControlSimple movementSkillsDisabled = new BoolControlSimple(BoolType.False);
    public bool MovementSkillsDisabled { get => movementSkillsDisabled.Value; }

    private readonly BoolControlSimple isInvulnerable = new BoolControlSimple(BoolType.False);
    public bool IsInvulnerable { get => isInvulnerable.Value; }

    private readonly BoolControlSimple isUntargetable = new BoolControlSimple(BoolType.False);
    public bool IsUntargetable { get => isUntargetable.Value; }

    //Other
    public bool CanRegenerate { get; private set; }
    public bool IsStationary { get; private set; }

    public bool IsDead { get; private set; }
    public bool IsRespawning { get; set; }

    //BUFFS AND DEBUFFS
    private readonly List<Buff> buffs = new List<Buff>();
    public List<Buff> Buffs { get => buffs; }
    private readonly Dictionary<BuffType, List<BuffImmunityContainer>> buffImmunities = new Dictionary<BuffType, List<BuffImmunityContainer>>();
    [SerializeField] private BuffImmunityContainer[] initialBuffImmunities = new BuffImmunityContainer[0];

    private readonly List<Debuff> debuffs = new List<Debuff>();
    public List<Debuff> Debuffs { get => debuffs; }
    private readonly Dictionary<DebuffType, List<DebuffImmunityContainer>> debuffImmunities = new Dictionary<DebuffType, List<DebuffImmunityContainer>>();
    [SerializeField] private DebuffImmunityContainer[] initialDebuffImmunities = new DebuffImmunityContainer[0];

    [SerializeField] private StatusEffect[] initialStatusEffects;

    //NECESSARRY REFERENCES
    private ICharacterControllerKeyFunc characterControllerKeyFunctions;
    [SerializeField] private Animator animator;
    [SerializeField] private new Collider collider;
    [SerializeField] private Character character;

    //when resetting triggers if stunned or other actions, do not reset these triggers
    private static readonly int[] triggerResetExceptionsHashes = { Animator.StringToHash("StunTrigger") };

    private static readonly int idleHash = Animator.StringToHash("Idle");
    private static readonly int isCastingHash = Animator.StringToHash("IsCasting");
    private static readonly int isChannelingHash = Animator.StringToHash("IsChanneling");
    private static readonly int stunTriggerHash = Animator.StringToHash("StunTrigger");
    private static readonly int stunnedHash = Animator.StringToHash("IsStunned");
    private static readonly int fullCastDoneHash = Animator.StringToHash("FullCastDone");

    bool requestForMonoDisable = false;
    bool nextFrame = true;

    private void Awake() {
        characterControllerKeyFunctions = GetComponent<ICharacterControllerKeyFunc>();

        if (animator == null) {
            animator = GetComponent<Animator>();
        }
        if (collider == null) {
            collider = GetComponent<Collider>();
        }
        if (character == null) {
            character = GetComponent<Character>();
        }

        OnIsDeadChangedLocalLateInvoke += (state) => {
            if (state) {
                ClearStatusEffectsOnDeath();
                nextFrame = false;

                if (buffs.Count == 0 && debuffs.Count == 0)
                    enabled = false;
                else
                    requestForMonoDisable = true;

            } else {
                requestForMonoDisable = false;
                enabled = true;
            }
        };
    }

    private void Start() {
        for (int i = 0; i < initialBuffImmunities.Length; i++) {
            AddBuffImmunity(initialBuffImmunities[i].BuffType, initialBuffImmunities[i].Removable);
        }

        for (int i = 0; i < initialDebuffImmunities.Length; i++) {
            AddDebuffImmunity(initialDebuffImmunities[i].DebuffType, initialDebuffImmunities[i].Removable);
        }

        for (int i = 0; i < initialStatusEffects.Length; i++) {
            initialStatusEffects[i].Initialize();
            _ = ApplyStatusEffect(initialStatusEffects[i], character, null, 1, out var _);
        }
    }

    private void Update() {
        if (buffs.Count == 0 && debuffs.Count == 0) return;

        float deltaTime = Time.deltaTime;
        nextFrame = true;

        for (int i = buffs.Count; i-- > 0;) {
            if (buffs[i].HasEnded) {
                EndBuff(i);
                continue;
            }

            buffs[i].Tick(deltaTime);
        }

        for (int i = debuffs.Count; i-- > 0;) {
            if (debuffs[i].HasEnded) {
                EndDebuff(i);
                continue;
            }

            debuffs[i].Tick(deltaTime);
        }

        if (requestForMonoDisable && nextFrame) {
            requestForMonoDisable = false;
            enabled = false;
        }
    }

    #region Buff/Debuffs

    public bool ApplyStatusEffect(StatusEffect statusEffect, Character applier, CharStatsValContainer applierStatsContainer, int stacksCount,
        out StatusEffect appliedEffect, HitOutput hitOutput = null) {

        if (stacksCount <= 0) { appliedEffect = null; return false; }
        if (statusEffect == null || applier == null) { appliedEffect = null; return false; }
        if (IsDead && !statusEffect.StatusEffectProperties.SurvivesDeath) { appliedEffect = null; return false; }

        StatusEffect applied = null;

        if (!statusEffect.CanApply(hitOutput)) goto End;

        StatusEffect statusEffectCopy = null;
        bool statusEffectAdded = false;

        if (statusEffect is Buff buff) {
            if (BuffImmunityContainsBuffType(buff)) goto End;

            var (refreshAt, hasInstance) = HasRefreshableStatusEffectOfId(buffs, buff.StatusEffectProperties.abilityId);
            if (refreshAt != -1) {
                buffs[refreshAt].Refresh(applierStatsContainer, stacksCount, hitOutput);
                applied = buffs[refreshAt];
            } else if (!hasInstance || (hasInstance && buff.StatusEffectProperties.MultiInstanced)) {
                buffs.Add(SECopy<Buff>());
                statusEffectAdded = true;
                applied = buffs[buffs.Count - 1];
            }
        } else if (statusEffect is Debuff debuff) {
            if (DebuffImmunityContainsDebuffType(debuff)) goto End;

            var (refreshAt, hasInstance) = HasRefreshableStatusEffectOfId(debuffs, debuff.StatusEffectProperties.abilityId);
            if (refreshAt != -1) {
                debuffs[refreshAt].Refresh(applierStatsContainer, stacksCount, hitOutput);
                applied = debuffs[refreshAt];
            } else if (!hasInstance || (hasInstance && debuff.StatusEffectProperties.MultiInstanced)) {
                debuffs.Add(SECopy<Debuff>());
                statusEffectAdded = true;
                applied = debuffs[debuffs.Count - 1];
            }
        }

        if (statusEffectAdded) {
            statusEffectCopy.PreApply(character, this, applier, applierStatsContainer);
            statusEffectCopy.Apply(stacksCount, hitOutput);
            statusEffectCopy.OnStartVfx();

            OnStatusEffectApplied?.Invoke(statusEffectCopy);

            appliedEffect = statusEffectCopy;
            return true;
        }

    End:

        appliedEffect = applied;
        return false;

        T SECopy<T>() where T : StatusEffect {
            return (T)(object)(statusEffectCopy = statusEffect.GetCopy());
        }
    }

    private (int refreshAt, bool hasInstance) HasRefreshableStatusEffectOfId<T>(List<T> list, string id) where T : StatusEffect {
        bool hasInstance = false;

        for (int i = 0; i < list.Count; i++) {
            if (!list[i].StatusEffectProperties.abilityId.Equals(id)) continue;

            if (list[i].CanRefresh()) {
                return (i, true);
            } else {
                hasInstance = true;
                continue;
            }
        }

        return (-1, hasInstance);
    }

    private bool EndStatusEffect(StatusEffect statusEffect) {
        if (statusEffect == null) return false;

        statusEffect.OnEndVfx();
        statusEffect.End();

        OnStatusEffectEnd?.Invoke(statusEffect);
        statusEffect.DestroyIfCopy();
        return true;
    }

    private void EndBuff(int index) {
        Buff buffToRemove = buffs[index];
        buffs.RemoveAt(index);
        _ = EndStatusEffect(buffToRemove);
    }

    private void EndDebuff(int index) {
        Debuff debuffToRemove = debuffs[index];
        debuffs.RemoveAt(index);
        _ = EndStatusEffect(debuffToRemove);
    }

    public bool EndStatusEffectExternal(StatusEffect statusEffect) {
        if (statusEffect == null) return false;

        if (statusEffect is Buff buff) {
            int index = buffs.FindIndex(x => x.StatusEffectProperties.abilityId == statusEffect.StatusEffectProperties.abilityId);
            if (index != -1) {
                EndBuff(index);
                return true;
            }
        } else {
            int index = debuffs.FindIndex(x => x.StatusEffectProperties.abilityId == statusEffect.StatusEffectProperties.abilityId);
            if (index != -1) {
                EndDebuff(index);
                return true;
            }
        }

        return false;
    }

    public bool ContainsStatusEffect(StatusEffect statusEffect) {
        if (statusEffect is Buff buff) {
            for (int i = 0; i < buffs.Count; i++) {
                if (buffs[i] == buff) return true;
            }
        } else {
            Debuff debuff = statusEffect as Debuff;

            for (int i = 0; i < debuffs.Count; i++) {
                if (debuffs[i] == debuff) return true;
            }
        }

        return false;
    }

    public bool ContainsBuffOfId(string buffId) {
        if (buffs.Count == 0) return false;

        for (int i = 0; i < buffs.Count; i++) {
            if (buffs[i].StatusEffectProperties.abilityId == buffId) return true;
        }

        return false;
    }

    public bool ContainsDebuffOfId(string debuffId) {
        if (debuffs.Count == 0) return false;

        for (int i = 0; i < debuffs.Count; i++) {
            if (debuffs[i].StatusEffectProperties.abilityId == debuffId) return true;
        }

        return false;
    }

    public bool ContainsBuffOfType(BuffType buffType) {
        if (buffs.Count == 0) return false;

        for (int i = 0; i < buffs.Count; i++) {
            for (int j = 0; j < buffs[i].BuffTypes.Length; j++) {
                if (buffs[i].BuffTypes[j] == buffType) return true;
            }
        }

        return false;
    }

    public bool ContainsDebuffOfType(DebuffType debuffType) {
        if (debuffs.Count == 0) return false;

        for (int i = 0; i < debuffs.Count; i++) {
            for (int j = 0; j < debuffs[i].DebuffTypes.Length; j++) {
                if (debuffs[i].DebuffTypes[j] == debuffType) return true;
            }
        }

        return false;
    }

    public bool BuffImmunityContainsBuffType(Buff buff) {
        if (buffImmunities.Count == 0) return false;

        for (int i = 0; i < buff.BuffTypes.Length; i++) {
            if (buffImmunities.ContainsKey(buff.BuffTypes[i])) return true;
        }

        return false;
    }

    public bool DebuffImmunityContainsDebuffType(Debuff debuff) {
        if (debuffImmunities.Count == 0) return false;

        for (int i = 0; i < debuff.DebuffTypes.Length; i++) {
            if (debuffImmunities.ContainsKey(debuff.DebuffTypes[i])) return true;
        }

        return false;
    }

    public bool AddBuffImmunity(BuffType buffType, bool removable = true) {
        _ = Utils.AddToDictList(buffImmunities, buffType, new BuffImmunityContainer() { BuffType = buffType, Removable = removable });

        bool atLeastOneBuffRemoved = false;

        for (int i = 0; i < buffs.Count; i++) {
            if (BuffImmunityContainsBuffType(buffs[i])) {
                buffs[i].HasEnded = true;
                atLeastOneBuffRemoved = true;
            }
        }

        return atLeastOneBuffRemoved;
    }

    public bool AddDebuffImmunity(DebuffType debuffType, bool removable = true) {
        _ = Utils.AddToDictList(debuffImmunities, debuffType, new DebuffImmunityContainer() { DebuffType = debuffType, Removable = removable });

        bool atLeastOneDebuffRemoved = false;

        for (int i = 0; i < debuffs.Count; i++) {
            if (DebuffImmunityContainsDebuffType(debuffs[i])) {
                debuffs[i].HasEnded = true;
                atLeastOneDebuffRemoved = true;
            }
        }

        return atLeastOneDebuffRemoved;
    }

    /// <summary>
    /// Removes only one buff immunity that is removable
    /// </summary>
    /// <param name="buffType"></param>
    /// <returns></returns>
    public bool RemoveBuffImmunity(BuffType buffType) {
        if (!buffImmunities.TryGetValue(buffType, out var values)) return false;

        for (int i = 0; i < values.Count; i++) {
            if (values[i].Removable) {
                Utils.RemoveFromDictList(buffImmunities, buffType, values[i]);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes only one debuff immunity that is removable
    /// </summary>
    /// <param name="debuffType"></param>
    /// <returns></returns>
    public bool RemoveDebuffImmunity(DebuffType debuffType) {
        if (!debuffImmunities.TryGetValue(debuffType, out var values)) return false;

        for (int i = 0; i < values.Count; i++) {
            if (values[i].Removable) {
                Utils.RemoveFromDictList(debuffImmunities, debuffType, values[i]);
                return true;
            }
        }

        return false;
    }

    public void ClearStatusEffectsOnDeath() {
        for (int i = buffs.Count; i-- > 0;) {
            if (!buffs[i].StatusEffectProperties.SurvivesDeath) {
                buffs[i].HasEnded = true;
            }
        }

        for (int i = debuffs.Count; i-- > 0;) {
            if (!debuffs[i].StatusEffectProperties.SurvivesDeath) {
                debuffs[i].HasEnded = true;
            }
        }
    }

    #endregion

    #region Core

    public void SetIsCasting(bool value) {
        if (IsCasting == value) return;
        IsCasting = value;

        if (IsCasting) { characterControllerKeyFunctions.StopMovement(); }

        animator.SetBool(isCastingHash, IsCasting);

        OnIsCastingChanged?.Invoke(IsCasting);
    }

    public void SetIsChanneling(bool value) {
        if (IsChanneling == value) return;
        IsChanneling = value;

        animator.SetBool(isChannelingHash, IsChanneling);

        OnIsChannelingChanged?.Invoke(IsChanneling);
    }

    public void SetCanInterruptAnim(bool value) {
        if (CanInterruptAnim == value) return;
        CanInterruptAnim = value;
    }

    public void SetCurrentSkillCasting(SkillTemplate skill) {
        if (skill != null && skill.SkillCastType == SkillCastType.Instant && skill is PlayerSkillTemplate playerSkill) {
            playerSkill.StandaloneCast(skill.CurrentDesiredCastPoint, skill.CurrentDesiredTarget, new RegularStandAloneCastParameter());
            return;
        }

        if (CurrentSkillCasting == null) {
            CurrentSkillCasting = skill;
            return;
        } else if (skill == null) {
            CurrentSkillCasting = skill;
            return;
        }

        NextSkillCasting = skill;

        if (CurrentSkillCasting != skill) {
            if(skill.SkillCastType == SkillCastType.Channel) {
                ApplyCooldownToCurrentSkillCastingBasedOnCastTime();
            } 

            CurrentSkillFullCastFinished();
            return;
        }
    }

    public void SetIsStunned(bool value) {
        isStunned.Set(value);

        if (isStunned.PrevEqualsCurrent) return;

        if (IsStunned) {
            characterControllerKeyFunctions.StopMovement();
            animator.SetTrigger(stunTriggerHash);
            animator.SetBool(stunnedHash, true);

            EndSkillCasts();
            ReturnToDefaultAnimationState();

            animator.ResetTrigger(fullCastDoneHash);
        } else {
            animator.SetBool(stunnedHash, false);
        }

        OnIsStunnedChanged?.Invoke(IsStunned);
        OnCompleteControlDisabled?.Invoke(IsStunned);
    }

    public void SetIsFrozen(bool value) {
        isFrozen.Set(value);

        if (isFrozen.PrevEqualsCurrent) return;

        if (IsFrozen) {
            characterControllerKeyFunctions.StopMovement();
            animator.speed = 0;
        } else {
            animator.speed = 1;
        }

        OnIsFrozenChanged?.Invoke(IsFrozen);
        OnCompleteControlDisabled?.Invoke(IsStunned);
    }

    public void SetIsRooted(bool value) {
        isRooted.Set(value);

        if (isRooted.PrevEqualsCurrent) return;

        if (IsRooted) { characterControllerKeyFunctions.StopMovement(); }

        OnIsRootedChanged?.Invoke(IsRooted);
    }

    public void SetIsSlowed(bool value) {
        isSlowed.Set(value);

        if (isSlowed.PrevEqualsCurrent) return;

        OnIsSlowedChanged?.Invoke(IsSlowed);
    }

    public void SetIsSilenced(bool value) {
        isSilenced.Set(value);

        if (isSilenced.PrevEqualsCurrent) return;

        OnIsSilencedChanged?.Invoke(IsSilenced);
    }

    public void SetMovementSkillsDisabled(bool value) {
        movementSkillsDisabled.Set(value);

        if (movementSkillsDisabled.PrevEqualsCurrent) return;

        OnMovementSkillsDisabledChanged?.Invoke(MovementSkillsDisabled);
    }

    public void SetIsInvulnerable(bool value) {
        isInvulnerable.Set(value);

        if (isInvulnerable.PrevEqualsCurrent) return;

        OnIsInvulnerableChanged?.Invoke(IsInvulnerable);
    }

    public void SetIsUntargetable(bool value) {
        isUntargetable.Set(value);

        if (isUntargetable.PrevEqualsCurrent) return;

        if (IsUntargetable) {
            collider.enabled = false;
        } else {
            collider.enabled = true;
        }

        OnIsUntargetableChanged?.Invoke(IsUntargetable);
    }

    public void SetCanRegenerate(bool value) {
        if (CanRegenerate == value) return;
        CanRegenerate = value;

        OnCanRegenerateChanged?.Invoke(CanRegenerate);
    }

    public void SetIsStationary(bool value) {
        if (IsStationary == value) return;
        IsStationary = value;

        OnIsStationaryChanged?.Invoke(IsStationary);
    }

    public void SetIsDead(bool value) {
        if (IsDead == value) return;
        IsDead = value;

        animator.SetBool("IsDead", IsDead);

        OnIsDeadChangedEarlyInvoke?.Invoke(value);

        if (value) {
            EndSkillCasts();
            ReturnToDefaultAnimationState();

            animator.ResetTrigger(fullCastDoneHash);
        }

        OnIsDeadChanged?.Invoke(IsDead);
        OnIsDeadChangedLocalLateInvoke?.Invoke(IsDead);
    }

    public bool CanMove() {
        return !IsCasting && !IsStunned && !IsFrozen && !IsRooted && !IsDead && !IsRespawning;
    }

    public bool CanCast(SkillTemplate skill) {
        bool ignoreCast = false;
        bool ignoreChannel = false;

        if (skill != null) {
            if (MovementSkillsDisabled && skill.IsSkillOfType(SkillType.Movement)) return false;

            if ((CurrentSkillCasting != null && CurrentSkillCasting.skillProperties.SkillCastType == SkillCastType.Channel)
                || skill.skillProperties.SkillCastType == SkillCastType.Instant) {

                ignoreCast = true;
                ignoreChannel = true;
            }
        }

        return (!IsCasting || ignoreCast) && (!IsChanneling || ignoreChannel) && !IsStunned && !IsFrozen && !IsSilenced && !IsDead && !IsRespawning;
    }

    public void CurrentSkillStartedCasting() {
        if (CurrentSkillCasting == null) { return; }
        CurrentSkillCasting.SkillAnimationStart();
    }

    public void FireCurrentSkillCasting() {
        if (CurrentSkillCasting == null) { return; }
        CurrentSkillCasting.FireSkill();
    }

    public void CurrentSkillFullCastFinished() {
        if (CurrentSkillCasting == null) return;
        if (NextSkillCasting == null) {
            SetIsCasting(false);
            SetIsChanneling(false);
        }
        CurrentSkillCasting.FullCastDone();
        SetCurrentSkillCasting(null);
        CurrentSkillCasting = NextSkillCasting;
        NextSkillCasting = null;
    }

    public void EndSkillCasts() {
        NextSkillCasting = null;
        if (CurrentSkillCasting != null) CurrentSkillCasting.ResetAnimationTrigger();
        CurrentSkillFullCastFinished();
    }

    public void ReturnToDefaultAnimationState() {
        animator.CrossFade(idleHash, 0f, 0);
    }

    public void TryInterruptCurrentAnim() {
        if (CurrentSkillCasting == null) return;

        if (CanInterruptAnim) {
            ApplyCooldownToCurrentSkillCastingBasedOnCastTime();
            EndSkillCasts();
            CanInterruptAnim = false;
        }
    }

    private void ApplyCooldownToCurrentSkillCastingBasedOnCastTime() {
        float interruptCooldown = CurrentSkillCasting.skillProperties.castTime.GetValue() * (1 - animator.GetCurrentAnimatorStateInfo(0).normalizedTime);

        OnSkillInterrupted?.Invoke(CurrentSkillCasting, interruptCooldown);
        OnSkillInterrupted?.Invoke(NextSkillCasting, 0f);

        CurrentSkillCasting.ApplyCooldown(interruptCooldown);
    }

    #endregion
}



