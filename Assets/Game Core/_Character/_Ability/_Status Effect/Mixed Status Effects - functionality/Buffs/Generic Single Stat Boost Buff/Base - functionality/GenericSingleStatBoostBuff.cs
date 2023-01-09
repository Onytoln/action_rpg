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

        AppliedToCharacterComponent.CharacterStats.AddStatModifier(genericSingleStatBoostBuffProperties.StatToBoost,
                 genericSingleStatBoostBuffProperties.statBoostValue.Value,
                 genericSingleStatBoostBuffProperties.StatAddType);
    }

    public override void End() {
        base.End();

        AppliedToCharacterComponent.CharacterStats.RemoveStatModifier(genericSingleStatBoostBuffProperties.StatToBoost,
           genericSingleStatBoostBuffProperties.statBoostValue.Value,
           genericSingleStatBoostBuffProperties.StatAddType);
    }
}
