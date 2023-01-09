using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HitOutput
{
    public HitInput hitInput;
    //if true, target was invulnerable to taking damage
    public bool wasInvulnerable;
    //if true then there is no hit
    public bool wasEvaded;
    //if is crit then == true
    public bool wasCrit;
    //if block happened then == true
    public bool wasBlock;

    public OutputByDamageType[] OutputByDamageType { get; set; }
    public float TotalPreReductionsDamage { get; set; }
    public float TotalDamageTakenPostReductions { get; set; }
    public float DamageBlocked { get; set; }
    public float TargetHealthRemaining { get; set; }

    public AbilityProperties HitSourceAbilityProperties => hitInput.CoreAbilityData.AbilityProperties;
    public AbilityPropertiesValuesContainer HitSourceAbilityPropertiesValuesContainer => hitInput.CoreAbilityData.AbilityPropertiesValuesContainer;
    public Character HitSourceCharacterComponent => hitInput.CoreAbilityData.CharacterComponent;
    public CharStatsValContainer HitSourceCoreStatsValues => hitInput.CoreAbilityData.CoreStatsValues;
    public Combat HitSourceCombat => hitInput.CoreAbilityData.Combat;
    public string CurrentHitInforId => hitInput.CurrentHitInfoId;
}
