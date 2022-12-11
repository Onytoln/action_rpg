using System;
using System.Collections.Generic;
using UnityEngine;

public static class BasicMyEnumExtensions {

    public static string ItemCooldownCategoryToReadableString(this ItemCooldownCategory icc) {
        return icc switch {
            ItemCooldownCategory.HealingPotion => "Healing Potion",
            ItemCooldownCategory.ManaPotion => "Mana Potion",
            ItemCooldownCategory.None => "None",
            ItemCooldownCategory.ManaPotionOverTime => "Restorative Mana Potion",
            ItemCooldownCategory.HealingPotionOverTime => "Restorative Healing Potion",
            _ => ""
        };
    }

    public static string StatTypeToReadableString(this StatType st) {
        return st switch {
            StatType.AttackSpeed => "Attack speed",
            StatType.BlockChance => "Block chance",
            StatType.BlockStrength => "Block stength",
            StatType.CriticalDamage => "Critical damage",
            StatType.CriticalStrike => "Critical strike chance",
            StatType.DebuffProtection => "Debuff protection",
            StatType.DebuffStrength => "Debuff strength",
            StatType.EvasionChance => "Evasion chance",
            StatType.PhysicalPenetration => "Physical penetration",
            StatType.FirePenetration => "Fire penetration",
            StatType.IcePenetration => "Ice penetration",
            StatType.LightningPenetration => "Lightning penetration",
            StatType.PoisonPenetration => "Poison penetration",
            StatType.FireResistance => "Fire resistance",
            StatType.IceResistance => "Ice resistance",
            StatType.LightningResistance => "Lightning resistance",
            StatType.PoisonResistance => "Poison resistance",
            StatType.MovementSpeed => "Movement speed",
            StatType.ManaRegeneration => "Mana regeneration",
            StatType.HealthRegeneration => "Health regeneration",
            StatType.LifeSteal => "Life steal",
            StatType.HealingEffectivity => "Healing effectivity",
            _ => st.ToString()
        };
    }

    public static StatStringType StatTypeToStatStringType(this StatType st) {
        return st switch {
            StatType.AttackSpeed => StatStringType.Percentage,
            StatType.BlockChance => StatStringType.Percentage,
            StatType.BlockStrength => StatStringType.Percentage,
            StatType.CriticalDamage => StatStringType.Percentage,
            StatType.CriticalStrike => StatStringType.Percentage,
            StatType.DebuffProtection => StatStringType.Percentage,
            StatType.DebuffStrength => StatStringType.Percentage,
            StatType.EvasionChance => StatStringType.Percentage,
            StatType.FirePenetration => StatStringType.Percentage,
            StatType.IcePenetration => StatStringType.Percentage,
            StatType.LightningPenetration => StatStringType.Percentage,
            StatType.PoisonPenetration => StatStringType.Percentage,
            StatType.FireResistance => StatStringType.Percentage,
            StatType.IceResistance => StatStringType.Percentage,
            StatType.LightningResistance => StatStringType.Percentage,
            StatType.PoisonResistance => StatStringType.Percentage,
            StatType.MovementSpeed => StatStringType.MovementSpeed,
            StatType.ManaRegeneration => StatStringType.PerSecond,
            StatType.HealthRegeneration => StatStringType.PerSecond,
            StatType.LifeSteal => StatStringType.Percentage,
            StatType.HealingEffectivity => StatStringType.Percentage,
            _ => StatStringType.Absolute
        };
    }

    private static readonly Dictionary<CriticalStrikeBenefitType, string> critBenefitCustomStringDict = new Dictionary<CriticalStrikeBenefitType, string>() {
         {  CriticalStrikeBenefitType.CritChanceAbsolute, "Critical strike chance +{0} (absolute)" },
         {  CriticalStrikeBenefitType.CritChanceRelative, "Critical strike chance +{0} (relative)" },
         {  CriticalStrikeBenefitType.CritDamageAbsolute, "Critical strike chance +{0} (absolute)" },
         {  CriticalStrikeBenefitType.CritDamageRelative, "Critical strike chance +{0} (relative)" }
    };

    public static string CritBenefitToCustomizedString(this CriticalStrikeBenefitType cb) {
        return Utils.DictTryGetOrDefault(critBenefitCustomStringDict, cb, throwErrorIfNotPresent: true);
    }

    private static readonly HashSet<StatType> isPenetrationHashSet = new HashSet<StatType>() {
        StatType.PhysicalPenetration, StatType.FirePenetration, StatType.IcePenetration, StatType.LightningPenetration, StatType.PoisonPenetration
    };

    public static bool IsPenetration(this StatType statType) {
        return isPenetrationHashSet.Contains(statType);
    }

    public static List<StatType> GetPenetrationsList() {
        StatType[] statTypes = (StatType[])Enum.GetValues(typeof(StatType));

        int penetrations = 0;
        List<StatType> penetrationsResult = new List<StatType>();

        for (int i = 0; i < statTypes.Length; i++) {
            if (!statTypes[i].IsPenetration()) continue;

            penetrations++;
            penetrationsResult.Add(statTypes[i]);
        }

        return penetrationsResult;
    }

    public static Color32 ResistanceStatToColor32(this StatType st) {
        return st switch {
            StatType.FireResistance => DataStorage.FireResistanceColor,
            StatType.IceResistance => DataStorage.IceResistanceColor,
            StatType.LightningResistance => DataStorage.LightningResistanceColor,
            StatType.PoisonResistance => DataStorage.PoisonResistanceColor,
            _ => DataStorage.DefaultResistanceColor
        };
    }

    public static string ResistanceStatToColorRGB(this StatType st) {
        return st switch {
            StatType.FireResistance => ColorUtility.ToHtmlStringRGB(DataStorage.FireResistanceColor),
            StatType.IceResistance => ColorUtility.ToHtmlStringRGB(DataStorage.IceResistanceColor),
            StatType.LightningResistance => ColorUtility.ToHtmlStringRGB(DataStorage.LightningResistanceColor),
            StatType.PoisonResistance => ColorUtility.ToHtmlStringRGB(DataStorage.PoisonResistanceColor),
            _ => ColorUtility.ToHtmlStringRGB(DataStorage.DefaultResistanceColor)
        };
    }
}
