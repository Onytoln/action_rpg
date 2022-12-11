
[System.Serializable]
public class SkillStatContainer {
    public float Value { get; private set; }
    public float PrimaryValue { get; private set; }

    public SkillStatContainer(float value, float primaryValue) {
        Value = value;
        PrimaryValue = primaryValue;
    }

    public SkillStatContainer(SkillStat skillStat) {
        Value = skillStat.GetValue();
        PrimaryValue = skillStat.GetPrimaryValue();
    }
}
