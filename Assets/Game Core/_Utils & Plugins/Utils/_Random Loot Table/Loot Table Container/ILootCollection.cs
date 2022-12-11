using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILootCollection<T> where T : LootTableTemplate
{
   void Add(T item);

   void Add(T item, params LootParameter[] lootParameters);
}
