using System;
using UnityEngine;

public class PlayerStats : CharacterStats {

    public override void Start() {
        base.Start();
        StatusEffectsManager.SetCanRegenerate(true);
        RunHealthRegenerationCoroutine();
        RunManaRegenerationCoroutine();

        EquipmentManager.Instance.OnEquipmentChanged += OnEquipmentChanged;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            AddRelativeStat(StatType.MovementSpeed, 0.3f, 0);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            RemoveRelativeStat(StatType.MovementSpeed, 0.30f);
        }

        if (Input.GetKeyDown(KeyCode.H)) {
            _ = TakeDamage(15, DamageType.Fire, 0.1f);
            
            /*GameObject gm = Instantiate(itemPrefab, transform.position + new Vector3(0,0.3f,0), transform.rotation);
            gm.GetComponent<ItemPickUp>().item = hpPot.GetCopy();
            gm.GetComponent<ItemPickUp>().tooltip = tooltip;*/
        }
    }

    void OnEquipmentChanged(Equipment newItem, Equipment oldItem) {
        if (newItem != null) {
            for (int i = 0; i < newItem.ActiveBaseStats.Count; i++) {
                AddAbsoluteStat(newItem.ActiveBaseStats[i].GetStatType(), newItem.ActiveBaseStats[i].GetBaseValue(), 0);
                //Debug.Log("Added absolute stat from " + newItem.name + ": " + newItem.activeBaseStats[i].GetBaseValue() + newItem.activeBaseStats[i].GetStatType().ToString());
            }

            for (int i = 0; i < newItem.ActiveEnchantments.Count; i++) {
                if(newItem.ActiveEnchantments[i] is BaseStatEnchantment bs) {
                    AddAbsoluteStat(bs.GetStatType(), bs.GetBaseValue(), 0);
                } 
            }
        }

        if (oldItem != null) {
            for (int i = 0; i < oldItem.ActiveBaseStats.Count; i++) {
                RemoveAbsoluteStat(oldItem.ActiveBaseStats[i].GetStatType(), oldItem.ActiveBaseStats[i].GetBaseValue());
                //Debug.Log("Remove absolute stat from " + oldItem.name + ": " + oldItem.activeBaseStats[i].GetBaseValue() + oldItem.activeBaseStats[i].GetStatType().ToString());
            }

            for (int i = 0; i < oldItem.ActiveEnchantments.Count; i++) {
                if (oldItem.ActiveEnchantments[i] is BaseStatEnchantment bs) {
                    RemoveAbsoluteStat(bs.GetStatType(), bs.GetBaseValue());
                }
            }
        }
    }

   
}
