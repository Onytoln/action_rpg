using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParameterizedLoot<T> where T : LootTableTemplate {

    public T LootTable { get; private set; }
    public List<LootParameter> LootParameters { get; private set; }

    public ParameterizedLoot(T lootTable) {
        LootTable = lootTable;
    }

    public ParameterizedLoot(T lootTable, params LootParameter[] lootParameters) {
        LootTable = lootTable;

        if (lootParameters == null || lootParameters.Length == 0) return;

        for (int i = 0; i < lootParameters.Length; i++) {
            AddParameter(lootParameters[i]);
        }
    }

    public void AddParameter(LootParameter lootParameter) {
        if (lootParameter == null) return;

        LootParameters ??= new List<LootParameter>();

        LootParameters.Add(lootParameter);
    }

}
