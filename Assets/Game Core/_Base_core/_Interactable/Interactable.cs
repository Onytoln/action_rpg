using UnityEngine;

public class Interactable : MonoBehaviour, ISelected {
    public float radiusItem = 3f;
    public Transform interactionTransform;

    bool isFocus = false;
    Transform player;

    bool hasInteracted = false;

    private Tooltip tooltip;
    public Tooltip Tooltip { get { if (tooltip == null) tooltip = Tooltip.Instance; return tooltip; } }

    public virtual void Awake() {
    }

    public virtual void Start() {
        enabled = false;
    }

    public virtual void OnValidate() { }

    public virtual void OnEnable() {

    }

    public virtual void OnDisable() {

    }

    public virtual void Interact() {
        //this method is meant to be overwritten
        //Debug.Log("Interacting with " + transform.name);
    }

    private void Update() {
        if (isFocus && !hasInteracted) {
            float distance = Vector3.Distance(player.position, interactionTransform.position);
            if (distance <= radiusItem + 0.3f) {
                hasInteracted = true;
                Interact();
                transform.hasChanged = false;
            }
        }
    }

    public void OnFocused(Transform playerTransform) {
        isFocus = true;
        player = playerTransform;
        hasInteracted = false;
        enabled = true;
    }

    public void OnDefocused() {
        isFocus = false;
        hasInteracted = false;
        enabled = false;
    }

    public virtual void OnSelected() {
        if (Tooltip == null) return;
        Tooltip.ShowTooltip(name);
    }

    public virtual void OnDeselected() {
        if (Tooltip == null) return;
        Tooltip.HideTooltip();
    }

    private void OnDrawGizmosSelected() {
        if (interactionTransform == null)
            interactionTransform = transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactionTransform.position, radiusItem);
    }
}
