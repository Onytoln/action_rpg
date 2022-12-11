using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DamageTypeToColor 
{
    public static Color32 DamageTypeToColor32(this DamageType damageType) {
        return damageType switch {
            DamageType.Physical => DataStorage.PhysicalDamageColor,
            DamageType.Fire => DataStorage.FireDamageColor,
            DamageType.Ice => DataStorage.IceDamageColor,
            DamageType.Lightning => DataStorage.LightningDamageColor,
            DamageType.Poison => DataStorage.PoisonDamageColor,
            DamageType.Magical => DataStorage.MagicalDamageColor,
            DamageType.Void => DataStorage.VoidDamageColor,
            _ => DataStorage.DefaultDamageColor,
        };
    }

    public static string DamageTypeToColorRGB(this DamageType damageType) {
        return damageType switch {
            DamageType.Physical => ColorUtility.ToHtmlStringRGB(DataStorage.PhysicalDamageColor),
            DamageType.Fire => ColorUtility.ToHtmlStringRGB(DataStorage.FireDamageColor),
            DamageType.Ice => ColorUtility.ToHtmlStringRGB(DataStorage.IceDamageColor),
            DamageType.Lightning => ColorUtility.ToHtmlStringRGB(DataStorage.LightningDamageColor),
            DamageType.Poison => ColorUtility.ToHtmlStringRGB(DataStorage.PoisonDamageColor),
            DamageType.Magical => ColorUtility.ToHtmlStringRGB(DataStorage.MagicalDamageColor),
            DamageType.Void => ColorUtility.ToHtmlStringRGB(DataStorage.VoidDamageColor),
            _ => ColorUtility.ToHtmlStringRGB(DataStorage.DefaultDamageColor),
        };
    }
}
