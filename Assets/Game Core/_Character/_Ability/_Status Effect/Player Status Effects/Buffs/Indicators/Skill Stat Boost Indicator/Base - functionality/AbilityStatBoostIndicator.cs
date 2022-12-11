using UnityEngine;

[CreateAssetMenu(fileName = "Ability Stat Boost Indicator", menuName = "Status Effects/Status Effect Indicators/Ability Stat Boost Indicator")]
public class AbilityStatBoostIndicator : Buff
{
    [SerializeField, TextArea] string buffText;
    public string BuffText => buffText;

    [SerializeField] StatStringType statStringType;
    public StatStringType StatStringType => statStringType;

    private float boostValue;
    public float BoostValue {
        get => boostValue;
        set {
            boostValue = value;
            StatusEffectProperties.SetTooltipIsDirty();
        }
    }

    public string BoostStatName { get; set; }

    public void SetStacks(int stacks) {
        if(StatusEffectProperties.maxStacks.GetValue() < stacks) {
            StatusEffectProperties.maxStacks.SetMinMax(0, stacks);
            StatusEffectProperties.maxStacks.SetPrimaryValue(stacks);
        }

        CurrentStacks = stacks;
    }
}
