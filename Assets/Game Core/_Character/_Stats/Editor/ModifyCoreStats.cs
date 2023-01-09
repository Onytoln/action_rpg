using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class ModifyCoreStats : EditorWindow
{
    int insertAfter = 0;
    string statName;
    public CharacterStatType statType = CharacterStatType.None;
    float primaryValue = 0;
    float minStatValue = 0;
    float maxStatValue = 0;
    float defaultUncappedMax = 0;

    [MenuItem("Custom Tools/Modify core stats")]
    static void Init() {
        UnityEditor.EditorWindow window = GetWindow(typeof(ModifyCoreStats));
        window.Show();
    }
    void OnGUI() {

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Insert stat after index: ");
        insertAfter = EditorGUILayout.IntField(insertAfter);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Stat name: ");
        statName = EditorGUILayout.TextField(statName);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Stat type: ");
        statType = (CharacterStatType)EditorGUILayout.EnumPopup(statType);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Stat primaryValue: ");
        primaryValue = EditorGUILayout.FloatField(primaryValue);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Min stat value: ");
        minStatValue = EditorGUILayout.FloatField(minStatValue);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Max stat value: ");
        maxStatValue = EditorGUILayout.FloatField(maxStatValue);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Uncapped default value: ");
        defaultUncappedMax = EditorGUILayout.FloatField(defaultUncappedMax);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Add Stat")) {
            AddStatToCoreStats(statName, insertAfter, statType, primaryValue, minStatValue, maxStatValue, defaultUncappedMax);
        }
    }

    public static void AddStatToCoreStats(string statName, int insertAfter, CharacterStatType statType, float primaryValue,
        float minStatValue, float maxStatValue, float defaultUncappedMax) {

        if (insertAfter < 0 || statType == CharacterStatType.None) return;

        List<StatValues> coreStats = Utils.FindAssetsByType<StatValues>();

        /*for (int i = 0; i < coreStats.Count; i++) {
            Stat[] newStats = new Stat[coreStats[i].stats.Length + 1];

            newStats[insertAfter + 1] = new Stat(statName, statType, primaryValue, minStatValue, maxStatValue, defaultUncappedMax);

            for (int j = 0; j < newStats.Length; j++) {
                if (j > insertAfter + 1) {
                    newStats[j] = coreStats[i].stats[j - 1];
                } else if(j < insertAfter + 1){
                    newStats[j] = coreStats[i].stats[j];
                }
            }

            coreStats[i].stats = newStats;

            for (int g = 0; g < newStats.Length; g++) {
                Debug.Log($"Stat type: { newStats[g].statType},  Stat entity: {i}");
            }
        }*/
    }
}
