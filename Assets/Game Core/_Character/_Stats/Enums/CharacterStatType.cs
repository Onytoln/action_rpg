using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterStatType {
    None = -1,
    Damage = 0,
    AttackSpeed = 1,
    CriticalStrike = 2,
    CriticalDamage = 3,
    DebuffStrength = 4,
    MovementSpeed = 5,
    Mana = 6,
    ManaRegeneration = 7,
    Health = 8,
    HealthRegeneration = 9,
    BlockChance = 10,
    BlockStrength = 11,
    EvasionChance = 12,
    PhysicalResistance = 13,
    FireResistance = 14,
    IceResistance = 15,
    LightningResistance = 16,
    PoisonResistance = 17,
    DebuffProtection = 18,
    PhysicalPenetration = 19,
    FirePenetration = 20,
    IcePenetration = 21,
    LightningPenetration = 22,
    PoisonPenetration = 23,
    LifeSteal = 24,
    HealingEffectivity = 25
}

public enum ScalableCharacterStatType {
    None = -1,
    PhysicalResistance, 
    FireResistance,
    IceResistance,
    LightningResistance,
    PoisonResistance
}