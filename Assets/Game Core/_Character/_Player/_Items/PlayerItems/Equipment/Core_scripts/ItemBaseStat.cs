using UnityEngine;

[System.Serializable]
public class ItemBaseStat {
    [SerializeField] public StatType baseStat;
    public bool isResistance;
    private float absoluteBaseValue;
    private float statPercantageValue;

    private float statMin;
    private float statMax;

    public float FinalDecrementalPercentage { get; private set; }

    [Range(0, 5)]
    [SerializeField] private float statMultiplier = 1;

    public void SetBaseValue(float value) {
        absoluteBaseValue = value;
    }

    public void SetPercentageValue(float value) {
        statPercantageValue = value;
    }

    public float GetBaseValue() {
        return absoluteBaseValue;
    }

    public float GetPercentageValue() {
        return statPercantageValue;
    }

    public void SetStatMultiplier(float value) {
        statMultiplier = value;
    }

    public float GetStatMultiplier() {
        return statMultiplier;
    }

    public void SetMinMax(float min, float max) {
        statMin = min;
        statMax = max;
    }

    public float GetMin() {
        return statMin;
    }

    public float GetMax() {
        return statMax;
    }

    public void SetFinalDecrementalMod(float value) {
        FinalDecrementalPercentage = value;
    }

    public StatType GetStatType() {
        return baseStat;
    }
}