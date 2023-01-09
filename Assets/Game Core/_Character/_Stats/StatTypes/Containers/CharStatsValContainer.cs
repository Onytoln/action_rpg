
public class CharStatsValContainer {

    private readonly StatInfo[] _stats;

    public CharStatsValContainer(ICharacterStatReadonly[] statValues) {
        _stats = new StatInfo[statValues.Length];

        for (int i = 0; i < statValues.Length; i++) {
            _stats[i] = new StatInfo(statValues[i]);
        }
    }

    #region Easy values access

    public float DamageValue => _stats[0].Value;
    public float AttackSpeedValue => _stats[1].Value;
    public float CriticalStrikeChanceValue => _stats[2].Value;
    public float CriticalDamageValue => _stats[3].Value;
    public float DebuffStrenghtValue => _stats[4].Value;
    public float MovementSpeedValue => _stats[5].Value;
    public float ManaValue => _stats[6].Value;
    public float ManaRegenerationValue => _stats[7].Value;
    public float HealthValue => _stats[8].Value;
    public float HealthRegenerationValue => _stats[9].Value;
    public float BlockChanceValue => _stats[10].Value;
    public float BlockStrenghtValue => _stats[11].Value;
    public float EvasionChanceValue => _stats[12].Value;
    public float ArmorValue => _stats[13].Value;
    public float FireResistanceValue => _stats[14].Value;
    public float IceResistanceValue => _stats[15].Value;
    public float LightningResistanceValue => _stats[16].Value;
    public float PoisonResistanceValue => _stats[17].Value;
    public float DebuffProtectionValue => _stats[18].Value;
    public float PhysicalPenetrationValue => _stats[19].Value;
    public float FirePenetrationValue => _stats[20].Value;
    public float IcePenetrationValue => _stats[21].Value;
    public float LightningPenetrationValue => _stats[22].Value;
    public float PoisonPenetrationValue => _stats[23].Value;
    public float LifeStealValue => _stats[24].Value;
    public float HealingEffectivityValue => _stats[25].Value;

    #endregion

    public float GetStatValue(CharacterStatType statType) {
        for (int i = 0; i < _stats.Length; i++) {
            if (_stats[i].StatType == statType) return _stats[i].Value;
        }

        return 0f;
    }

    public float GetPenetrationValueByDamageType(DamageType damageType) => damageType switch {
        DamageType.Physical => PhysicalPenetrationValue,
        DamageType.Fire => FirePenetrationValue,
        DamageType.Ice => IcePenetrationValue,
        DamageType.Lightning => LightningPenetrationValue,
        DamageType.Poison => PoisonPenetrationValue,
        DamageType.Magical => PhysicalPenetrationValue,
        _ => 0f,
    };

    public CharStatsValContainer CompareStats(CharStatsValContainer stats) {
        if (this == stats) return this;

        int statsHigher = 0;

        for (int i = 0; i < _stats.Length; i++) {
            if (stats._stats[i].Value >= _stats[i].Value) statsHigher++;
        }

        return statsHigher > _stats.Length * 0.5f ? stats : this;
    }

    public CharStatsValContainer CompareStatsByOffensiveValues(CharStatsValContainer stats) {
        if (this == stats) return this;

        int statsHigher = 0;

        if (stats.DamageValue >= DamageValue) statsHigher++;
        if (stats.AttackSpeedValue >= AttackSpeedValue) statsHigher++;
        if (stats.CriticalStrikeChanceValue >= CriticalStrikeChanceValue) statsHigher++;
        if (stats.CriticalDamageValue >= CriticalDamageValue) statsHigher++;
        if (stats.DebuffStrenghtValue >= DebuffStrenghtValue) statsHigher++;
        if (stats.PhysicalPenetrationValue >= PhysicalPenetrationValue) statsHigher++;
        if (stats.FirePenetrationValue >= FirePenetrationValue) statsHigher++;
        if (stats.IcePenetrationValue >= IcePenetrationValue) statsHigher++;
        if (stats.LightningPenetrationValue >= LightningPenetrationValue) statsHigher++;
        if (stats.PoisonPenetrationValue >= PoisonPenetrationValue) statsHigher++;

        return statsHigher > 10 * 0.5f ? stats : this;
    }

    public CharStatsValContainer CompareStatsDefensiveValues(CharStatsValContainer stats) {
        if (this == stats) return this;

        int statsHigher = 0;

        if (stats.HealthValue >= HealthValue) statsHigher++;
        if (stats.HealthRegenerationValue >= HealthRegenerationValue) statsHigher++;
        if (stats.BlockChanceValue >= BlockChanceValue) statsHigher++;
        if (stats.BlockStrenghtValue >= BlockStrenghtValue) statsHigher++;
        if (stats.EvasionChanceValue >= EvasionChanceValue) statsHigher++;
        if (stats.ArmorValue >= ArmorValue) statsHigher++;
        if (stats.FireResistanceValue >= FireResistanceValue) statsHigher++;
        if (stats.IceResistanceValue >= IceResistanceValue) statsHigher++;
        if (stats.LightningResistanceValue >= LightningResistanceValue) statsHigher++;
        if (stats.PoisonResistanceValue >= PoisonResistanceValue) statsHigher++;
        if (stats.DebuffProtectionValue >= DebuffProtectionValue) statsHigher++;

        return statsHigher > 11 * 0.5f ? stats : this;
    }

    public CharStatsValContainer CompareStatsByStatType(CharStatsValContainer stats, CharacterStatType statType) {
        if (this == stats) return this;

        bool sentStatHigher = false;

        for (int i = 0; i < _stats.Length; i++) {
            if (_stats[i].StatType == statType && stats._stats[i].Value >= _stats[i].Value) {
                sentStatHigher = true;
                break;
            }
        }

        return sentStatHigher ? stats : this;
    }
}

public class StatInfo {
    public readonly CharacterStatType StatType;
    public readonly float Value;

    public StatInfo(ICharacterStatReadonly characterStat) {
        StatType = characterStat.StatType;
        Value = characterStat.Value;
    }
}