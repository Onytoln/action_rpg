using UnityEngine;


[CreateAssetMenu(fileName = "Mana Percentage Restoration Buff", menuName = "Status Effects/Buffs/Mana Percentage Restoration Buff")]
public class ManaRestorePercentageBuff : Buff {

    [SerializeField] private ParticleSystem manaRestoreParticlesPrefab;

    private ManaRestorePercentageBuffProperties manaRestorePercentageBuffProperties;

    public override void Awake() {
        base.Awake();

        if (BuffTypes == null || BuffTypes.Length == 0) {
            BuffTypes = new BuffType[2] { BuffType.ManaRestoreOverTime, BuffType.ManaRestore };
        }
    }

    public override void Apply(int stacksCount, HitOutput hitOutput) {
        base.Apply(stacksCount, hitOutput);
        manaRestorePercentageBuffProperties = statusEffectProperties as ManaRestorePercentageBuffProperties;
    }

    public override void Tick(float deltaTime) {
        base.Tick(deltaTime);
        if (CurrentTickTime >= statusEffectProperties.tickRate.GetValue()) {
            CurrentTickTime = 0;
            AppliedToCharacterComponent.CharacterCombat.RestoreManaPercetage(manaRestorePercentageBuffProperties.percentManaPerTick.GetValue());

            ManaRestoreVfx();
        }
    }

    private void ManaRestoreVfx() {
        if (manaRestoreParticlesPrefab != null) {
            VfxManager.PlayOneShotParticle(manaRestoreParticlesPrefab, AppliedToCharacterComponent.transform);
        }
    }
}
