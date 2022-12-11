using System.Text;
using UnityEngine;

public class CombatTextPrinter : MonoBehaviour {
    [SerializeField] private Character characterComponent;

    //[SerializeField] private Color32 selfDamageTakenColor = Color.red;

    [field: SerializeField, HideInInspector] public bool PrintAttack { get; set; } = true;
    [field: SerializeField, HideInInspector] public bool PrintAttacked { get; set; } = true;
    [field: SerializeField, HideInInspector] public bool PrintHeal { get; set; } = true;
    [field: SerializeField, HideInInspector] public bool PrintManaRestore { get; set; } = true;

    private Vector3 defaultStatusScale = new Vector3(0.06f, 0.06f, 0.06f);

    private CombatTextUtility combatTextUtility;
    public CombatTextUtility CombatTextUtility {
        get {
            if (combatTextUtility == null) {
                combatTextUtility = CombatTextUtility.Instance;
            }
            return combatTextUtility;
        }
    }

    void Start() {
        if (characterComponent == null) {
            characterComponent = GetComponent<Character>();
        }

        if (PrintAttack) {
            characterComponent.CharacterCombat.OnHitDone += AttackedEnemy;
            characterComponent.CharacterCombat.OnNonHitDamageDone += AttackedEnemy;
        }

        if (PrintAttacked) {
            characterComponent.CharacterCombat.OnHitTaken += AttackedByEnemy;
            characterComponent.CharacterCombat.OnNonHitDamageTaken += AttackedByEnemy;
        }

        if (PrintHeal) {
            characterComponent.CharacterCombat.OnHeal += OnHeal;
        }

        if (PrintManaRestore) {
            characterComponent.CharacterCombat.OnManaRestored += OnManaRestore;
        }
    }

    private void OnDestroy() {
        characterComponent.CharacterCombat.OnHitDone -= AttackedEnemy;
        characterComponent.CharacterCombat.OnHitTaken -= AttackedByEnemy;
        characterComponent.CharacterCombat.OnHitTaken += AttackedByEnemy;
        characterComponent.CharacterCombat.OnNonHitDamageTaken += AttackedByEnemy;
        characterComponent.CharacterCombat.OnHeal += OnHeal;
        characterComponent.CharacterCombat.OnManaRestored += OnManaRestore;
    }

    private void AttackedEnemy(HitOutput hitOutput) {
        Vector3 textSpawnPos = hitOutput.hitInput.TargetCombatComponent.transform.position;

        if (hitOutput.wasInvulnerable) {
            CombatTextUtility.SpawnCombatText(textSpawnPos, "Invulnerable", DataStorage.DefaultStatusColor, combatTextScale: defaultStatusScale);
            return;
        }

        if (hitOutput.wasEvaded) {
            CombatTextUtility.SpawnCombatText(textSpawnPos, "Evaded", DataStorage.DefaultStatusColor, combatTextScale: defaultStatusScale);
            return;
        }

        if (hitOutput.TotalDamageTakenPostReductions <= 0f) return;

        StringBuilder finalText = new StringBuilder();
        Color32 finalColor = DataStorage.MultipleDamageTypesColor1;
        Color32? finalColor2 = DataStorage.MultipleDamageTypesColor2;
        Color32? finalOutline = null;
        int finalFontSize = 0;

        if (hitOutput.wasBlock) {
            finalText.Append("\U0001F6E1");
            finalOutline = DataStorage.DefaultBlockColor;
        }

        finalText.Append(string.Format("{0:n0}", hitOutput.TotalDamageTakenPostReductions));

        if (hitOutput.OutputByDamageType.Length == 1) {
            finalColor = hitOutput.OutputByDamageType[0].damageType.DamageTypeToColor32();
            finalColor2 = null;
        }

        if (hitOutput.wasCrit) {
            finalFontSize = 48;
            finalText.Append("!");
        }

        CombatTextUtility.SpawnCombatText(textSpawnPos, finalText.ToString(), finalColor, finalOutline, null, finalFontSize, textColor2: finalColor2);
    }

    private void AttackedByEnemy(HitOutput hitOutput) {
        Vector3 textSpawnPos = transform.position;

        if (hitOutput.wasInvulnerable) {
            CombatTextUtility.SpawnCombatText(textSpawnPos, "Invulnerable", DataStorage.DefaultStatusColor, combatTextScale: defaultStatusScale);
            return;
        }

        if (hitOutput.wasEvaded) {
            CombatTextUtility.SpawnCombatText(textSpawnPos, "Evaded", DataStorage.DefaultStatusColor, combatTextScale: defaultStatusScale);
            return;
        }

        if (hitOutput.TotalDamageTakenPostReductions <= 0f) return;

        StringBuilder finalText = new StringBuilder();
        Color32 finalColor = DataStorage.MultipleDamageTypesColor1;
        Color32? finalColor2 = DataStorage.MultipleDamageTypesColor2;
        Color32? finalOutline = null;
        int finalFontSize = 0;

        if (hitOutput.wasBlock) {
            finalText.Append("\U0001F6E1");
            finalOutline = DataStorage.DefaultBlockColor;
        }

        finalText.Append(string.Format("{0:n0}", hitOutput.TotalDamageTakenPostReductions));

        if (hitOutput.OutputByDamageType.Length == 1) {
            finalColor = hitOutput.OutputByDamageType[0].damageType.DamageTypeToColor32();
            finalColor2 = null;
        }

        if (hitOutput.wasCrit) {
            finalFontSize = 48;
            finalText.Append("!");
        }

        CombatTextUtility.SpawnCombatText(textSpawnPos, finalText.ToString(), finalColor, finalOutline, transform, finalFontSize, textColor2: finalColor2);
    }

    private void OnHeal(float healedVal) {
        if (healedVal == 0f) return;

        CombatTextUtility.SpawnCombatText(transform.position + (Vector3.up * UnityEngine.Random.Range(0.6f, 1f)) + (0.35f * UnityEngine.Random.Range(-1f, 1f) * Vector3.left),
              $"+{healedVal:N0}", DataStorage.DefaultHealColor, followedTransform: transform,
            disappearTime: 1.2f, speed: 1.5f, direction: new Vector3(0f, -0.3f, 0f), fontSize: 26, scaleRate: -1f, speedDecrement: 0.4f);
    }

    private void OnManaRestore(float manaRestoredVal) {
        if (manaRestoredVal == 0f) return;

        CombatTextUtility.SpawnCombatText(transform.position + (Vector3.up * UnityEngine.Random.Range(0.6f, 1f)) + (0.35f * UnityEngine.Random.Range(-1f, 1f) * Vector3.left),
              $"+{manaRestoredVal:N0}", DataStorage.DefaultManaRegenColor, followedTransform: transform,
            disappearTime: 1.2f, speed: 1.5f, direction: new Vector3(0f, -0.3f, 0f), fontSize: 26, scaleRate: -1f, speedDecrement: 0.4f);
    }

}
