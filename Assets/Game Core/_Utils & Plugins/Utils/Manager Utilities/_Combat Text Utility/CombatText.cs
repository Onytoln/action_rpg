using System;
using TMPro;
using UnityEngine;

public class CombatText : MonoBehaviour {

    [SerializeField] private TextMeshPro textMeshPro;
    
    [SerializeField, Header("Size and transform following offset")] private Vector3 combatTextDefaultSize;
    [SerializeField] private Vector3 offset;
    [SerializeField, Header("Combat text core properties")] private float defaultDisappearTime = 1;
    [SerializeField] private float defaultSpeed = 10;
    [SerializeField] private float defaultSpeedDecrement = 2f;
    [SerializeField] private float defaultDirectionX = 0.8f;
    [SerializeField] private float defaultDirectionY = 1;
    [SerializeField] private float defaultScaleRate = 1;
    [SerializeField] private float defaultDisappearSpeed = 3;
    [SerializeField, Header("Font")] private float defaultFontSize = 36;
    [SerializeField] private Color32 defaultOutlineColor = Color.black;

    private Vector3 intialScaleVector;

    private Vector3 initialDirection;
    private Vector3 currentDirection;

    private float initialDisappearTime;
    private float currentDisappearTime;

    private float initialSpeed;

    private float currentSpeedDecrement;

    private float initialScaleRate;

    private Color textColor;

    public event Action<CombatText> OnTextEnd;

    private void Awake() {
        if (textMeshPro == null) {
            textMeshPro = GetComponent<TextMeshPro>();
        }
    }

    public void Initialize(string text) {
        _ = SetText(text);
        _ = SetColor(Color.white);
        _ = SetOutlineColor(defaultOutlineColor);
        _ = SetFontSize(defaultFontSize);
        _ = SetScale(combatTextDefaultSize);
        _ = SetDissapearTime(defaultDisappearTime);
        _ = SetSpeed(defaultSpeed);
        _ = SetSpeedDecrement(defaultSpeedDecrement);
        _ = SetScaleRate(defaultScaleRate);
        _ = SetDirection(new Vector3(UnityEngine.Random.Range(-defaultDirectionX, defaultDirectionX), defaultDirectionY, 0f));

        textMeshPro.sortingOrder = CombatTextUtility.combatTextSortOrder;
    }

    public CombatText SetText(string text) {
        textMeshPro.SetText(text);
        return this;
    }

    public CombatText SetColor(Color32 textColor) {
        textMeshPro.color = textColor;
        textMeshPro.enableVertexGradient = false;
        this.textColor = textMeshPro.color;
        return this;
    }

    public CombatText SetColor(Color32 textColor1, Color32 textColor2) {
        textMeshPro.ResetColor();
        textMeshPro.colorGradient = new VertexGradient(textColor1, textColor2, textColor1, textColor2);
        textMeshPro.enableVertexGradient = true;
        return this;
    }

    public CombatText SetColor(Color32 textColor1, Color32? textColor2) {
        if(textColor2 == null) {
            return SetColor(textColor1);
        } else {
            return SetColor(textColor1, (Color32)textColor2);
        }
    }

    public CombatText SetColor(Color32 textColor1, Color32 textColor2, Color32 textColor3, Color32 textColor4) {
        textMeshPro.ResetColor();
        textMeshPro.colorGradient = new VertexGradient(textColor1, textColor2, textColor3, textColor4);
        textMeshPro.enableVertexGradient = true;
        return this;
    }

    public CombatText SetOutlineColor(Color32 outlineColor) {
        textMeshPro.outlineColor = outlineColor;
        return this;
    }

    public CombatText SetOutlineColor(Color32? outlineColor) {
        if (outlineColor == null) return this;
        return SetOutlineColor((Color32)outlineColor);
    }

    public CombatText SetFontSize(float fontSize) {
        if (fontSize <= 0f) return this;

        textMeshPro.fontSize = fontSize;
        return this;
    }

    public CombatText SetScale(Vector3 scale) {
        transform.localScale = intialScaleVector = scale;
        return this;
    }

    public CombatText SetParent(Transform parent) {
        transform.SetParent(parent);
        return this;
    }

    public CombatText SetDissapearTime(float disappearTime) {
        if (disappearTime <= 0f) return this;

        currentDisappearTime = initialDisappearTime = disappearTime;
        return this;
    }

    public CombatText SetSpeed(float speed) {
        if(speed <= 0f) return this;

        currentDirection = (initialSpeed = speed) * initialDirection;
        return this;
    }

    public CombatText SetSpeedDecrement(float speedDecrement) {
        if(speedDecrement <= 0f) return this;

        currentSpeedDecrement = speedDecrement;
        return this;
    }

    public CombatText SetScaleRate(float scaleRate) {
        if (scaleRate > 0f) {
            initialScaleRate = scaleRate;
        } else {
            initialScaleRate = 0f;
        }

        return this;
    }

    public CombatText SetDirection(Vector3 direction) {
        currentDirection = (initialDirection = direction) * initialSpeed;
        return this;
    }

    public CombatText Activate() {
        gameObject.SetActive(true);
        return this;
    }

    public void UpdateCombatText() {
        float deltaTime = Time.deltaTime;

        transform.position += currentDirection * deltaTime;

        currentDirection -= currentSpeedDecrement * deltaTime * currentDirection;

        if (initialScaleRate > 0f) {
            if (currentDisappearTime > initialDisappearTime * 0.5f) {
                transform.localScale += initialScaleRate * deltaTime * intialScaleVector;
            } else {
                transform.localScale -= initialScaleRate * deltaTime * intialScaleVector;
            }
        }
        
        currentDisappearTime -= deltaTime;
        if(currentDisappearTime < 0) {
            textColor.a -= defaultDisappearSpeed * deltaTime;
            textMeshPro.color = textColor;
            if(textColor.a < 0) {
                OnTextEnd?.Invoke(this);
            }
        }
    }
}
