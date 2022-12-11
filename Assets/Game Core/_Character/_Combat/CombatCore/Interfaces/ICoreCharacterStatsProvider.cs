using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICoreCharacterStatsProvider {

    float GetPenetrationValueByDamageType(DamageType damageType);

    public ICoreCharacterStat DamageStat { get; }
    public ICoreCharacterStat AttackSpeedStat { get; }
    public ICoreCharacterStat CriticalStrikeChanceStat { get; }
    public ICoreCharacterStat CriticalDamageStat { get; }
    public ICoreCharacterStat DebuffStrenghtStat { get; }
    public ICoreCharacterStat MovementSpeedStat { get; }
    public ICoreCharacterStat ManaStat { get; }
    public ICoreCharacterStat ManaRegenerationStat { get; }
    public ICoreCharacterStat HealthStat { get; }
    public ICoreCharacterStat HealthRegenerationStat { get; }
    public ICoreCharacterStat BlockChanceStat { get; }
    public ICoreCharacterStat BlockStrenghtStat { get; }
    public ICoreCharacterStat EvasionChanceStat { get; }
    public ICoreCharacterStat ArmorStat { get; }
    public ICoreCharacterStat FireResistanceStat { get; }
    public ICoreCharacterStat IceResistanceStat { get; }
    public ICoreCharacterStat LightningResistanceStat { get; }
    public ICoreCharacterStat PoisonResistanceStat { get; }
    public ICoreCharacterStat DebuffProtectionStat { get; }
    public ICoreCharacterStat PhysicalPenetrationStat { get; }
    public ICoreCharacterStat FirePenetrationStat { get; }
    public ICoreCharacterStat IcePenetrationStat { get; }
    public ICoreCharacterStat LightningPenetrationStat { get; }
    public ICoreCharacterStat PoisonPenetrationStat { get; }
    public ICoreCharacterStat LifeStealStat { get; }
    public ICoreCharacterStat HealingEffectivityStat { get; }
}
