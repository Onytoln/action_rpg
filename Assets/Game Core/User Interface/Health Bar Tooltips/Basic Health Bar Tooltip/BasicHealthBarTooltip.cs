using UnityEngine;
using UnityEngine.UI;
using MEC;

public class BasicHealthBarTooltip : MonoBehaviour
{
    public static BasicHealthBarTooltip Instance { get; private set; }
    private void Awake() {
        if(Instance == null) {
            Instance = this;
        }

        GameSceneManager.LatePostSceneLoadPhase.ExecuteSync(() => {
            objectPoolManager.PrePoolObjects("AdditionalInfoText", additionalInfoTextPrefab.gameObject, 6);
        }, null, ExecuteAmount.Always);
    }

    private const string Format = "N0";

    [SerializeField] private CanvasGroup canvasGroup;
    private CoroutineHandle fadeTooltipCoroutine;

    [SerializeField] private Text nameText;
    [SerializeField] private Text rankText;

    [SerializeField] private Text lvlText;
    [SerializeField] private Slider barSlider;
    [SerializeField] private Text healthText;

    [SerializeField] private Transform additionalInfoParent;
    [SerializeField] private Text additionalInfoTextPrefab;
    private Text[] instantiatedTextPrefabs;

    private Character characterComponent;

    private ObjectPoolManager objectPoolManager;

    public Text test;

    void Start() {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        EventManager.Instance.OnCharacterSelected += ShowHealthBarTooltip;
        EventManager.Instance.OnCharacterDeselected += HideHealthBarTooltip;
        objectPoolManager = ObjectPoolManager.Instance;
    }

    public void ShowHealthBarTooltip(Character character) {
        if (character == null) return;

        if(characterComponent != character) {
            characterComponent = character;

            nameText.text = characterComponent.CharacterName;
            
            if (LayerMask.LayerToName(characterComponent.gameObject.layer) == "Enemy") {
                rankText.text = characterComponent.CharacterRank.ToString() + "  -  " + "Enemy";
            } else {
                rankText.text = characterComponent.CharacterRank.ToString() + "  -  " + "Ally";
            }

            lvlText.text = characterComponent.CharacterLevel.ToString();
        }

        barSlider.value = characterComponent.CharacterStats.CurrentHealth / characterComponent.CharacterStats.CoreStats.HealthValue;
        healthText.text = $"{characterComponent.CharacterStats.CurrentHealth.ToString(Format)}/" +
            $"{characterComponent.CharacterStats.CoreStats.HealthValue.ToString(Format)}";

        characterComponent.CharacterStats.CurrentHealthChange += UpdateHealthBar;

        if (characterComponent.AdditionalInfo.Length != 0) {

            instantiatedTextPrefabs = new Text[characterComponent.AdditionalInfo.Length];

            for (int i = 0; i < characterComponent.AdditionalInfo.Length; i++) {
                instantiatedTextPrefabs[i] = objectPoolManager.GetPooledObject("AdditionalInfoText", additionalInfoTextPrefab.gameObject).GetComponent<Text>();
                instantiatedTextPrefabs[i].text = characterComponent.AdditionalInfo[i];
                instantiatedTextPrefabs[i].transform.SetParent(additionalInfoParent);
                instantiatedTextPrefabs[i].transform.localScale = Vector3.one;
                instantiatedTextPrefabs[i].gameObject.SetActive(true);
            }
        }

        fadeTooltipCoroutine = Utils.FadeCanvasGroup(canvasGroup, true, fadeTooltipCoroutine, 6);
    }

    public void UpdateHealthBar(float hp, CharacterStats stats) {
        barSlider.value = hp / characterComponent.CharacterStats.CoreStats.HealthValue;
        healthText.text = $"{characterComponent.CharacterStats.CurrentHealth.ToString(Format)}/" +
            $"{characterComponent.CharacterStats.CoreStats.HealthValue.ToString(Format)}";
    }

    public void HideHealthBarTooltip(Character character) {
        Timing.KillCoroutines(fadeTooltipCoroutine);
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        characterComponent.CharacterStats.CurrentHealthChange -= UpdateHealthBar;
        for (int i = 0; i < instantiatedTextPrefabs?.Length; i++) {
            objectPoolManager.PoolObjectBack(instantiatedTextPrefabs[i].gameObject.name, instantiatedTextPrefabs[i].gameObject);
        }
    }

}
