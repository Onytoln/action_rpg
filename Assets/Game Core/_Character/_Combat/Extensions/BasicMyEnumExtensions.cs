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

    public static string StatTypeToReadableString(this CharacterStatType st) {
        return st switch {
            CharacterStatType.AttackSpeed => "Attack speed",
            CharacterStatType.BlockChance => "Block chance",
            CharacterStatType.BlockStrength => "Block stength",
            CharacterStatType.CriticalDamage => "Critical damage",
            CharacterStatType.CriticalStrike => "Critical strike chance",
            CharacterStatType.DebuffProtection => "Debuff protection",
            CharacterStatType.DebuffStrength => "Debuff strength",
            CharacterStatType.EvasionChance => "Evasion chance",
            CharacterStatType.PhysicalPenetration => "Physical penetration",
            CharacterStatType.FirePenetration => "Fire penetration",
            CharacterStatType.IcePenetration => "Ice penetration",
            CharacterStatType.LightningPenetration => "Lightning penetration",
            CharacterStatType.PoisonPenetration => "Poison penetration",
            CharacterStatType.PhysicalResistance => "Physical resistance",
            CharacterStatType.FireResistance => "Fire resistance",
            CharacterStatType.IceResistance => "Ice resistance",
            CharacterStatType.LightningResistance => "Lightning resistance",
            CharacterStatType.PoisonResistance => "Poison resistance",
            CharacterStatType.MovementSpeed => "Movement speed",
            CharacterStatType.ManaRegeneration => "Mana regeneration",
            CharacterStatType.HealthRegeneration => "Health regeneration",
            CharacterStatType.LifeSteal => "Life steal",
            CharacterStatType.HealingEffectivity => "Healing effectivity",
            _ => st.ToString()
        };
    }

    public static StatStringType StatTypeToStatStringType(this CharacterStatType st) {
        return st switch {
            CharacterStatType.AttackSpeed => StatStringType.Percentage,
            CharacterStatType.BlockChance => StatStringType.Percentage,
            CharacterStatType.BlockStrength => StatStringType.Percentage,
            CharacterStatType.CriticalDamage => StatStringType.Percentage,
            CharacterStatType.CriticalStrike => StatStringType.Percentage,
            CharacterStatType.DebuffProtection => StatStringType.Percentage,
            CharacterStatType.DebuffStrength => StatStringType.Percentage,
            CharacterStatType.EvasionChance => StatStringType.Percentage,
            CharacterStatType.FirePenetration => StatStringType.Percentage,
            CharacterStatType.IcePenetration => StatStringType.Percentage,
            CharacterStatType.LightningPenetration => StatStringType.Percentage,
            CharacterStatType.PoisonPenetration => StatStringType.Percentage,
            CharacterStatType.FireResistance => StatStringType.Percentage,
            CharacterStatType.IceResistance => StatStringType.Percentage,
            CharacterStatType.LightningResistance => StatStringType.Percentage,
            CharacterStatType.PoisonResistance => StatStringType.Percentage,
            CharacterStatType.MovementSpeed => StatStringType.MovementSpeed,
            CharacterStatType.ManaRegeneration => StatStringType.PerSecond,
            CharacterStatType.HealthRegeneration => StatStringType.PerSecond,
            CharacterStatType.LifeSteal => StatStringType.Percentage,
            CharacterStatType.HealingEffectivity => StatStringType.Percentage,
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

    private static readonly HashSet<CharacterStatType> isPenetrationHashSet = new HashSet<CharacterStatType>() {
        CharacterStatType.PhysicalPenetration, CharacterStatType.FirePenetration, CharacterStatType.IcePenetration, CharacterStatType.LightningPenetration, CharacterStatType.PoisonPenetration
    };

    public static bool IsPenetration(this CharacterStatType statType) {
        return isPenetrationHashSet.Contains(statType);
    }

    public static List<CharacterStatType> GetPenetrationsList() {
        CharacterStatType[] statTypes = (CharacterStatType[])Enum.GetValues(typeof(CharacterStatType));

        int penetrations = 0;
        List<CharacterStatType> penetrationsResult = new List<CharacterStatType>();

        for (int i = 0; i < statTypes.Length; i++) {
            if (!statTypes[i].IsPenetration()) continue;

            penetrations++;
            penetrationsResult.Add(statTypes[i]);
        }

        return penetrationsResult;
    }

    public static Color32 ResistanceStatToColor32(this CharacterStatType st) {
        return st switch {
            CharacterStatType.FireResistance => DataStorage.FireResistanceColor,
            CharacterStatType.IceResistance => DataStorage.IceResistanceColor,
            CharacterStatType.LightningResistance => DataStorage.LightningResistanceColor,
            CharacterStatType.PoisonResistance => DataStorage.PoisonResistanceColor,
            _ => DataStorage.DefaultResistanceColor
        };
    }

    public static string ResistanceStatToColorRGB(this CharacterStatType st) {
        return st switch {
            CharacterStatType.FireResistance => ColorUtility.ToHtmlStringRGB(DataStorage.FireResistanceColor),
            CharacterStatType.IceResistance => ColorUtility.ToHtmlStringRGB(DataStorage.IceResistanceColor),
            CharacterStatType.LightningResistance => ColorUtility.ToHtmlStringRGB(DataStorage.LightningResistanceColor),
            CharacterStatType.PoisonResistance => ColorUtility.ToHtmlStringRGB(DataStorage.PoisonResistanceColor),
            _ => ColorUtility.ToHtmlStringRGB(DataStorage.DefaultResistanceColor)
        };
    }
}
