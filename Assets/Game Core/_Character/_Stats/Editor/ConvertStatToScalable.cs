using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ConvertStatToScalable : EditorWindow {
    int scaleValue = 0;
    float minScalableStatValue, maxScalableStatValue, defaultScalableUncappedMax = 0;
    string statName;
    public CharacterStatType statType = CharacterStatType.None;
    float primaryValue = 0;
    float minStatValue = 0;
    float maxStatValue = 0;
    float defaultUncappedMax = 0;

    [MenuItem("Custom Tools/Convert Stat to Scalable")]
    static void Init() {
        UnityEditor.EditorWindow window = GetWindow(typeof(ConvertStatToScalable));
        window.Show();
    }
    void OnGUI() {

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Stat base scale value: ");
        scaleValue = EditorGUILayout.IntField(scaleValue);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Min scalable stat value: ");
        minScalableStatValue = EditorGUILayout.FloatField(minScalableStatValue);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Max scalable stat value: ");
        maxScalableStatValue = EditorGUILayout.FloatField(maxScalableStatValue);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Default scalable uncap: ");
        defaultScalableUncappedMax = EditorGUILayout.FloatField(defaultScalableUncappedMax);
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

        if (GUILayout.Button("Convert Stat")) {
            Debug.Log("Converting");
            ConvertStat(scaleValue, minScalableStatValue, maxScalableStatValue, defaultScalableUncappedMax,
                statName, statType, primaryValue, minStatValue, maxStatValue, defaultUncappedMax);
        }
    }

    public static void ConvertStat(int scaleValue, float minScalableStatValue, float maxScalableStatValue, float defaultScalableUncappedMax,
        string statName, CharacterStatType statType, float primaryValue, float minStatValue, float maxStatValue, float defaultUncappedMax) { 

        /*
        if (scaleValue <= 0 || statType == StatType.NoType) return;

        List<NPCStats> coreStats = StaticUtils.FindAssetsByType<NPCStats>();

        Debug.Log("Converting " + coreStats.Count + " stats.");

        for (int i = 0; i < coreStats.Count; i++) {

            for (int j = 0; j < coreStats[i].stats.Length; j++) {
                if(coreStats[i].stats[j].statType == statType) {
                    coreStats[i].stats[j] = new ScalableStat(scaleValue, minScalableStatValue, maxScalableStatValue, defaultScalableUncappedMax,
                        statName, statType, primaryValue, minStatValue, maxStatValue, defaultUncappedMax);
                    break;
                }
            }
        }
        */
    }
}
