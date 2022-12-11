using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TargetManager : MonoBehaviour, ILoadable, IProgress, IDisableUntilGameLoaded {
    #region Singleton
    public static TargetManager Instance;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }

        PlayerAllies = new List<Character> {
            Capacity = 50
        };

        UntargetablePlayerAllies = new List<Character> {
            Capacity = 20
        };

        Enemies = new List<Character> {
            Capacity = 200
        };

        ActiveEnemies = new List<Character> {
            Capacity = 100
        };

        UntargetableEnemies = new List<Character> {
            Capacity = 20
        };

        Items = new List<GameObject> {
            Capacity = 100
        };

        eventManager = EventManager.Instance;
        eventManager.OnNpcSpawned += AddSpawnedNpcToTargetList;
        eventManager.OnNpcRespawn += AddSpawnedNpcToTargetList;
        eventManager.OnNpcActivated += AddActivatedNpcToActiveList;

        GameSceneManager.PostSceneLoadPhase.ExecuteSync(Initialize, this, ExecuteAmount.Always);
    }
    #endregion

    private Camera cam;
    private EventManager eventManager;

    //player
    public static GameObject Player { get; private set; }
    public static Player PlayerComponent { get; private set; }

    //interactable target
    public static Interactable CurrentPlayersInteractableTarget { get; private set; }

    //enemy target
    public static GameObject CurrentPlayersEnemyTarget { get; private set; }
    public static Character CurrentPlayersEnemyTargetCharacterComponent { get; private set; }

    //ally target
    public static GameObject CurrentPlayersAllyTarget { get; private set; }
    public static Character CurrentPlayersAllyTargetCharacterComponent { get; private set; }

    public static Vector3 CurrentPlayersTargetPoint { get; private set; }
    public static bool TargetPointValid { get; private set; }

    public static Vector3 CurrentPlayersWalkableTargetPoint { get; private set; }

    [SerializeField, Range(0, 0.5f)]
    private float sphereCastThickness = 0.25f;
    [SerializeField] private LayerMask playerTargetsMask;
    [SerializeField] private LayerMask groundHitMask;


    public List<Character> PlayerAllies { get; private set; }
    public List<Character> UntargetablePlayerAllies { get; private set; }
    public List<Character> Enemies { get; private set; }
    public List<Character> ActiveEnemies { get; private set; }
    public List<Character> UntargetableEnemies { get; private set; }

    public List<GameObject> Items { get; private set; }

    private Interactable lastInteractableSelected;
    private Character lastEnemySelected;
    private Character lastAllySelected;


    [SerializeField, Range(0f, 1f)] private float playerTargetedChance = 0.5f;

    public bool IsLoaded { get; private set; } = false;
    public event Action<ILoadable> OnLoad;

    public bool ReportsProgress => false;
    public float Progress { get; private set; }

    private void Start() {
        
    }

    private void Initialize() {
        Player = GameObject.FindGameObjectWithTag("Player");
        PlayerComponent = Player.GetComponent<Player>();
        cam = Camera.main;

        IsLoaded = true;
        OnLoad?.Invoke(this);
    }

    private void Update() {
        if (Player == null) return;

        CurrentPlayersInteractableTarget = null;
        CurrentPlayersEnemyTarget = null;
        CurrentPlayersAllyTarget = null;
        CurrentPlayersTargetPoint = Vector3.zero;
        TargetPointValid = false;
        CurrentPlayersWalkableTargetPoint = Vector3.zero;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hit = Physics.SphereCastAll(ray, sphereCastThickness, 100, playerTargetsMask);
        if (hit.Length != 0) {
            for (int i = 0; i < hit.Length; i++) {
                if (CurrentPlayersInteractableTarget == null && hit[i].collider.CompareTag("Interactable")) {
                    CurrentPlayersInteractableTarget = hit[i].collider.gameObject.GetComponent<Interactable>();
                } else if (CurrentPlayersEnemyTarget == null && hit[i].collider.CompareTag("Enemy")) {
                    CurrentPlayersEnemyTarget = hit[i].collider.gameObject;
                    CurrentPlayersEnemyTargetCharacterComponent = CurrentPlayersEnemyTarget.GetComponent<Character>();
                } else if (CurrentPlayersAllyTarget == null && (hit[i].collider.CompareTag("PlayerAlly") || hit[i].collider.CompareTag("Player"))) {
                    CurrentPlayersAllyTarget = hit[i].collider.gameObject;
                    CurrentPlayersAllyTargetCharacterComponent = CurrentPlayersAllyTarget.GetComponent<Character>();
                } else {
                    break;
                }
            }
        }

#if UNITY_EDITOR
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.yellow);
#endif

        TargetPointValid = Physics.Raycast(ray, out RaycastHit hitGround, 100f, groundHitMask);
        if (TargetPointValid) {
            CurrentPlayersTargetPoint = hitGround.point;
        } else {
            CurrentPlayersTargetPoint = Vector3.zero;
        }


        if (CurrentPlayersEnemyTarget != null) {
            if (lastAllySelected != null) {
                lastAllySelected.OnDeselected();
                lastAllySelected = null;
            }

            if (lastEnemySelected != null && CurrentPlayersEnemyTarget != lastEnemySelected.gameObject) {
                lastEnemySelected.OnDeselected();
            }
            if (lastEnemySelected != CurrentPlayersEnemyTargetCharacterComponent) {
                lastEnemySelected = CurrentPlayersEnemyTargetCharacterComponent;
                lastEnemySelected.OnSelected();
            }
        } else if (CurrentPlayersEnemyTarget == null && lastEnemySelected != null) {
            lastEnemySelected.OnDeselected();
            lastEnemySelected = null;
        }

        if (CurrentPlayersEnemyTarget != null) {
            if (lastInteractableSelected != null) {
                lastInteractableSelected.OnDeselected();
                lastInteractableSelected = null;
            }
            return;
        }

        if (CurrentPlayersAllyTarget != null) {
            if (lastAllySelected != null && CurrentPlayersAllyTarget != lastAllySelected.gameObject) {
                lastAllySelected.OnDeselected();
            }
            if (lastAllySelected != CurrentPlayersAllyTargetCharacterComponent) {
                lastAllySelected = CurrentPlayersAllyTargetCharacterComponent;
                lastAllySelected.OnSelected();
            }
        } else if (CurrentPlayersEnemyTarget == null && lastAllySelected != null) {
            lastAllySelected.OnDeselected();
            lastAllySelected = null;
        }

        if (CurrentPlayersAllyTarget != null && !CurrentPlayersAllyTarget.CompareTag("Player")) {
            if (lastInteractableSelected != null) {
                lastInteractableSelected.OnDeselected();
                lastInteractableSelected = null;
            }
            return;
        }

        if (CurrentPlayersInteractableTarget != null) {
            if (lastInteractableSelected != null && CurrentPlayersInteractableTarget != lastInteractableSelected) {
                lastInteractableSelected.OnDeselected();
            }
            if (lastInteractableSelected != CurrentPlayersInteractableTarget) {
                lastInteractableSelected = CurrentPlayersInteractableTarget;
                lastInteractableSelected.OnSelected();
            }
        } else if (CurrentPlayersInteractableTarget == null && lastInteractableSelected != null) {
            lastInteractableSelected.OnDeselected();
            lastInteractableSelected = null;
        }
    }

    public void AddSpawnedNpcToTargetList(Character character) {
        if (character == null) return;

        if (character.CompareTag("Enemy")) {
            Enemies.Add(character);
            character.OnCharacterDeath += RemoveDeadEnemyFromTargetList;
            character.OnCharacterUntargetable += HandleEnemyToUntargetable;
        } else if (character.CompareTag("PlayerAlly")) {
            PlayerAllies.Add(character);
            character.OnCharacterDeath += RemoveDeadPlayerAllyFromTargetList;
            character.OnCharacterUntargetable += HandlePlayerAllyToUntargetable;
        }
    }

    public void AddActivatedNpcToActiveList(Character character, bool state) {
        if (character == null || character.CharacterStatusEffectsManager.IsUntargetable) return;

        if (state) {
            ActiveEnemies.Add(character);
        } else {
            ActiveEnemies.Remove(character);
        }
    }

    public void RemoveDeadEnemyFromTargetList(Character character) {
        if (character.CharacterStatusEffectsManager.IsUntargetable) {
            UntargetableEnemies.Remove(character);
        }

        Enemies.Remove(character);

        character.OnCharacterDeath -= RemoveDeadEnemyFromTargetList;
        character.OnCharacterUntargetable -= HandleEnemyToUntargetable;
    }

    public void RemoveDeadPlayerAllyFromTargetList(Character character) {
        if (character.CharacterStatusEffectsManager.IsUntargetable) {
            UntargetablePlayerAllies.Remove(character);
        } else {
            PlayerAllies.Remove(character);
        }
        character.OnCharacterDeath -= RemoveDeadPlayerAllyFromTargetList;
        character.OnCharacterUntargetable -= HandlePlayerAllyToUntargetable;
    }

    public void HandleEnemyToUntargetable(Character character, bool state) {
        if (state) {
            UntargetableEnemies.Add(character);
            if (character.IsActiveTarget) ActiveEnemies.Remove(character);
        } else {
            UntargetableEnemies.Remove(character);
            if (character.IsActiveTarget) ActiveEnemies.Add(character);
        }
    }

    public void HandlePlayerAllyToUntargetable(Character character, bool state) {
        if (state) {
            PlayerAllies.Remove(character);
            UntargetablePlayerAllies.Add(character);
        } else {
            UntargetablePlayerAllies.Remove(character);
            PlayerAllies.Add(character);
        }
    }

    public Transform AcquireTarget(string tag) {
        if (!IsLoaded) return null;

        if (tag == "Enemy") {
            bool targetPlayer = true;
            float rand = UnityEngine.Random.Range(0f, 1f);
            //Debug.Log(rand);
            if (rand > playerTargetedChance) {
                //Debug.Log("Selecting player ally target");
                if (PlayerAllies.Count > 0) {
                    targetPlayer = false;
                }
            }

            if (targetPlayer) {
                //Debug.Log("returning player target");
                return Player.transform;
            } else {
                //Debug.Log("returning player ally target");
                return PlayerAllies[UnityEngine.Random.Range(0, PlayerAllies.Count)].transform;
            }
        } else if (tag == "PlayerAlly") {
            if (ActiveEnemies.Count > 0) {
                return ActiveEnemies[UnityEngine.Random.Range(0, ActiveEnemies.Count)].transform;
            } else {
                return null;
            }
        }

        return null;
    }

    public Interactable TryGetPlayersInteractableTarget() {
        /*if (currentPlayersTarget == null) return null;
        if (currentPlayersTarget.CompareTag("Interactable")) return currentPlayersTarget;*/

        return CurrentPlayersInteractableTarget;
    }

    public GameObject TryGetPlayersEnemyTarget() {
        /*if (currentPlayersTarget == null) return null;
        if (currentPlayersTarget.CompareTag("Enemy")) return currentPlayersTarget;*/

        return CurrentPlayersEnemyTarget;
    }

    public GameObject TryGetPlayersPlayerOrPlayerAllyTarget() {
        /*if (currentPlayersTarget == null) return null;
        if (currentPlayersTarget.CompareTag("Player") || currentPlayersTarget.CompareTag("PlayerAlly")) return currentPlayersTarget;*/

        return CurrentPlayersAllyTarget;
    }

    public (bool pointValid, Vector3 point) TryGetPlayersCurrentTargetPoint() {
        return (TargetPointValid, CurrentPlayersTargetPoint);
    }

    public (bool pointValid, Vector3 point) TryGetPlayersCurrentWalkableTargetPoint() {
        if (!TargetPointValid) return (false, Vector3.zero);

        if (CurrentPlayersWalkableTargetPoint == Vector3.zero) {
            for (int i = 0; i < 10; i++) {
                if (NavMesh.SamplePosition(CurrentPlayersTargetPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas)) {
                    CurrentPlayersWalkableTargetPoint = hit.position;
                    break;
                }
            }
        }

        if (CurrentPlayersWalkableTargetPoint == Vector3.zero) {
            return (false, Vector3.zero);
        }

        return (true, CurrentPlayersWalkableTargetPoint);
    }
}
