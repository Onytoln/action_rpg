

//All game enums

#region Stat enums

public enum StatsCompareType { Offensive, Defensive, All }

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


