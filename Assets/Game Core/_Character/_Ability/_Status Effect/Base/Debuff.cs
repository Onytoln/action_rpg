using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debuff : StatusEffect {

    [SerializeField] private DebuffType[] debuffTypes;
    public DebuffType[] DebuffTypes { get => debuffTypes; set => debuffTypes = value; }

    private CountConstrainedList<float> debuffStrengthModifiers;
    [SerializeField] private int maxDebuffStrenghtModifiers = 1;

    private float finalDebuffStrenghtValue = 0f;
    protected float FinalDebuffStrenghtValue {
        get {
            if(finalDebuffStrenghtValue <= 0f && ApplierStatsContainer != null) {
                finalDebuffStrenghtValue = ApplierStatsContainer.DebuffStrenghtValue;
            }

            return finalDebuffStrenghtValue;
        }

        private set => finalDebuffStrenghtValue = value;
    }

    public float DebuffStrenghtModifier => 1f + FinalDebuffStrenghtValue - AppliedToStats.DebuffProtectionValue;

    protected void InitializeDebuffStrenghtModifiersList() {
        debuffStrengthModifiers = new CountConstrainedList<float>(maxDebuffStrenghtModifiers);
    }

    /// <summary>
    /// Adds debuff strenght stat to hashset and then calculated average of it for FinalDebuffStrenghtValue,
    /// has a max limit of values that hashset can contains, duplicates are ignored
    /// </summary>
    protected void UpdateDebuffStrengthModifiers(CharStatsValContainer applierStatsContainer) {
        if (applierStatsContainer == null) return;
        if (debuffStrengthModifiers == null) InitializeDebuffStrenghtModifiersList();

        _ = debuffStrengthModifiers.Add(applierStatsContainer.DebuffStrenghtValue);
        FinalDebuffStrenghtValue = Utils.Average(debuffStrengthModifiers);
    }
}
