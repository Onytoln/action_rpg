using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ModifyCoreStatUncaps : EditorWindow
{
    float[] coreStatUncaps = new float[26];

    //[MenuItem("Custom Tools/Modify core stat uncaps")]
    static void Init() {
        UnityEditor.EditorWindow window = GetWindow(typeof(ModifyCoreStatUncaps));
        window.Show();
    }

    private void OnGUI() {
        List<NPCStats> coreStats = Utils.FindAssetsByType<NPCStats>();

        if (coreStats.Count < 1) return;

        if(coreStatUncaps.Length != coreStats[0].Stats.Length) {
            coreStatUncaps = new float[coreStats[0].Stats.Length];
        }

        for (int i = 0; i < coreStats[0].Stats.Length; i++) {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(coreStats[0].Stats[i].statType.ToString());
            coreStatUncaps[i] = EditorGUILayout.FloatField(coreStatUncaps[i]);
            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Modify core stats uncaps")) {
            ModifyCoreStasUncapValue(coreStatUncaps);
        }
    }
    public static void ModifyCoreStasUncapValue(float[] uncapValues) {

        List<NPCStats> coreStats = Utils.FindAssetsByType<NPCStats>();

        if (uncapValues.Length != coreStats[0].Stats.Length) return;

        /*for (int i = 0; i < coreStats.Count; i++) {
            for (int j = 0; j < coreStats[i].stats.Length; j++) {
                coreStats[i].stats[j].ModifyDefaultUncap(uncapValues[j]);
            }
        }*/
    }

}
