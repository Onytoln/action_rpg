using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(StatValues))]
public class PreloadDefaultValues : Editor
{
    public CharacterStat[] defaultStats;

    private readonly CharacterStatType[] order = { CharacterStatType.Damage, CharacterStatType.AttackSpeed, CharacterStatType.CriticalStrike, CharacterStatType.CriticalDamage, CharacterStatType.DebuffStrength,
    CharacterStatType.MovementSpeed, CharacterStatType.Mana, CharacterStatType.ManaRegeneration, CharacterStatType.Health, CharacterStatType.HealthRegeneration,
    CharacterStatType.BlockChance, CharacterStatType.BlockStrength, CharacterStatType.EvasionChance,
    CharacterStatType.PhysicalResistance, CharacterStatType.FireResistance, CharacterStatType.IceResistance, CharacterStatType.LightningResistance, CharacterStatType.PoisonResistance, CharacterStatType.DebuffProtection,
    CharacterStatType.PhysicalPenetration, CharacterStatType.FirePenetration, CharacterStatType.IcePenetration, CharacterStatType.LightningPenetration, CharacterStatType.PoisonPenetration,
    CharacterStatType.LifeSteal, CharacterStatType.HealingEffectivity };
    
    /*public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Preload defaults to selected core stats")) {
            PreloadDefaults();
        }
    }

    public void PreloadDefaults() {
        string path = AssetDatabase.GetAssetPath(target);
        int index = path.LastIndexOf("/");
        if(index > 0) {
            path = path.Substring(0, index);
        }
        
        string[] folders = AssetDatabase.GetSubFolders(path);

        string[] assetGUIDs = AssetDatabase.FindAssets("t:Stat", folders);

        List<CharacterStat> stats = new List<CharacterStat>();

        for (int i = 0; i < assetGUIDs.Length; i++) {

            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);

            var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(CharacterStat));

            if (asset is CharacterStat stat) {
                stats.Add(stat);
            }
        }

        if (stats.Count != order.Length) {
            Debug.Log($"Not enough stats found in the subdirectories. Required {order.Length}");
            return;
        }

        CharacterStat[] resultOrdered = new CharacterStat[stats.Count];
        for (int i = 0; i < stats.Count; i++) {
            for (int j = 0; j < order.Length; j++) {
                if(stats[i].StatType == order[j]) {
                    resultOrdered[j] = stats[i];
                }
            }
        }

        (target as StatValues).SetStats(resultOrdered);
        EditorUtility.SetDirty(target);
    }*/
}
