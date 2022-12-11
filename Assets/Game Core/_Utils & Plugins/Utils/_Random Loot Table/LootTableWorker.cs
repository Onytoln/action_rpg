using System.Collections.Generic;
using UnityEngine;

public static class LootTableWorker {

    private const int IsCumulativeVal = -99999;

    #region To Cumulative
    /// <summary>
    /// Shifts loot table template array drop weights into cumulative sums
    /// MODIFIES POINTER OF SENT ARRAY TO POINT TO NEW ARRAY
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lootArray"></param>
    public static void LootTableTempleToCumulative<T>(ref T[] lootArray) where T : LootTableTemplate {

        if (lootArray == null || lootArray.Length == 0) return;

        //if loot table array has -1 on last index as drop weight value then the array is already cumulative
        if (lootArray[lootArray.Length - 1]?.DropWeight == IsCumulativeVal) return;

        if (ValidateNonCumulativeLootArray(lootArray)) {
            Debug.LogError("Loot Array sent to ToCumulative method contains null values or values that are equal or smaller than 0.");
            return;
        }

        T[] newLootArray = new T[lootArray.Length + 1];

        newLootArray[0] = Utils.CreateInstanceWithParams<T>(lootArray[0]);

        for (int i = 1; i < lootArray.Length; i++) {
            newLootArray[i] = Utils.CreateInstanceWithParams<T>(lootArray[i], newLootArray[i - 1]);
        }

        newLootArray[newLootArray.Length - 1] = Utils.CreateInstanceWithParams<T>(IsCumulativeVal);

        lootArray = newLootArray;
    }

    /// <summary>
    /// Return new array of loot table template with cumulative valeus
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lootArray"></param>
    /// <returns></returns>
    public static T[] ToCumulative<T>(this T[] lootArray) where T : LootTableTemplate {
        if (lootArray == null || lootArray.Length == 0) return null;

        //if loot table array has -1 on last index as drop weight value then the array is already cumulative
        if (lootArray[lootArray.Length - 1]?.DropWeight == IsCumulativeVal) return lootArray;

        if (ValidateNonCumulativeLootArray(lootArray)) {
            Debug.LogError("Loot Array sent to ToCumulative method contains null values or values that are equal or smaller than 0.");
            return null;
        }

        T[] newLootArray = new T[lootArray.Length + 1];

        newLootArray[0] = Utils.CreateInstanceWithParams<T>(lootArray[0]);

        for (int i = 1; i < lootArray.Length; i++) {
            newLootArray[i] = Utils.CreateInstanceWithParams<T>(lootArray[i], newLootArray[i - 1]);
        }

        newLootArray[newLootArray.Length - 1] = Utils.CreateInstanceWithParams<T>(IsCumulativeVal);

        return newLootArray;
    }

    /// <summary>
    /// Return new array of loot table template with cumulative valeus
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lootArray"></param>
    /// <returns></returns>
    public static T[] LootTableToCumulative<T>(T[] lootArray) where T : LootTableTemplate {
        if (lootArray == null || lootArray.Length == 0) return null;

        //if loot table array has -1 on last index as drop weight value then the array is already cumulative
        if (lootArray[lootArray.Length - 1]?.DropWeight == IsCumulativeVal) return lootArray;

        if (ValidateNonCumulativeLootArray(lootArray)) {
            Debug.LogError("Loot Array sent to ToCumulative method contains null values or values that are equal or smaller than 0.");
            return null;
        }

        T[] newLootArray = new T[lootArray.Length + 1];

        newLootArray[0] = Utils.CreateInstanceWithParams<T>(lootArray[0]);

        for (int i = 1; i < lootArray.Length; i++) {
            newLootArray[i] = Utils.CreateInstanceWithParams<T>(lootArray[i], newLootArray[i - 1]);
        }

        newLootArray[newLootArray.Length - 1] = Utils.CreateInstanceWithParams<T>(IsCumulativeVal);

        return newLootArray;
    }

    #endregion

    #region Randomization
    /// <summary>
    /// Returns single randomized item
    /// REPLACES YOUR ARRAY WITH ARRAY WITH CUMULATIVE VALUES OF DROP WEIGHT
    /// </summary>
    /// <returns></returns>
    public static T RandomizeLootTypeSingle<T>(ref T[] lootArray)
        where T : LootTableTemplate {

        LootTableTempleToCumulative(ref lootArray);

        if (lootArray == null || lootArray.Length < 2 || lootArray[lootArray.Length - 1].DropWeight != IsCumulativeVal) {
            Debug.LogError($"Loot array is either null or has lenght lower than 2 or last element in loot array has dropweight different than {IsCumulativeVal}.");
            return null;
        }
      
        int randomizedWeight = Random.Range(0, lootArray[lootArray.Length - 2].DropWeight + 1);

        //lenght -1 because last value in array is indentifier if the array is already filled with cumulative values
        for (int i = 0; i < lootArray.Length - 1; i++) {
            if (randomizedWeight <= lootArray[i].DropWeight) {
                return lootArray[i];
            }
        }

        Debug.LogError("No item was selected when randomizing. This should never happen.");

        return null;
    }

    /// <summary>
    /// Returns single randomized item and outs its index 
    /// REPLACES YOUR ARRAY WITH ARRAY WITH CUMULATIVE VALUES OF DROP WEIGHT
    /// </summary>
    /// <returns></returns>
    public static T RandomizeLootTypeSingle<T>(ref T[] lootArray, out int index)
        where T : LootTableTemplate {

        LootTableTempleToCumulative(ref lootArray);

        if (lootArray == null || lootArray.Length < 2 || lootArray[lootArray.Length - 1].DropWeight != IsCumulativeVal) {
            Debug.LogError($"Loot array is either null or has lenght lower than 2 or last element in loot array has dropweight different than {IsCumulativeVal}.");
            index = -1;
            return null;
        }

        int randomizedWeight = Random.Range(0, lootArray[lootArray.Length - 2].DropWeight + 1);

        //lenght -1 because last value in array is indentifier if the array is already filled with cumulative values
        for (int i = 0; i < lootArray.Length - 1; i++) {
            if (randomizedWeight <= lootArray[i].DropWeight) {
                index = i;
                return lootArray[i];
            }
        }

        Debug.LogError("No item was selected when randomizing. This should never happen.");

        index = -1;
        return null;
    }


    /// <summary>
    /// Adds selected count of randomized items into list that was provided
    /// REPLACES YOUR ARRAY WITH ARRAY WITH CUMULATIVE VALUES OF DROP WEIGHT
    /// </summary>
    /// <returns></returns>
    public static void RandomizeLootTypeSingle<T>(ref T[] lootArray, int count, List<T> lootList)
        where T : LootTableTemplate {

        LootTableTempleToCumulative(ref lootArray);

        if (lootArray == null || lootArray.Length < 2 || lootArray[lootArray.Length - 1].DropWeight != IsCumulativeVal) {
            Debug.LogError($"Loot array is either null or has lenght lower than 2 or last element in loot array has dropweight different than {IsCumulativeVal}.");
            return;
        }

        if (lootList == null) {
            Debug.LogError("Provided list for randomization of items is null.");
            return;
        }

        for (int i = 0; i < count; i++) {
            int randomizedWeight = Random.Range(0, lootArray[lootArray.Length - 2].DropWeight + 1);

            //lenght -1 because last value in array is indentifier if the array is already filled with cumulative values
            for (int j = 0; j < lootArray.Length - 1; j++) {
                if (randomizedWeight <= lootArray[j].DropWeight) {
                    lootList.Add(lootArray[j]);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Adds selected count of randomized items into loot collection that was provided
    /// MODIFIES POINTER OF SENT ARRAY TO POINT TO NEW ARRAY
    /// </summary>
    /// <returns></returns>
    public static void RandomizeLootTypeSingle<T>(ref T[] lootArray, int count, ILootCollection<T> lootCollection)
        where T : LootTableTemplate {

        LootTableTempleToCumulative(ref lootArray);

        if (lootArray == null || lootArray.Length < 2 || lootArray[lootArray.Length - 1].DropWeight != IsCumulativeVal) {
            Debug.LogError($"Loot array is either null or has lenght lower than 2 or last element in loot array has dropweight different than {IsCumulativeVal}.");
            return;
        }

        if (lootCollection == null) {
            Debug.LogError("Provided loot collection for randomization of items is null.");
            return;
        }

        for (int i = 0; i < count; i++) {
            int randomizedWeight = Random.Range(0, lootArray[lootArray.Length - 2].DropWeight + 1);

            //lenght -1 because last value in array is indentifier if the array is already filled with cumulative values
            for (int j = 0; j < lootArray.Length - 1; j++) {
                if (randomizedWeight <= lootArray[j].DropWeight) {
                    lootCollection.Add(lootArray[j]);
                    break;
                }
            }
        }
    }

    public static List<T> RandomizeLootTypeMultiple<T>(T[] lootTable) where T : MultipleLootTableTemplate {
        if (lootTable == null) return null;

        List<T> results = new List<T>();

        for (int i = 0; i < lootTable.Length; i++) {
            if (lootTable[i].DropChance >= Random.value) {
                results.Add(lootTable[i]);
            }
        }

        return results;
    }

    #endregion

    /// <summary>
    /// Accepts already cumulative lootTables
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lootTable"></param>
    /// <param name="index">Index of item to have its chance decreased.</param>
    /// <param name="decreaseByPercentage">Value greater than 0 and 1 at most.</param>
    public static void DecreaseChance<T>(ref T[] lootTable, int index, float decreaseByPercentage) where T : LootTableTemplate {
        if (lootTable == null
            || lootTable.Length == 0
            || index < 0
            || lootTable.Length - 2 < index
            || decreaseByPercentage <= 0f
            || decreaseByPercentage > 1f) {

            Debug.LogError("One of the parameters for DecreaseChance is wrong. Investigate!");
            return;
        }

        if (lootTable[lootTable.Length - 1]?.DropWeight != IsCumulativeVal) return;

        int decrementBy = (int)((lootTable[index]?.DropWeight - (index != 0 ? lootTable[index - 1]?.DropWeight : 0)) * (1f - decreaseByPercentage));

        if (decrementBy <= 0) {
            Debug.LogError("When decreasing chance there has been an error. Items in loot table could be null or decrement value is 0.");
            return;
        }

        for (int i = index; i < lootTable.Length - 1; i++) {
            lootTable[i].DropWeight -= decrementBy;
        }

        //Debug.Log("Chance decreased successfully.");
    }

    #region Helpers
    public static T[] CopyLootArray<T>(T[] lootArray) where T : LootTableTemplate {
        if (lootArray == null) return null;

        T[] newLootArray = new T[lootArray.Length];

        for (int i = 0; i < lootArray.Length; i++) {
            newLootArray[i] = (T)new LootTableTemplate(lootArray[i].DropWeight);
        }

        return newLootArray;
    }

    public static bool ValidateNonCumulativeLootArray<T>(T[] lootArray) where T : LootTableTemplate {
        if (lootArray == null) return true;

        for (int i = 0; i < lootArray.Length; i++) {
            bool lastIndexCheck = !(i == lootArray.Length - 1 && lootArray[i].DropWeight == IsCumulativeVal);

            if (lootArray[i] == null || (lootArray[i].DropWeight <= 0 && lastIndexCheck)) {
                return true;
            }
        }

        return false;
    }

    public static bool IsCumulative<T>(T[] lootArray) where T : LootTableTemplate {
        if (lootArray == null) return false;

        for (int i = 0; i < lootArray.Length - 1; i++) {
            if (lootArray[i].DropWeight > lootArray[i + 1].DropWeight) return false;
        }

        return true;
    }

    public static bool IsCumulativeQuickCheck<T>(T[] lootArray) where T : LootTableTemplate {
        if (lootArray == null || lootArray[lootArray.Length - 1]?.DropWeight != IsCumulativeVal) return false;

        return true;
    }
    #endregion
}
