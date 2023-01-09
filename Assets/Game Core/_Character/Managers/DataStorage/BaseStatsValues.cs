
[System.Serializable]
public class BaseStatsValues {
    public CharacterStatType statType;
    public float min;
    public float max;    
}

[System.Serializable]
public readonly struct MinMax {
    public readonly float min;
    public readonly float max;

    public MinMax(float min, float max) {
        this.min = min;
        this.max = max;
    }
}
