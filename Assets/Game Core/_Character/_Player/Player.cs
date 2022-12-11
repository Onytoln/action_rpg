using UnityEngine;
using MEC;
using System.Collections.Generic;


public class Player : Character {

    private PlayerSkillTemplate[] playerSkills;
    public PlayerSkillTemplate[] PlayerSkills { get { if (playerSkills == null || playerSkills.Length == 0) { ProcessSkills(); } return playerSkills; } }

    /*public Summon summonTest;

    [field: SerializeField] public SkillStatInt SummonsCount { get; set; }
    [field: SerializeField] public SkillStat SummonDuration { get; set; }
    [field: SerializeField] public bool PermanentSummon { get; set; } = false;
    [field: SerializeField] public SummonStatBoosts[] SummonStatBoosts { get; set; }*/

    public Transform testObjectToAnimate;
    public CoroutineHandle? h;

    public override void Awake() {
        base.Awake();
        hitLayerName = DataStorage.EnemyLayerName;
        hitLayerBitMask = DataStorage.EnemyLayerBitMask;
        abilityObjectHitLayerName = DataStorage.CollideWithEnemyLayerName;
        abilityObjectHitLayerBitMask = DataStorage.CollideWithEnemyLayerBitMask;

        if (gameObject.layer != DataStorage.PlayerLayerIndex) {
            Debug.LogError("Player layer was not set to Player, setting to Player now.");
            gameObject.SetLayerRecursively(DataStorage.PlayerLayerIndex, true);
        }

        ExperienceManager.Instance.OnPlayerLevelUp += ScalePlayerStats;
    }

    public override void Start() {
        //tbd
        /*SummonsCount.CalculateValue();
        SummonDuration.CalculateValue();*/
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.G)) ProcessRespawn();
        /*if (Input.GetKeyDown(KeyCode.Keypad5)) {
            Summon summon = Instantiate(summonTest, TargetManager.CurrentPlayersTargetPoint, Quaternion.identity);
            summon.ProcessSummon(this, CharacterStats.CoreStats, this);
        }*/

        if (Input.GetKeyDown(KeyCode.Keypad7)) {
            //h = Utils.Instance.JumpCharacterToLocation(CharacterNavMeshAgent, TargetManager.CurrentPlayersTargetPoint, 0.5f, testObjectToAnimate, h);
            //StaticUtils.RemoveMaterial(CharacterRenderer, _dissolveMaterial);
            //transform.rotation *= Quaternion.identity;
        }

        if (Input.GetKeyDown(KeyCode.Keypad1)) {
            if (CharacterStatusEffectsManager.IsFrozen) {
                CharacterStatusEffectsManager.SetIsFrozen(false);
            } else {
                CharacterStatusEffectsManager.SetIsFrozen(true);
            }
        }

        /*if (Input.GetKeyDown(KeyCode.Keypad2)) {
            CharacterAnimator.SetTrigger("StunTrigger");
        }*/
    }

    private void ScalePlayerStats(int level) {
        ApplyLevelStatModifier(level);
    }

    private void ProcessSkills() {
        playerSkills = GetComponentsInChildren<PlayerSkillTemplate>();
    }

    public override SkillTemplate[] GetCharacterSkills() {
        return PlayerSkills;
    }

    public override SkillTemplate GetSkillById(string id) {
        for (int i = 0; i < PlayerSkills.Length; i++) {
            if (playerSkills[i].skillProperties.abilityId.Equals(id)) return playerSkills[i];
        }

        return null;
    }

    public override SkillProperties GetSkillPropertiesById(string id) {
        for (int i = 0; i < PlayerSkills.Length; i++) {
            if (playerSkills[i].skillProperties.abilityId.Equals(id)) return playerSkills[i].skillProperties;
        }

        return null;
    }

    public override void OnSelected() {
        //
    }

    public override void OnDeselected() {
        //
    }

    public override void ProcessDeath() {
        respawnable = true;
        base.ProcessDeath();
        EventManager.Instance.OnPlayerDeath?.Invoke(this);
        CharacterNavMeshAgent.enabled = false;
        if(CharacterNavMeshObstacle != null) CharacterNavMeshObstacle.enabled = false;
    }
    
    public override void ProcessRespawn() {
        base.ProcessRespawn();
        EventManager.Instance.OnPlayerRespawn?.Invoke(this);
        CharacterNavMeshAgent.enabled = true;
        if (CharacterNavMeshObstacle != null) CharacterNavMeshObstacle.enabled = true;
    }
}
