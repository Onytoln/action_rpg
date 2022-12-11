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

    public static string StatValueToFormattedString(this float ibs, StatType st) {
        return st switch {
            StatType.Damage => ibs.ToString("N2"),
            StatType.AttackSpeed => string.Format("{0:0.00}%", ibs * 100),
            StatType.CriticalStrike => string.Format("{0:0.00}%", ibs * 100),
            StatType.CriticalDamage => string.Format("{0:0.00}%", ibs * 100),
            StatType.DebuffStrength => string.Format("{0:0.00}%", ibs * 100),
            StatType.MovementSpeed => string.Format("{0:0.00}m/s", ibs),
            StatType.Mana => ibs.ToString("N2"),
            StatType.ManaRegeneration => string.Format("{0:0.00}/s", ibs),
            StatType.Health => ibs.ToString("N2"),
            StatType.HealthRegeneration => string.Format("{0:0.00}/s", ibs),
            StatType.BlockChance => string.Format("{0:0.00}%", ibs * 100),
            StatType.BlockStrength => string.Format("{0:0.00}%", ibs * 100),            
            StatType.EvasionChance => string.Format("{0:0.00}%", ibs * 100),
            StatType.Armor => ibs.ToString("N2"),
            StatType.FireResistance => ibs.ToString("N2"),
            StatType.IceResistance => ibs.ToString("N2"),
            StatType.LightningResistance => ibs.ToString("N2"),
            StatType.PoisonResistance => ibs.ToString("N2"),
            StatType.DebuffProtection => string.Format("{0:0.00}%", ibs * 100),
            StatType.PhysicalPenetration => string.Format("{0:0.00}%", ibs * 100),
            StatType.FirePenetration => string.Format("{0:0.00}%", ibs * 100),
            StatType.IcePenetration => string.Format("{0:0.00}%", ibs * 100),
            StatType.LightningPenetration => string.Format("{0:0.00}%", ibs * 100),
            StatType.PoisonPenetration => string.Format("{0:0.00}%", ibs * 100),
            StatType.LifeSteal => string.Format("{0:0.00}%", ibs * 100),
            StatType.HealingEffectivity => string.Format("{0:0.00}%", ibs * 100),
            _ => ibs.ToString("N2")
        };
    }
}
