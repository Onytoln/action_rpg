using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour{
    #region
    public static EventManager Instance;

    private void Awake() {
        Instance = this;
        //Debug.Log("Event manager awoken");
    }
    #endregion


    public List<HitOutput> outputsFromHits;
    public List<HitOutput> hitsTaken;

    private void Start() {
        OnPlayerHit += (x) => outputsFromHits.Add(x);
        OnPlayerHitTaken += (x) => hitsTaken.Add(x);
        //onProjectilesFired = (x,y) => Debug.Log(x.Length + " " + y.name);
    }

    //TEST EVENTS
    public delegate void TestEvent(string text);
    public TestEvent testEvent1;
    // public event EventHandler testEv;
    public UnityEvent testUev;
    //---------------------------------------------------------------------------------------------------//

    //PLAYER COMBAT EVENTS (GLOBAL)

    //skill fire
    private EncapsulatedAction<SkillProperties> onPlayerSkillFireGlobal;
    public EncapsulatedAction<SkillProperties> OnPlayerSkillFire {
        get => onPlayerSkillFireGlobal;
        set {
            if (value == null) return;

            onPlayerSkillFireGlobal = value;
        }
    }

    //player hit
    private EncapsulatedAction<HitOutput> onPlayerHit;
    public EncapsulatedAction<HitOutput> OnPlayerHit {
        get => onPlayerHit;
        set {
            if (value == null) return;

            onPlayerHit = value;
        }
    }

    //player non-hit
    private EncapsulatedAction<HitOutput> onPlayerNonHitDamageDone;
    public EncapsulatedAction<HitOutput> OnPlayerNonHitDamageDone {
        get => onPlayerNonHitDamageDone;
        set {
            if (value == null) return;

            onPlayerNonHitDamageDone = value;
        }
    }

    //player hit kill
    private EncapsulatedAction<HitOutput> onPlayerHitKill;
    public EncapsulatedAction<HitOutput> OnPlayerHitKill {
        get => onPlayerHitKill;
        set {
            if (value == null) return;

            onPlayerHitKill = value;
        }
    }

    //player kill
    private EncapsulatedAction<HitOutput> onPlayerKill;
    public EncapsulatedAction<HitOutput> OnPlayerKill {
        get => onPlayerKill;
        set {
            if (value == null) return;

            onPlayerKill = value;
        }
    }

    //player hit taken
    private EncapsulatedAction<HitOutput> onPlayerHitTaken;
    public EncapsulatedAction<HitOutput> OnPlayerHitTaken {
        get => onPlayerHitTaken;
        set {
            if (value == null) return;

            onPlayerHitTaken = value;
        }
    }

    //player non-hit taken
    private EncapsulatedAction<HitOutput> onPlayerNonHitDamageTaken;
    public EncapsulatedAction<HitOutput> OnPlayerNonHitDamageTaken {
        get => onPlayerNonHitDamageTaken;
        set {
            if (value == null) return;

            onPlayerNonHitDamageTaken = value;
        }
    }

    //player heal
    private EncapsulatedAction<float> onPlayerHeal;
    public EncapsulatedAction<float> OnPlayerHeal {
        get => onPlayerHeal;
        set {
            if (value == null) return;

            onPlayerHeal = value;
        }
    }

    //player mana restore
    private EncapsulatedAction<float> onPlayerManaRestored;
    public EncapsulatedAction<float> OnPlayerManaRestored {
        get => onPlayerManaRestored;
        set {
            if (value == null) return;

            onPlayerManaRestored = value;
        }
    }

    //player death
    private EncapsulatedAction<Player> onPlayerDeath;
    public EncapsulatedAction<Player> OnPlayerDeath {
        get => onPlayerDeath;
        set {
            if (value == null) return;

            onPlayerDeath = value;
        }
    }

    //player respawn
    private EncapsulatedAction<Player> onPlayerRespawn;
    public EncapsulatedAction<Player> OnPlayerRespawn {
        get => onPlayerRespawn;
        set {
            if (value == null) return;

            onPlayerRespawn = value;
        }
    }

    //player applied status effect
    private EncapsulatedAction<StatusEffect> onPlayerAppliedStatusEffect;
    public EncapsulatedAction<StatusEffect> OnPlayerAppliedStatusEffect {
        get => onPlayerAppliedStatusEffect;
        set {
            if (value == null) return;

            onPlayerAppliedStatusEffect = value;
        }
    }

    //status effect applied to player
    private EncapsulatedAction<StatusEffect> onAppliedStatusEffectToPlayer;
    public EncapsulatedAction<StatusEffect> OnAppliedStatusEffectToPlayer {
        get => onAppliedStatusEffectToPlayer;
        set {
            if (value == null) return;

            onAppliedStatusEffectToPlayer = value;
        }
    }
    //---------------------------------------------------------------------------------------------------//

    //GENERAL COMBAT EVENTS

    //projectiles fired
    private EncapsulatedAction<AbilityObject[], Character> onProjectilesFired;
    public EncapsulatedAction<AbilityObject[], Character> OnProjectilesFired {
        get => onProjectilesFired;
        set {
            if (value == null) return;

            onProjectilesFired = value;
        }
    }

    //npc spawned
    private EncapsulatedAction<Character> onNpcSpawned;
    public EncapsulatedAction<Character> OnNpcSpawned {
        get => onNpcSpawned;
        set {
            if (value == null) return;

            onNpcSpawned = value;
        }
    }

    //npc died
    private EncapsulatedAction<Character> onNpcDeath;
    public EncapsulatedAction<Character> OnNpcDeath {
        get => onNpcDeath;
        set {
            if (value == null) return;

            onNpcDeath = value;
        }
    }

    //npc respawned
    private EncapsulatedAction<Character> onNpcRespawn;
    public EncapsulatedAction<Character> OnNpcRespawn {
        get => onNpcRespawn;
        set {
            if (value == null) return;

            onNpcRespawn = value;
        }
    }

    //npc became untargetable
    private EncapsulatedAction<Character, bool> onNpcUntargetable;
    public EncapsulatedAction<Character, bool> OnNpcUntargetable {
        get => onNpcUntargetable;
        set {
            if (value == null) return;

            onNpcUntargetable = value;
        }
    }

    //npc activated
    private EncapsulatedAction<Character, bool> onNpcActivated;
    public EncapsulatedAction<Character, bool> OnNpcActivated {
        get => onNpcActivated;
        set {
            if (value == null) return;

            onNpcActivated = value;
        }
    }
    //---------------------------------------------------------------------------------------------------//

    //EXPERIENCE
    private EncapsulatedAction<IExperience> onExperienceSourceDeath;
    public EncapsulatedAction<IExperience> OnExperienceSourceTriggered {
        get => onExperienceSourceDeath;
        set {
            if (value == null) return;

            onExperienceSourceDeath = value;
        }
    }
    //---------------------------------------------------------------------------------------------------//

    //LOOT
    private EncapsulatedAction<ILootDropper> onLootDropperDeath;
    public EncapsulatedAction<ILootDropper> OnLootDropperTriggered {
        get => onLootDropperDeath;
        set {
            if (value == null) return;

            onLootDropperDeath = value;
        }
    }
    //---------------------------------------------------------------------------------------------------//
    //ITEM RELATED EVENTS
    private EncapsulatedAction<Equipment> onEquipmentPreBaseStatsRandomization;
    public EncapsulatedAction<Equipment> OnEquipmentPreBaseStatsRandomization {
        get => onEquipmentPreBaseStatsRandomization;
        set {
            if (value == null) return;

            onEquipmentPreBaseStatsRandomization = value;
        }
    }
    //---------------------------------------------------------------------------------------------------//

    //CHARACTER SELECTED/DESELETED EVENTS
    private EncapsulatedAction<Character> onCharacterSelected;
    public EncapsulatedAction<Character> OnCharacterSelected {
        get => onCharacterSelected;
        set {
            if (value == null) return;

            onCharacterSelected = value;
        }
    }

    private EncapsulatedAction<Character> onCharacterDeselected;
    public EncapsulatedAction<Character> OnCharacterDeselected {
        get => onCharacterDeselected;
        set {
            if (value == null) return;

            onCharacterDeselected = value;
        }
    }
    //---------------------------------------------------------------------------------------------------//
}
