using ICSharpCode.NRefactory.Ast;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterStatEnumExtension
{
    public static CharacterStatType ToCharacterStatType(this ScalableCharacterStatType scalableCharacterStatType) => scalableCharacterStatType switch {
        ScalableCharacterStatType.PhysicalResistance => CharacterStatType.PhysicalResistance,
        ScalableCharacterStatType.FireResistance => CharacterStatType.FireResistance,
        ScalableCharacterStatType.IceResistance => CharacterStatType.IceResistance,
        ScalableCharacterStatType.LightningResistance => CharacterStatType.LightningResistance,
        ScalableCharacterStatType.PoisonResistance => CharacterStatType.PoisonResistance,
        _ => throw new System.NotImplementedException()
    };
}
