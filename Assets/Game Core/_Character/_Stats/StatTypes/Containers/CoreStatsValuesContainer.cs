[System.Serializable]
public class CoreStatsValuesContainer : ICoreCharacterStatsProvider {
    public StatInfo[] Stats { get; private set; }

    public CoreStatsValuesContainer(StatValues npcStats) {
        Stats = new StatInfo[npcStats.Stats.Length];

        CharacterStat[] stats = npcStats.Stats;

        for (int i = 0; i < stats.Length; i++) {
            Stats[i] = new StatInfo(stats[i]);
        }
    }

    #region Easy values access

    public float DamageValue => Stats[0].Value;
    public float AttackSpeedValue => Stats[1].Value;
    public float CriticalStrikeChanceValue => Stats[2].Value;
    public float CriticalDamageValue => Stats[3].Value;
    public float DebuffStrenghtValue => Stats[4].Value;
    public float MovementSpeedValue => Stats[5].Value;
    public float ManaValue => Stats[6].Value;
    public float ManaRegenerationValue => Stats[7].Value;
    public float HealthValue => Stats[8].Value;
    public float HealthRegenerationValue => Stats[9].Value;
    public float BlockChanceValue => Stats[10].Value;
    public float BlockStrenghtValue => Stats[11].Value;
    public float EvasionChanceValue => Stats[12].Value;
    public float ArmorValue => Stats[13].Value;
    public float FireResistanceValue => Stats[14].Value;
    public float IceResistanceValue => Stats[15].Value;
    public float LightningResistanceValue => Stats[16].Value;
    public float PoisonResistanceValue => Stats[17].Value;
    public float DebuffProtectionValue => Stats[18].Value;
    public float PhysicalPenetrationValue => Stats[19].Value;
    public float FirePenetrationValue => Stats[20].Value;
    public float IcePenetrationValue => Stats[21].Value;
    public float LightningPenetrationValue => Stats[22].Value;
    public float PoisonPenetrationValue => Stats[23].Value;
    public float LifeStealValue => Stats[24].Value;
    public float HealingEffectivityValue => Stats[25].Value;

    public ICoreCharacterStat DamageStat => Stats[0];
    public ICoreCharacterStat AttackSpeedStat => Stats[1];
    public ICoreCharacterStat CriticalStrikeChanceStat => Stats[2];
    public ICoreCharacterStat CriticalDamageStat => Stats[3];
    public ICoreCharacterStat DebuffStrenghtStat => Stats[4];
    public ICoreCharacterStat MovementSpeedStat => Stats[5];
    public ICoreCharacterStat ManaStat => Stats[6];
    public ICoreCharacterStat ManaRegenerationStat => Stats[7];
    public ICoreCharacterStat HealthStat => Stats[8];
    public ICoreCharacterStat HealthRegenerationStat => Stats[9];
    public ICoreCharacterStat BlockChanceStat => Stats[10];
    public ICoreCharacterStat BlockStrenghtStat => Stats[11];
    public ICoreCharacterStat EvasionChanceStat => Stats[12];
    public ICoreCharacterStat ArmorStat => Stats[13];
    public ICoreCharacterStat FireResistanceStat => Stats[14];
    public ICoreCharacterStat IceResistanceStat => Stats[15];
    public ICoreCharacterStat LightningResistanceStat => Stats[16];
    public ICoreCharacterStat PoisonResistanceStat => Stats[17];
    public ICoreCharacterStat DebuffProtectionStat => Stats[18];
    public ICoreCharacterStat PhysicalPenetrationStat => Stats[19];
    public ICoreCharacterStat FirePenetrationStat => Stats[20];
    public ICoreCharacterStat IcePenetrationStat => Stats[21];
    public ICoreCharacterStat LightningPenetrationStat => Stats[22];
    public ICoreCharacterStat PoisonPenetrationStat => Stats[23];
    public ICoreCharacterStat LifeStealStat => Stats[24];
    public ICoreCharacterStat HealingEffectivityStat => Stats[25];

    #endregion

    public float GetStatValue(StatType statType) {
        for (int i = 0; i < Stats.Length; i++) {
            if (Stats[i].StatType == statType) return Stats[i].Value;
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

    public (float reduction, float min) GetResistanceValueByDamageType(DamageType damageType) => damageType switch {
        DamageType.Physical => (ArmorValue, Stats[13].MinValue),
        DamageType.Fire => (FireResistanceValue, Stats[14].MinValue),
        DamageType.Ice => (IceResistanceValue, Stats[15].MinValue),
        DamageType.Lightning => (LightningResistanceValue, Stats[16].MinValue),
        DamageType.Poison => (PoisonResistanceValue, Stats[17].MinValue),
        DamageType.Magical => (ArmorValue * 0.5f, Stats[13].MinValue),
        _ => (0f, 0f),
    };

    public CoreStatsValuesContainer CompareStats(CoreStatsValuesContainer stats) {
        if (this == stats) return this;

        int statsHigher = 0;

        for (int i = 0; i < Stats.Length; i++) {
            if (stats.Stats[i].Value >= Stats[i].Value) statsHigher++;
        }

        return statsHigher > Stats.Length * 0.5f ? stats : this;
    }

    public CoreStatsValuesContainer CompareStatsByOffensiveValues(CoreStatsValuesContainer stats) {
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

    public CoreStatsValuesContainer CompareStatsDefensiveValues(CoreStatsValuesContainer stats) {
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

    public CoreStatsValuesContainer CompareStatsByStatType(CoreStatsValuesContainer stats, StatType statType) {
        if (this == stats) return this;

        bool sentStatHigher = false;

        for (int i = 0; i < Stats.Length; i++) {
            if (Stats[i].StatType == statType && stats.Stats[i].Value >= Stats[i].Value)
                sentStatHigher = true;
        }

        return sentStatHigher ? stats : this;
    }
}

[System.Serializable]
public class StatInfo : ICoreCharacterStat {

    public StatType StatType { get; private set; }

    public float Value { get; private set; }

    public float MinValue { get; private set; }

    public StatInfo(CharacterStat stat) {
        StatType = stat.StatType;
        Value = stat.Value;
        MinValue = stat.MinValue;
    }
}