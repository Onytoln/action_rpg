
[System.Serializable]
public class SkillStatIntContainer {
    public int Value { get; private set; }
    public int PrimaryValue { get; private set; }

    public SkillStatIntContainer(int value, int primaryValue) { 
        Value = value;
        PrimaryValue = primaryValue;
    }

    public SkillStatIntContainer(StatInt skillStatInt) {
        Value = skillStatInt.GetValue();
        PrimaryValue = skillStatInt.GetPrimaryValue();
    }
}
