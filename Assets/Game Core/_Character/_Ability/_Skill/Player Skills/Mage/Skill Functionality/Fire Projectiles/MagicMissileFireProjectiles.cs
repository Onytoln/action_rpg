using System.Collections;
using System;
using UnityEngine;

public class MagicMissileFireProjectiles : FireProjectilesDefault
{
    private Action<ICooldown> CooldownSetAction;
    private Action<ICooldown> CooldownChangedAction;

    private MagicMissileProperties magicMissileProperties;

    private float addedDamage;
    [SerializeField] AbilityStatBoostIndicator indicator;
    private AbilityStatBoostIndicator indicatorApplied;
    bool needToApplyIndicator;

    private bool skillInterrupted;
    private float interruptCooldown;
    

    public override void Awake() {
        base.Awake();

        magicMissileProperties = skillProperties as MagicMissileProperties;

        CooldownSetAction = (iCd) => {
            if(skillInterrupted) {
                skillInterrupted = false;
                return;
            }
           
            float damageToAdd = Mathf.Clamp(
                (iCd.CurrentCooldown - interruptCooldown) /
                (magicMissileProperties.cooldownForMaxDamageBonus.GetValue() * magicMissileProperties.damageBonusByCooldown.GetValue()),
                0f,
                magicMissileProperties.damageBonusByCooldown.GetValue());
            magicMissileProperties.abilityDamage.AddRelativeModifier(damageToAdd, addedDamage);
            addedDamage = damageToAdd;

            if (indicatorApplied == null) {
                indicatorApplied = indicator.GetCopy() as AbilityStatBoostIndicator;
                needToApplyIndicator = true;
            }
            indicatorApplied.BoostStatName = "damage";
            indicatorApplied.BoostValue = damageToAdd;
            indicatorApplied.SetStacks((int)(damageToAdd * 100f));
       
            if (needToApplyIndicator) {
                needToApplyIndicator = false;
                CharacterComponent.CharacterStatusEffectsManager.ApplyStatusEffect(indicatorApplied, CharacterComponent, null, 1, out var _);
            }
        };

        OnCooldownStart += CooldownSetAction;
        OnSkillFired += EndIndicatorBuff;

        CooldownChangedAction = (cd) => {
            interruptCooldown -= Time.deltaTime;
            if(interruptCooldown <= 0f) {
                interruptCooldown = 0f;
                OnCooldownChanged -= CooldownChangedAction;
            }
        };

        CharacterComponent.CharacterStatusEffectsManager.OnSkillInterrupted += (skill, cooldown) => {
            if (skill == this) {
                skillInterrupted = true;
                interruptCooldown = cooldown;
                OnCooldownChanged -= CooldownChangedAction;
                OnCooldownChanged += CooldownChangedAction;
            }
        };

        OnActionBarAssigned += (skill, state) => {
            if (!state) {
                EndIndicatorBuff(this);
            }
        };
    }

    private void EndIndicatorBuff(SkillTemplate skillTemplate) {
        RemoveModifier();
        if (indicatorApplied == null) return;
        indicatorApplied.HasEnded = true;
    }

    private void RemoveModifier() {
        magicMissileProperties.abilityDamage.RemoveRelativeModifier(addedDamage);
        addedDamage = 0f;
    }
}
