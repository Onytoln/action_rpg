using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootTableContainer<T> : ILootCollection<T> where T : LootTableTemplate {

    public List<ParameterizedLoot<T>> ParameterizedLoot { get; private set; }

    public LootTableContainer() {
        ParameterizedLoot = new List<ParameterizedLoot<T>>();
    }

    public void Add(T item) {
        ParameterizedLoot.Add(new ParameterizedLoot<T>(item));
    }

    public void Add(T item, params LootParameter[] lootParameters) {
        ParameterizedLoot.Add(new ParameterizedLoot<T>(item, lootParameters));
    }
}
