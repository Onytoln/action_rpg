using MEC;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(UnityEngine.AI.NavMeshAgent), typeof(UnityEngine.AI.NavMeshObstacle))]
[RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody), typeof(NPCController))]
[RequireComponent(typeof(SummonStats), typeof(SummonCombat), typeof(StatusEffectsManager))]
public class Summon : Character {

    public NPCBehavior NPCBehavior { get; private set; }

    private SkillTemplate[] summonSkills;
    public SkillTemplate[] SummonSkills { get { if (summonSkills == null || summonSkills.Length == 0) { ProcessSkills(); } return summonSkills; } }

    public SummonStats SummonStats { get; private set; }

    public Character Summoner { get; private set; }

    public ISummon SummonProperties { get; private set; }

    private float deathTime;

    private CoroutineHandle deathTimer;

    [SerializeField] private Color outlinePlayerAllyColor = new Color32(124, 252, 0, 180);
    [SerializeField] private Color outlineEnemyColor = new Color32(255, 0, 0, 180);

    protected SummonMasterType summonMasterType;

    public override void Awake() {
        base.Awake();
        SummonStats = (SummonStats)CharacterStats;
        NPCBehavior = GetComponent<NPCBehavior>();
        respawnable = false;
    }

    public override void Start() {
        base.Start();
        EventManager.OnNpcSpawned?.Invoke(this);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.G)) ProcessRespawn();
    }

    public void ProcessSummon(Character summoner, StatValues summonerStats, ISummon summonProperties) {
        if (summonProperties == null || summonerStats == null || summonProperties == null) {
            Destroy(this);
            return;
        }

        Summoner = summoner;
        SummonProperties = summonProperties;

        if (LayerMask.LayerToName(Summoner.gameObject.layer) == DataStorage.PlayerLayerName) {
            summonMasterType = SummonMasterType.Player;

            tag = DataStorage.PlayerAllyTag;

            if (gameObject.layer != DataStorage.PlayerLayerIndex)
                gameObject.SetLayerRecursively(DataStorage.PlayerLayerIndex, true);

            hitLayerName = DataStorage.EnemyLayerName;
            hitLayerBitMask = DataStorage.EnemyLayerBitMask;
            abilityObjectHitLayerName = DataStorage.CollideWithEnemyLayerName;
            abilityObjectHitLayerBitMask = DataStorage.CollideWithEnemyLayerBitMask;

            if (CharacterOutline != null) {
                CharacterOutline.OutlineColor = outlinePlayerAllyColor;
            }

            gameObject.AddComponent<CombatTextPrinter>();

        } else if (LayerMask.LayerToName(Summoner.gameObject.layer) == DataStorage.EnemyLayerName) {
            summonMasterType = SummonMasterType.Enemy;

            tag = DataStorage.EnemyTag;

            if (gameObject.layer != DataStorage.EnemyLayerIndex)
                gameObject.SetLayerRecursively(DataStorage.EnemyLayerIndex, true);

            hitLayerName = DataStorage.PlayerLayerName;
            hitLayerBitMask = DataStorage.PlayerLayerBitMask;
            abilityObjectHitLayerName = DataStorage.CollideWithPlayerLayerName;
            abilityObjectHitLayerBitMask = DataStorage.CollideWithPlayerLayerBitMask;

            if (CharacterOutline != null) {
                CharacterOutline.OutlineColor = outlineEnemyColor;
            }

            if (TryGetComponent<CombatTextPrinter>(out CombatTextPrinter combatTextPrinter)) {
                Destroy(combatTextPrinter);
            }
        }

        SummonStats.SetSummonStats(summonerStats, summonProperties);

        Summoner.CharacterStats.OnDeathInternal += OnSummonerDeath;

        for (int i = 0; i < summonSkills.Length; i++) {
            summonSkills[i].skillProperties.SetUpListeners();
        }

        if (!summonProperties.PermanentSummon) {
            deathTime = SummonProperties.SummonDuration.GetValue();
            deathTimer = Timing.RunCoroutine(DeathTimer());
        }

        ActivateTarget();
    }

    private void OnSummonerDeath() {
        SummonStats.SetCurrentHealth(0);
    }

    private IEnumerator<float> DeathTimer() {
        while (deathTime > 0) {
            deathTime -= Time.deltaTime;
            yield return Timing.WaitForOneFrame;
        }

        SummonStats.SetCurrentHealth(0);
    }

    private void ProcessSkills() {
        summonSkills = GetComponents<SkillTemplate>();
    }

    public override SkillTemplate[] GetCharacterSkills() {
        return SummonSkills;
    }

    public override void ActivateTarget() {
        if (summonMasterType == SummonMasterType.Enemy) {
            if (CharacterStatusEffectsManager.IsDead) return;
            if (IsActiveTarget) return;
            IsActiveTarget = true;
            EventManager.OnNpcActivated?.Invoke(this, true);
        }

        EnableCharacter();
    }

    public override void DeactivateTarget() {
        if (summonMasterType == SummonMasterType.Enemy) {
            if (!IsActiveTarget) return;
            IsActiveTarget = false;
            EventManager.OnNpcActivated?.Invoke(this, false);
        }

        DisableCharacter();
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
        if (CharacterOutline != null) {
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
    }

    public override void ProcessRespawn() {
        //cannot be respawned
    }

    public override void SetHitLayer(string layerName) {
        hitLayerBitMask = 1 << LayerMask.NameToLayer(layerName);
        hitLayerName = layerName;
    }

    public override void SetObjectHitLayer(string layerName) {
        abilityObjectHitLayerBitMask = 1 << LayerMask.NameToLayer(layerName);
        abilityObjectHitLayerName = layerName;
    }
}
