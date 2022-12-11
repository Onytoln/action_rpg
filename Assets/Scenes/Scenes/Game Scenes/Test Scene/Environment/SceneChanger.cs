using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour   
{
    InventoryUI inventoryUI;
    Inventory inventory;
    EquipmentManager eManage;
    void OnTriggerEnter(Collider other){
        if (other.CompareTag("Player"))
            SceneManager.LoadScene(1);       
    }
}


