using System.Collections.Generic;
using UnityEngine;
using MEC;

public class TestSpeed : MonoBehaviour
{
    public GameObject[] enemies;

    void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Timing.RunCoroutine(IncreaseTheirSpeed(), "speedCorutine");
    }

    IEnumerator<float> IncreaseTheirSpeed() {
        for (int i = 0; i < enemies.Length; i++) {
            if (enemies[i] != null) {
                    enemies[i].GetComponent<EnemyStats>().AddRelativeStat(StatType.MovementSpeed, 0.3f, 0);
                    yield return Timing.WaitForSeconds(5f);
            }
        }
        Timing.KillCoroutines("speedCorutine");
    }
}
