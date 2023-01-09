using System.Text;
using UnityEngine;

public static class ItemExtensions { 
    public static void AppendCoreItemDetails(StringBuilder sb, Item item) {
        sb.Append($"<size=35><color=#{item.itemRarity.ItemRarityToColorRGB()}><b>").Append($"<align=center>{item.name}</align>").AppendLine("</b></color></size>");
        _ = sb.AppendLine();
        sb.AppendLine($"<size=18><color=#{item.itemRarity.ItemRarityToColorRGB()}>{item.itemRarity} </color>");
        if (item.stackAmountMax > 1) {
            sb.AppendLine($"Stack size: {item.StackSize}/{item.stackAmountMax}");
        }
        if(item.ItemCooldownCategory != ItemCooldownCategory.None) {
            sb.AppendLine($"Item cooldown category: {item.ItemCooldownCategory.ItemCooldownCategoryToReadableString()}");
            sb.AppendLine("This item shares cooldown with other items from the same cooldown category.");
        }
        sb.Append("</size>");
    }

    public static string StatValueToFormattedString(this float ibs, CharacterStatType st) {
        return st switch {
            CharacterStatType.Damage => ibs.ToString("N2"),
            CharacterStatType.AttackSpeed => string.Format("{0:0.00}%", ibs * 100),
            CharacterStatType.CriticalStrike => string.Format("{0:0.00}%", ibs * 100),
            CharacterStatType.CriticalDamage => string.Format("{0:0.00}%", ibs * 100),
            CharacterStatType.DebuffStrength => string.Format("{0:0.00}%", ibs * 100),
            CharacterStatType.MovementSpeed => string.Format("{0:0.00}m/s", ibs),
            CharacterStatType.Mana => ibs.ToString("N2"),
            CharacterStatType.ManaRegeneration => string.Format("{0:0.00}/s", ibs),
            CharacterStatType.Health => ibs.ToString("N2"),
            CharacterStatType.HealthRegeneration => string.Format("{0:0.00}/s", ibs),
            CharacterStatType.BlockChance => string.Format("{0:0.00}%", ibs * 100),
            CharacterStatType.BlockStrength => string.Format("{0:0.00}%", ibs * 100),            
            CharacterStatType.EvasionChance => string.Format("{0:0.00}%", ibs * 100),
            CharacterStatType.PhysicalResistance => ibs.ToString("N2"),
            CharacterStatType.FireResistance => ibs.ToString("N2"),
            CharacterStatType.IceResistance => ibs.ToString("N2"),
            CharacterStatType.LightningResistance => ibs.ToString("N2"),
            CharacterStatType.PoisonResistance => ibs.ToString("N2"),
            CharacterStatType.DebuffProtection => string.Format("{0:0.00}%", ibs * 100),
            CharacterStatType.PhysicalPenetration => string.Format("{0:0.00}%", ibs * 100),
            CharacterStatType.FirePenetration => string.Format("{0:0.00}%", ibs * 100),
            CharacterStatType.IcePenetration => string.Format("{0:0.00}%", ibs * 100),
            CharacterStatType.LightningPenetration => string.Format("{0:0.00}%", ibs * 100),
            CharacterStatType.PoisonPenetration => string.Format("{0:0.00}%", ibs * 100),
            CharacterStatType.LifeSteal => string.Format("{0:0.00}%", ibs * 100),
            CharacterStatType.HealingEffectivity => string.Format("{0:0.00}%", ibs * 100),
            _ => ibs.ToString("N2")
        };
    }
}
