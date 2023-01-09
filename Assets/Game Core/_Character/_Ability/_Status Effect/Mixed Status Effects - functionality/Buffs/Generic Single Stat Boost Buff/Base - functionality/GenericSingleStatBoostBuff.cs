using UnityEngine;

[CreateAssetMenu(fileName = "Generic Single Stat Boost Buff", menuName = "Status Effects/Buffs/Generic Single Stat Boost Buff")]
public class GenericSingleStatBoostBuff : Buff {

    private GenericSingleStatBoostBuffProperties genericSingleStatBoostBuffProperties;

    public override void Awake() {
        base.Awake();

        if (BuffTypes == null || BuffTypes.Length == 0) {
            BuffTypes = new BuffType[1] { BuffType.StatBoost };
        }
    }

    public override void Apply(int stacksCount, HitOutput hitOutput) {
        base.Apply(stacksCount, hitOutput);

        genericSingleStatBoostBuffProperties = statusEffectProperties as GenericSingleStatBoostBuffProperties;

        switch (genericSingleStatBoostBuffProperties.StatAddType) {
            case StatValueType.Absolute:
                AppliedToCharacterComponent.CharacterStats.AddAbsoluteStat(genericSingleStatBoostBuffProperties.StatToBoost,
                    genericSingleStatBoostBuffProperties.statBoostValue.GetValue());
                break;
            case StatValueType.Relative:
                AppliedToCharacterComponent.CharacterStats.AddRelativeStat(genericSingleStatBoostBuffProperties.StatToBoost,
                 genericSingleStatBoostBuffProperties.statBoostValue.GetValue());
                break;
            case StatValueType.Collective:
                AppliedToCharacterComponent.CharacterStats.AddTotalStat(genericSingleStatBoostBuffProperties.StatToBoost,
                 genericSingleStatBoostBuffProperties.statBoostValue.GetValue());
                break;
        }
    }

    public override void End() {
        base.End();
        switch (genericSingleStatBoostBuffProperties.StatAddType) {
            case StatValueType.Absolute:
                AppliedToCharacterComponent.CharacterStats.RemoveAbsoluteStat(genericSingleStatBoostBuffProperties.StatToBoost,
                    genericSingleStatBoostBuffProperties.statBoostValue.GetValue());
                break;
            case StatValueType.Relative:
                AppliedToCharacterComponent.CharacterStats.RemoveRelativeStat(genericSingleStatBoostBuffProperties.StatToBoost,
                 genericSingleStatBoostBuffProperties.statBoostValue.GetValue());
                break;
            case StatValueType.Collective:
                AppliedToCharacterComponent.CharacterStats.RemoveTotalStat(genericSingleStatBoostBuffProperties.StatToBoost,
                 genericSingleStatBoostBuffProperties.statBoostValue.GetValue());
                break;
        }
    }
}
