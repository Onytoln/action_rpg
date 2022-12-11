using System;
using TMPro;
using UnityEngine;

public class CombatText : MonoBehaviour {

    [SerializeField] private TextMeshPro textMeshPro;
    
    [SerializeField, Header("Size and transform following offset")] private Vector3 combatTextDefaultSize;
    [SerializeField] private Vector3 offset;
    [SerializeField, Header("Combat text core properties")] private float defaultDissapearTime = 1;
    [SerializeField] private float defaultSpeed = 10;
    [SerializeField] private float defaultSpeedDecrement = 2f;
    [SerializeField] private float defaultDirectionX = 0.8f;
    [SerializeField] private float defaultDirectionY = 1;
    [SerializeField] private float defaultScaleRate = 1;
    [SerializeField] private float defaultDisappearSpeed = 3;
    [SerializeField, Header("Font")] private float defaultFontSize = 36;
    [SerializeField] private Color32 defaultOutlineColor = Color.black;

    private Vector3 currentScaleVector;
    private Vector3 currentDirection;
    private float currentDissapearTimeStart;
    private float currentDissapearTime;
    private float currentSpeed;
    private float currentSpeedDecrement;
    private Transform currentFollowedTransform;
    private bool scaling;
    private float currentScaleRate;
    private Color textColor;

    public event Action<CombatText> OnTextEnd;

    private void Awake() {
        if (textMeshPro == null) {
            textMeshPro = GetComponent<TextMeshPro>();
        }
    }

    public void SetCombatText(string text, Color32 textColor, Color32? outlineColor, Transform followedTransform, int fontSize, Vector3? combatTextScale,
        float disappearTime, float speed, float speedDecrement, float scaleRate, Vector3? direction, Color32? textColor2) {

        textMeshPro.SetText(text);
        SetCombatTextProperties(textColor, outlineColor, followedTransform, fontSize, combatTextScale, disappearTime, speed, speedDecrement, scaleRate, direction, textColor2);
    }

    private void SetCombatTextProperties(Color32 textColor, Color32? outlineColor, Transform followedTransform, int fontSize, Vector3? combatTextScale,
        float disappearTime, float speed, float speedDecrement, float scaleRate, Vector3? direction, Color32? textColor2) {

        if (textColor2 == null) {
            textMeshPro.color = textColor;
            textMeshPro.enableVertexGradient = false;
        } else {
            ResetColor();
            textMeshPro.colorGradient = new VertexGradient(textColor, (Color32)textColor2, textColor, (Color32)textColor2);
            textMeshPro.enableVertexGradient = true;
        }

        this.textColor = textMeshPro.color;

        if(outlineColor != null) {
            textMeshPro.outlineColor = (Color32)outlineColor;
        } else {
            textMeshPro.outlineColor = defaultOutlineColor;
        }

        if (followedTransform != null) {
            transform.SetParent(followedTransform);
        } 

        textMeshPro.sortingOrder = CombatTextUtility.combatTextSortOrder;

        if (fontSize > 0) {
            textMeshPro.fontSize = fontSize;
        } else {
            textMeshPro.fontSize = defaultFontSize;
        }

        currentFollowedTransform = followedTransform;

        if (combatTextScale != null) {
            currentScaleVector = (Vector3)combatTextScale;
            transform.localScale = currentScaleVector;
        } else {
            currentScaleVector = combatTextDefaultSize;
            transform.localScale = currentScaleVector;
        }

        if (disappearTime > 0) {
            currentDissapearTimeStart = disappearTime;
            currentDissapearTime = disappearTime;
        } else {
            currentDissapearTimeStart = defaultDissapearTime;
            currentDissapearTime = defaultDissapearTime;
        }

        if (speed > 0) {
            currentSpeed = speed;
        } else {
            currentSpeed = defaultSpeed;
        }

        if (speedDecrement > 0) {
            currentSpeedDecrement = speedDecrement;
        } else {
            currentSpeedDecrement = defaultSpeedDecrement;
        }

        if(scaleRate > 0) {
            currentScaleRate = scaleRate;
            scaling = true;
        } else if(scaleRate == 0) {
            currentScaleRate = defaultScaleRate;
            scaling = true;
        } else if(scaleRate == -1) {
            currentScaleRate = 0;
            scaling = false;
        }

        if (direction != null) {
            currentDirection = (Vector3)direction * currentSpeed;
        } else {
            currentDirection = new Vector3(UnityEngine.Random.Range(-defaultDirectionX, defaultDirectionX), defaultDirectionY, 0) * currentSpeed;
        }
    }

    public void UpdateCombatText() {
        float deltaTime = Time.deltaTime;

        transform.position += currentDirection * deltaTime;

        /*if (currentFollowedTransform == null) {
            transform.position += currentDirection * deltaTime;
        } else {
            transform.position = currentFollowedTransform.position + offset + currentDirection * deltaTime;
        }*/

        currentDirection -= currentSpeedDecrement * deltaTime * currentDirection;

        if (scaling) {
            if (currentDissapearTime > currentDissapearTimeStart * 0.5f) {
                transform.localScale += currentScaleRate * deltaTime * currentScaleVector;
            } else {
                transform.localScale -= currentScaleRate * deltaTime * currentScaleVector;
            }
        }
        
        currentDissapearTime -= deltaTime;
        if(currentDissapearTime < 0) {
            textColor.a -= defaultDisappearSpeed * deltaTime;
            textMeshPro.color = textColor;
            if(textColor.a < 0) {
                OnTextEnd?.Invoke(this);
            }
        }
    }

    private void ResetColor() {
        Color color = textMeshPro.color;
        color = Color.white;
        textMeshPro.color = color;
    }
}
