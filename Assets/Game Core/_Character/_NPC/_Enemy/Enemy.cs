using CustomOutline;
using UnityEngine;
using UnityEngine.AI;

//This class contains all enemy data
[RequireComponent(typeof(EnemyCombat), typeof(Animator), typeof(UnityEngine.AI.NavMeshAgent))]
[RequireComponent(typeof(UnityEngine.AI.NavMeshObstacle), typeof(CapsuleCollider), typeof(Rigidbody))]
[RequireComponent(typeof(Outline), typeof(CombatTextPrinter))]
[RequireComponent(typeof(EnemyStats), typeof(NPCController), typeof(StatusEffectsManager))]
public class Enemy : Character, IExperience, ILootDropper {

    public NPCBehavior NPCBehavior { get; private set; }

    private SkillTemplate[] enemySkills;
    public SkillTemplate[] EnemySkills { get { if (enemySkills == null || enemySkills.Length == 0) { ProcessSkills(); } return enemySkills; } }

    [field: SerializeField, Header("XP")] public int DefaultExperienceGain { get; private set; }
    [field: SerializeField, Header("Loot")] public bool CanGiveExperience { get; private set; } = true;


    #region Loot Drop
    [field: SerializeField] public int MinItemsToDrop { get; private set; } = -1;
    [field: SerializeField] public int GuaranteedCurrencyLoot { get; private set; } = 0;
    [field: SerializeField] public int GuaranteedRegularEquipmentLoot { get; private set; } = 0;
    [field: SerializeField] public int GuaranteedUniqueEquipmentLoot { get; private set; } = 0;
    [field: SerializeField] public int GuaranteedConsumablesLoot { get; private set; } = 0;
    [field: SerializeField] public int GuaranteedOtherLoot { get; private set; } = 0;
    [field: SerializeField] public MultipleItemLootTable[] PersonalLoot { get; private set; }
    [field: SerializeField] public float ChanceToDropLoot { get; private set; }
    [field: SerializeField] public bool CanDropLoot { get; private set; }
    #endregion

    public override void Awake() {
        base.Awake();
        gameObject.tag = "Enemy";

        hitLayerName = DataStorage.PlayerLayerName;
        hitLayerBitMask = DataStorage.PlayerLayerBitMask;
        abilityObjectHitLayerName = DataStorage.CollideWithPlayerLayerName;
        abilityObjectHitLayerBitMask = DataStorage.CollideWithPlayerLayerBitMask;

        if (gameObject.layer != DataStorage.EnemyLayerIndex) {
            Debug.LogError("Enemy layer was not set to Enemy, setting to Enemy now.");
            gameObject.SetLayerRecursively(DataStorage.EnemyLayerIndex, true);
        }

        NPCBehavior = GetComponent<NPCBehavior>();

        CanGiveExperience = true;
        CanDropLoot = true;

        if (PersonalLoot != null) {
            for (int i = 0; i < PersonalLoot.Length; i++) {
                PersonalLoot[i].ItemLootTable = PersonalLoot[i].ItemLootTable.ToCumulative();
            }
        }
    }

    public override void OnValidate() {
        base.OnValidate();
        if (gameObject.layer != DataStorage.EnemyLayerIndex) {
            Utils.SetLayerRecursively(gameObject, DataStorage.EnemyLayerIndex, true);
        }

        animatorRespawnLayerIndex = 1;

        if (TryGetComponent<Animator>(out var animator)) {
            animator.cullingMode = AnimatorCullingMode.CullCompletely;
        }

        if (TryGetComponent<NavMeshAgent>(out var agent)) {
            agent.autoRepath = false;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
            agent.angularSpeed = 1000f;
            agent.acceleration = 1005f;
            agent.stoppingDistance = 1.5f;
        }

        if (TryGetComponent<NavMeshObstacle>(out var obstacle)) {
            obstacle.shape = NavMeshObstacleShape.Capsule;
            obstacle.carving = true;
            obstacle.carveOnlyStationary = false;
            obstacle.center = new Vector3(0f, 0.5f, 0f);
            obstacle.enabled = false;
        }

        if(TryGetComponent<Outline>(out var outline)) {
            outline.OutlineColor = DataStorage.DefaultEnemyOutlineColor;
            outline.OutlineWidth = DataStorage.DefaultEnemyOutlineWidth;
        }

        if(TryGetComponent<CombatTextPrinter>(out var cbtPrinter)) {
            cbtPrinter.PrintAttack = false;
            cbtPrinter.PrintAttacked = false;
            cbtPrinter.PrintHeal = true;
            cbtPrinter.PrintManaRestore = false;
        }
    }

    public override void Start() {
        base.Start();
        EventManager.OnNpcSpawned?.Invoke(this);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.G)) ProcessRespawn();
        if (Input.GetKeyDown(KeyCode.Keypad9)) {
            if (CharacterStatusEffectsManager.IsStunned) {
                CharacterStatusEffectsManager.SetIsStunned(false);
            } else {
                CharacterStatusEffectsManager.SetIsStunned(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.Keypad8)) {
            if (CharacterStatusEffectsManager.IsFrozen) {
                CharacterStatusEffectsManager.SetIsFrozen(false);
            } else {
                CharacterStatusEffectsManager.SetIsFrozen(true);
            }
        }
    }

    public int GetCurrentLevel() {
        return CharacterLevel;
    }

    protected override void LoadLvlData(ILoadable loadable) {
        ExperienceManager experienceManager = loadable as ExperienceManager;
        characterLevel = Random.Range(experienceManager.CurrentPlayerLevel - 1, experienceManager.CurrentPlayerLevel + 3);
        characterLevel = Mathf.Clamp(characterLevel, 1, experienceManager.GetMaxPossibleLevel());
        ApplyLevelStatModifier(characterLevel);
    }

    private void ProcessSkills() {
        enemySkills = GetComponents<SkillTemplate>();
    }

    public override SkillTemplate[] GetCharacterSkills() {
        return EnemySkills;
    }

    public override void ActivateTarget() {
        if (CharacterStatusEffectsManager.IsDead) return;
        if (IsActiveTarget) return;
        IsActiveTarget = true;
        EnableCharacter();
        EventManager.OnNpcActivated?.Invoke(this, true);
    }

    public override void DeactivateTarget() {
        if (!IsActiveTarget) return;
        IsActiveTarget = false;
        DisableCharacter();
        EventManager.OnNpcActivated?.Invoke(this, false);
    }

    public override void EnableCharacter() {
        if (CharacterStatusEffectsManager.IsDead) return;
        base.EnableCharacter();
        NPCBehavior.enabled = true;
        NPCBehavior.NpcController.enabled = true;
    }

    public override void DisableCharacter() {
        base.DisableCharacter();
        NPCBehavior.enabled = false;
        NPCBehavior.NpcController.enabled = false;
    }

    public override void OnSelected() {
        EventManager.OnCharacterSelected?.Invoke(this);
        if(CharacterOutline != null) {
            CharacterOutline.enabled = true;
        }
    }

    public override void OnDeselected() {
        EventManager.OnCharacterDeselected?.Invoke(this);
        if (CharacterOutline != null) {
            CharacterOutline.enabled = false;
        }
    }

    public override void ProcessDeath() {
        base.ProcessDeath();
        CharacterNavMeshAgent.enabled = false;
        CharacterNavMeshObstacle.enabled = false;
        NPCBehavior.NpcController.ForbidRotation();
        EventManager.OnNpcDeath?.Invoke(this);
        EventManager.OnExperienceSourceTriggered?.Invoke(this);
        EventManager.OnLootDropperTriggered?.Invoke(this);
        CanGiveExperience = false;
        CanDropLoot = false;
    }

    public override void ProcessRespawn() {
        base.ProcessRespawn();
        if (!respawnable) return;
        if (CharacterStatusEffectsManager.IsDead) {
            CharacterNavMeshAgent.enabled = true;
        }
    }

    public override void SetHitLayer(string layerName) {
        hitLayerBitMask = 1 << LayerMask.NameToLayer(layerName);
        hitLayerName = layerName;
    }

    public override void SetObjectHitLayer(string layerName) {
        abilityObjectHitLayerBitMask = 1 << LayerMask.NameToLayer(layerName);
        abilityObjectHitLayerName = layerName;
    }

    public int GetLootCategoryRank() {
        return (int)CharacterRank;
    }

    public Vector3 GetDropPosition() {
        return transform.position;
    }
}
