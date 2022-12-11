

//All game enums

#region Stat enums

public enum StatType {
    NoType = -1, Damage = 0, AttackSpeed = 1, CriticalStrike = 2, CriticalDamage = 3, DebuffStrength = 4,
    MovementSpeed = 5, Mana = 6, ManaRegeneration = 7, Health = 8, HealthRegeneration = 9,
    BlockChance = 10, BlockStrength = 11, EvasionChance = 12,
    Armor = 13, FireResistance = 14, IceResistance = 15, LightningResistance = 16, PoisonResistance = 17, DebuffProtection = 18,
    PhysicalPenetration = 19, FirePenetration = 20, IcePenetration = 21, LightningPenetration = 22, PoisonPenetration = 23,
    LifeSteal = 24, HealingEffectivity = 25
}

public enum StatAddType { Absolute, Relative, Total }

public enum StatsCompareType { Offensive, Defensive, All }

public enum CriticalStrikeBenefitType { CritChanceAbsolute = 0, CritChanceRelative = 1, CritDamageAbsolute = 2, CritDamageRelative = 3 }

public enum StatStringType { Absolute, Percentage, MovementSpeed, PerSecond, Seconds, Meters }

#endregion

public enum EquipmentSlot { None = -1, Head, Pauldrons, Chest, Hands, Legs, Boots, MainHand, OffHand, Ring, Amulet }

public enum ItemRarity { Common = 0, Uncommon = 1, Rare = 2, Legendary = 3, Unique = 4, Mythical = 5 }

public enum NpcState { Idle, FollowingTarget, Attacking, Interrupted, WalkingBackToSpawnPoint, FollowingMaster }

public enum CharacterRank { Common = 0, Guardian = 1, MiniBoss = 2, Boss = 3, Summon = 4, Player = 5 }

public enum DamageType { Physical, Fire, Ice, Lightning, Poison, Magical, Void }

#region Skill Enums

public enum SkillCastType { Cast, Channel, Instant }

public enum SkillCastSpeedScalingType { Scalable, FixedCastTime }

public enum SkillType { Melee, Ranged, AreaOfEffect, Cone, Movement, Special }

public enum SkillInterruptType { ByCastingAndMovement, ByMovement }

#endregion

public enum SlotStateChanged { ObjectAdded, ObjectRemoved }

public enum ProjectileFireType { InLine, Cone, Spread180, Spread360 }

public enum ActionSlotTriggerType { KeyDown, KeyHold }

public enum ItemClickOperation { LeftSingleClick, LeftDoubleClick, RightSingleClick, RightDoubleClick }

public enum ChargeReplenishmentType { OneByOne, AllAtOnce }

public enum SummonMasterType { Player, Enemy }

#region Status Effects enums

public enum BuffType { Heal, StatBoost, ManaRestore, DamageBoost, AttackSpeedBoost, CritChanceBoost, HealOverTime, ManaRestoreOverTime, MovementSpeedBoost,
    MagicMissileSkillBuff, ProjectileBooster }

public enum DebuffType { DamageOverTime, FireDamageOverTime, IceDamageOverTime, LightningDamageOverTime, PoisonDamageOverTime, PureDamageOverTime, PhysicalDamageOverTime,
    StatDecrement, Slow, Stun, Frost
}

public enum SEApplicationStringStyle { None, OnHit, OnHitTwice, OnCastEnd }

#endregion

#region Loot enums

public enum LootType { Currency, RegularEquipment, UniqueEquipment, Consumables, Other }

#endregion


