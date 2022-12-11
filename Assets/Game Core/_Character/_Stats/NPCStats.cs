using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Stats", menuName = "NPC/Stats")]
public class NPCStats : ScriptableObject, ICoreCharacterStatsProvider {
    public bool IsCopy { get; set; } = false;
    //!!!!!!!!!!!!!!NEVER RENAME THIS VARIABLE!!!!!!!!!!!!!!! - if it happened, set back to previous name immediately, otherwise set back to previous and restore values
    //from save file of scriptable object, if that's not possible then this is fucked lmao
    [field: SerializeField] public Stat[] Stats { get; private set; }

    public NPCStats GetCopy() {
        if (IsCopy) {
            return this;
        } else {
            NPCStats newNpcStats = Instantiate(this);
            
            newNpcStats.IsCopy = true;

            for (int i = 0; i < Stats.Length; i++) {
                newNpcStats.Stats[i] = this.Stats[i].GetCopy();
            }

            return newNpcStats;
        }
    }

    public void SetStats(Stat[] stats) => Stats = stats;

    public CoreStatsValuesContainer GetStatsValuesCopy() {
        return new CoreStatsValuesContainer(this);
    }

    private void OnDisable() {
        //Debug.Log("SO disabled");
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
        DamageType.Physical => (ArmorValue, Stats[13].GetMinPossibleValue()),
        DamageType.Fire => (FireResistanceValue, Stats[14].GetMinPossibleValue()),
        DamageType.Ice => (IceResistanceValue, Stats[15].GetMinPossibleValue()),
        DamageType.Lightning => (LightningResistanceValue, Stats[16].GetMinPossibleValue()),
        DamageType.Poison => (PoisonResistanceValue, Stats[17].GetMinPossibleValue()),
        DamageType.Magical => (ArmorValue * 0.5f, Stats[13].GetMinPossibleValue()),
        _ => (0f, 0f),
    };
    

    #region Easy values access

    public float DamageValue => Stats[0].GetValue();
    public float AttackSpeedValue => Stats[1].GetValue();
    public float CriticalStrikeChanceValue => Stats[2].GetValue();
    public float CriticalDamageValue => Stats[3].GetValue();
    public float DebuffStrenghtValue => Stats[4].GetValue();
    public float MovementSpeedValue => Stats[5].GetValue();
    public float ManaValue => Stats[6].GetValue();
    public float ManaRegenerationValue => Stats[7].GetValue();
    public float HealthValue => Stats[8].GetValue();
    public float HealthRegenerationValue => Stats[9].GetValue();
    public float BlockChanceValue => Stats[10].GetValue();
    public float BlockStrenghtValue => Stats[11].GetValue();
    public float EvasionChanceValue => Stats[12].GetValue();
    public float ArmorValue => Stats[13].GetValue();
    public float FireResistanceValue => Stats[14].GetValue();
    public float IceResistanceValue => Stats[15].GetValue();
    public float LightningResistanceValue => Stats[16].GetValue();
    public float PoisonResistanceValue => Stats[17].GetValue();
    public float DebuffProtectionValue => Stats[18].GetValue();
    public float PhysicalPenetrationValue => Stats[19].GetValue();
    public float FirePenetrationValue => Stats[20].GetValue();
    public float IcePenetrationValue => Stats[21].GetValue();
    public float LightningPenetrationValue => Stats[22].GetValue();
    public float PoisonPenetrationValue => Stats[23].GetValue();
    public float LifeStealValue => Stats[24].GetValue();
    public float HealingEffectivityValue => Stats[25].GetValue();



    #endregion

    #region ICoreCharacterStatsProvider
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
}
