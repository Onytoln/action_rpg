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
        CreateTextDataAndSpawnCore(hitOutput.hitInput.TargetCombatComponent.transform.position, hitOutput);
    }

    private void AttackedByEnemy(HitOutput hitOutput) {
        CreateTextDataAndSpawnCore(transform.position, hitOutput)
            .SetParent(transform);
    }

    private CombatText CreateTextDataAndSpawnCore(Vector3 textSpawnPos, HitOutput hitOutput) {
        if (hitOutput.wasInvulnerable) {
            return CombatTextUtility.SpawnCombatText(textSpawnPos, "Invulnerable")
                .SetColor(DataStorage.DefaultStatusColor)
                .SetScale(defaultStatusScale);

        }

        if (hitOutput.wasEvaded) {
            return CombatTextUtility.SpawnCombatText(textSpawnPos, "Evaded")
               .SetColor(DataStorage.DefaultStatusColor)
               .SetScale(defaultStatusScale);
        }

        if (hitOutput.TotalDamageTakenPostReductions <= 0f) return null;

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

        return CombatTextUtility.SpawnCombatText(textSpawnPos, finalText.ToString())
            .SetColor(finalColor, finalColor2)
            .SetOutlineColor(finalOutline)
            .SetFontSize(finalFontSize)
            .Activate();
    }

    private void OnHeal(float healedVal) {
        if (healedVal == 0f) return;
        SpawnRestorationText(healedVal, DataStorage.DefaultHealColor);
    }

    private void OnManaRestore(float manaRestoredVal) {
        if (manaRestoredVal == 0f) return;
        SpawnRestorationText(manaRestoredVal, DataStorage.DefaultManaRegenColor);
    }

    private void SpawnRestorationText(float restoredValue, Color32 color) {
        CombatTextUtility.SpawnCombatText(transform.position + (Vector3.up * UnityEngine.Random.Range(0.6f, 1f)) + (0.35f * UnityEngine.Random.Range(-1f, 1f) * Vector3.left),
        $"+{restoredValue:N0}")
            .SetColor(color)
            .SetParent(transform)
            .SetDissapearTime(1.2f)
            .SetSpeed(1.5f)
            .SetDirection(new Vector3(0f, -0.3f, 0f))
            .SetFontSize(26)
            .SetScaleRate(0f)
            .SetSpeedDecrement(0.4f)
            .Activate();
    }
}
