using System;
using System.Collections.Generic;
using UnityEngine;

public class LootTableRandomizationTest : MonoBehaviour {
    [System.Serializable]
    public class LootTest {
        public int randomedValue;
        public int randomedCount;
        public string expectedDropChance;
        public string calculatedDropChance;
    }

    public LootTableTemplate[] weights;
    public float[] expectedDropChances;

    public LootTableTemplate[] randomizedWeights;
    public int totalRandomizedItems;
    public List<LootTest> randomizedWeightsCount;

    void Start() {
        expectedDropChances = new float[weights.Length];

        int weightsSum = 0;
        for (int i = 0; i < weights.Length; i++) {
            weightsSum += weights[i].DropWeight;
        }

        for (int i = 0; i < weights.Length; i++) {
            expectedDropChances[i] = (float)weights[i].DropWeight / weightsSum * 100f;
        }
    }

    private void Update() {
        if (Input.GetKey(KeyCode.Keypad3)) {
            RandomizeWeights();
        }
    }

    private void RandomizeWeights() {

        randomizedWeights = new LootTableTemplate[1000];
        for (int i = 0; i < 1000; i++) {
            randomizedWeights[i] = LootTableWorker.RandomizeLootTypeSingle(ref weights);

            totalRandomizedItems++;

            int index = randomizedWeightsCount.FindIndex(x => x.randomedValue == randomizedWeights[i].DropWeight);
            if (index != -1) {
                randomizedWeightsCount[index].randomedCount++;
                randomizedWeightsCount[index].calculatedDropChance = string.Format("{0:0.00} %", (float)randomizedWeightsCount[index].randomedCount / totalRandomizedItems * 100f);
            } else {
                randomizedWeightsCount.Add(
                    new LootTest() {
                        randomedValue = randomizedWeights[i].DropWeight,
                        randomedCount = 1,
                        expectedDropChance = string.Format("{0:0.00} %", expectedDropChances[Array.FindIndex(weights, x => x.DropWeight == randomizedWeights[i].DropWeight)]),
                        calculatedDropChance = string.Format("{0:0.00} %", (float)1 / totalRandomizedItems * 100f)
                    }
                );
            }


        }
    }
}
