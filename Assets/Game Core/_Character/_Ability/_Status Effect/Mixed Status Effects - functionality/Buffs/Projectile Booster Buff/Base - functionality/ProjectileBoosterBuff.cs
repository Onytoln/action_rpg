using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Booster Buff", menuName = "Status Effects/Buffs/Projectile Booster Buff")]
public class ProjectileBoosterBuff : Buff {

    [SerializeField, HideInInspector] private ProjectileBoosterBuffProperties projectileBoosterBuffProperties;

    [SerializeField, HideInInspector] private SkillTemplate skill;
    [SerializeField, HideInInspector] private SkillProperties skillToBoostProperties;
    private IProjectile projectileProperties;

    private Action<SkillTemplate> skillFiredAction;

    private int addedProjectiles;
    public int AddedProjectiles { get => addedProjectiles; }

    private float addedScaleValue;
    public float AddedScaleValue { get => addedScaleValue; }    

    public override void Awake() {
        base.Awake();

        if (BuffTypes == null || BuffTypes.Length == 0) {
            BuffTypes = new BuffType[1] { BuffType.ProjectileBooster };
        }
    }

    public override void StatusEffectHolderInitialized() {
        base.StatusEffectHolderInitialized();

        projectileBoosterBuffProperties = statusEffectProperties as ProjectileBoosterBuffProperties;

        skill = TargetManager.PlayerComponent.GetSkillById(projectileBoosterBuffProperties.SkillToBoostId);
        skillToBoostProperties = skill.skillProperties;
        projectileBoosterBuffProperties.SkillToBoostProperties = skillToBoostProperties;
    }

    public override void Apply(int stacksCount, HitOutput hitOutput) {
        base.Apply(stacksCount, hitOutput);
        
        projectileProperties = skillToBoostProperties as IProjectile;
        if (projectileProperties == null) {
            HasEnded = true;
            return;
        }

        if (projectileBoosterBuffProperties.RemoveOnSkillFired.Value) {
            skillFiredAction = (skillTemplate) => { HasEnded = true; };
            skill.OnSkillFired += skillFiredAction;
        }

        HandleBoosts();
    }

    public override void Refresh(CharStatsValContainer applierStatsContainer, int stacksCount, HitOutput hitOutput) {
        base.Refresh(applierStatsContainer, stacksCount, hitOutput);

        HandleBoosts();
    }

    private void HandleBoosts() {
        int projectilesToAdd = CurrentStacks / projectileBoosterBuffProperties.bonusProjectilePerStacks.GetValue();
        float scaleToAdd = CurrentStacks / projectileBoosterBuffProperties.bonusScalePerStacks.GetValue() * projectileBoosterBuffProperties.bonusScaleValue.GetValue();
 
        projectileProperties.ProjectileCount.AddAbsoluteModifier(projectilesToAdd, addedProjectiles);
        projectileProperties.Scale.AddAbsoluteModifier(scaleToAdd, addedScaleValue);

        addedProjectiles = projectilesToAdd;
        addedScaleValue = scaleToAdd;
    }

    public override void End() {
        base.End();

        if (projectileBoosterBuffProperties.RemoveOnSkillFired.Value) {
            skill.OnSkillFired -= skillFiredAction;
        }

        projectileProperties.ProjectileCount.RemoveAbsoluteModifier(addedProjectiles);
        projectileProperties.Scale.RemoveAbsoluteModifier(addedScaleValue);
    }
}
