using UnityEngine;

[CreateAssetMenu(fileName = "Healing Percentage Buff", menuName = "Status Effects/Buffs/Healing Percentage Buff")]
public class HealingPercentageBuff : Buff {

    [SerializeField] private ParticleSystem healParticlesPrefab;

    private HealingPercentageBuffProperties healingPotionBuffProperties;

    public override void Awake() {
        base.Awake();

        if (BuffTypes == null || BuffTypes.Length == 0) {
            BuffTypes = new BuffType[2] { BuffType.HealOverTime, BuffType.Heal };
        }
    }

    public override void Apply(int stacksCount, HitOutput hitOutput) {
        base.Apply(stacksCount, hitOutput);
        healingPotionBuffProperties = statusEffectProperties as HealingPercentageBuffProperties;
    }

    public override void Tick(float deltaTime) {
        base.Tick(deltaTime);
        if (CurrentTickTime >= statusEffectProperties.tickRate.GetValue()) {
            CurrentTickTime = 0;
            AppliedToCharacterComponent.CharacterCombat.RestoreHealthPercentage(healingPotionBuffProperties.percentHealthPerTick.GetValue(),
                healingPotionBuffProperties.AffectedByHealingEffectivity);

            HealVfx();
        }
    }

    private void HealVfx() {
        if (healParticlesPrefab != null) {
            VfxManager.PlayOneShotParticle(healParticlesPrefab, AppliedToCharacterComponent.transform);
        }
    }
}
