using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectsUIManager : MonoBehaviour
{

    [SerializeField] GameObject buffSlotsSource;
    [SerializeField] GameObject debuffSlotsSource;

    private StatusEffectUISlot[] buffUISlots;
    private StatusEffectUISlot[] debuffUISlots;

    private StatusEffectsManager playerStatusEffectsManager;

    private void Awake() {
        buffUISlots = buffSlotsSource.GetComponentsInChildren<StatusEffectUISlot>();
        debuffUISlots = debuffSlotsSource.GetComponentsInChildren<StatusEffectUISlot>();
    }

    void Start()
    {
        LoadHandler.NotifyOnLoad(TargetManager.Instance, (loadable) => Initalize());
    }

    private void Initalize() {
        playerStatusEffectsManager = TargetManager.PlayerComponent.CharacterStatusEffectsManager;
        playerStatusEffectsManager.OnStatusEffectApplied += OnStatusEffectsStateChanged;
        playerStatusEffectsManager.OnStatusEffectEnd += OnStatusEffectsStateChanged;
    }

    void Update()
    {
        for (int i = 0; i < buffUISlots.Length; i++) {
            buffUISlots[i].UpdateSlot();
        }

        for (int i = 0; i < debuffUISlots.Length; i++) {
            debuffUISlots[i].UpdateSlot();
        }
    }

    private void OnStatusEffectsStateChanged(StatusEffect statusEffect) {
        if(statusEffect is Buff) {
            RefreshBuffList(statusEffect);
        } else {
            RefreshDebuffList(statusEffect);
        }
    }

    private void RefreshBuffList(StatusEffect statusEffect) {
        List<Buff> buffs = playerStatusEffectsManager.Buffs;

        int buffsCount = buffs.Count;

        for (int i = 0; i < buffUISlots.Length; i++) {
            if (i < buffsCount && buffs[i] != null) {
                buffUISlots[i].FillSlot(buffs[i], buffs[i] == statusEffect); 
            } else {
                buffUISlots[i].ClearSlot();
            }
        }
    }

    private void RefreshDebuffList(StatusEffect statusEffect) {
        List<Debuff> debuffs = playerStatusEffectsManager.Debuffs;
        
        int debuffsCount = debuffs.Count;
   
        for (int i = 0; i < debuffUISlots.Length; i++) {
            if (i < debuffsCount && debuffs[i] != null) {
                debuffUISlots[i].FillSlot(debuffs[i], debuffs[i] == statusEffect);
            } else {
                debuffUISlots[i].ClearSlot();
            }
        }
    }
}
