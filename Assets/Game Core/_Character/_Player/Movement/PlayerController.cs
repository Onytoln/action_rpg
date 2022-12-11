using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour, ICharacterControllerKeyFunc, IDisableUntilGameLoaded {
    public Interactable focus;
    private PlayerMotor motor;
    public Animator animator;
    public StatusEffectsManager statusEffectsManager;
    public bool CanM1Move { get; set; } = true;

    private TargetManager targetManager;

    public Vector3 lastPos;

    private float stuckTime;

    private bool pickedUpItemRecently;

    public event Action<Vector3> OnDestinationChanged;

    private void Start() {
        motor = GetComponent<PlayerMotor>();
        statusEffectsManager = gameObject.GetComponent<StatusEffectsManager>();
        targetManager = TargetManager.Instance;
        pickedUpItemRecently = false;
        CanM1Move = true;
    }

    void Update() {
        if (!statusEffectsManager.IsStationary && Vector3.Distance(lastPos, transform.position) < motor.agent.speed * Time.deltaTime * 0.5f) {
            stuckTime += Time.deltaTime;
            if (stuckTime >= 0.3f) {
                StopMovement();
                stuckTime = 0f;
            }
        } else {
            stuckTime = 0f;
        }

        if (transform.position != lastPos) {
            animator.SetBool("IsMoving", true);
            statusEffectsManager.SetIsStationary(false);
        } else {
            animator.SetBool("IsMoving", false);
            statusEffectsManager.SetIsStationary(true);
        }
        lastPos = transform.position;

        if (Input.GetMouseButtonUp(0) == true && pickedUpItemRecently == true) pickedUpItemRecently = false;

        M1HoldMove();
        M1DownMove();

        /*if (!UItrigger.instance.BlockedByUI) {
            if (Input.GetMouseButton(0)) {
                stuckTime = 0;
                statusEffectsManager.InterruptCurrentAnimIfPossible();
                if (statusEffectsManager.CanMove()) {
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);

                    if (Physics.Raycast(ray, out hit, 100, movementMask)) {
                        if (targetManager.TryGetPlayersInteractableTarget() == null) {
                            motor.MoveToPoint(hit.point);
                            RemoveFocus();
                        }
                    }
                }
            }

            if (Input.GetMouseButtonDown(0)) {
                statusEffectsManager.InterruptCurrentAnimIfPossible();
                if (statusEffectsManager.CanMove()) {
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);

                    if (Physics.Raycast(ray, out hit, 100, movementMask)) {
                        Interactable interactable = targetManager.TryGetPlayersInteractableTarget();
                        if (interactable != null) {
                            RemoveFocus();
                            SetFocus(interactable);
                        }
                    }
                }
            }
        }*/
    }

    private void M1HoldMove() {
        if (UItrigger.Instance.BlockedByUI || pickedUpItemRecently) return;

        if (CanM1Move && Input.GetMouseButton(0)) {
            stuckTime = 0;
            statusEffectsManager.TryInterruptCurrentAnim();
            if (statusEffectsManager.CanMove()) {
                var pointData = targetManager.TryGetPlayersCurrentWalkableTargetPoint();
                if (pointData.pointValid) {
                    RemoveFocus();
                    motor.MoveToPoint(pointData.point);
                    OnDestinationChanged?.Invoke(pointData.point);
                }
            }
        }
    }

    private void M1DownMove() {
        if (UItrigger.Instance.BlockedByUI || pickedUpItemRecently) return;

        if (Input.GetMouseButtonDown(0)) {
            statusEffectsManager.TryInterruptCurrentAnim();
            if (statusEffectsManager.CanMove()) {
                Interactable interactable = TargetManager.CurrentPlayersInteractableTarget;
                if (interactable != null) {
                    RemoveFocus();
                    SetFocus(interactable);
                    pickedUpItemRecently = true;
                    OnDestinationChanged?.Invoke(interactable.transform.position);
                }
            }
        }
    }

    void SetFocus(Interactable newFocus) {
        focus = newFocus;
        motor.FollowTarget(newFocus);
        newFocus.OnFocused(transform);
    }

    void RemoveFocus() {
        if (focus != null)
            focus.OnDefocused();

        focus = null;
        motor.StopFollowingTarget();
    }

    public void StopMovement() {
        motor.FullStop();
    }

    public void FacePointQuickly(Vector3 point) {
        RemoveFocus();
        motor.FacePointQuickly(point);
    }

    public void MoveToPointExternal(Vector3 point) {
        statusEffectsManager.TryInterruptCurrentAnim();
        if (!statusEffectsManager.CanMove()) return;
        RemoveFocus();
        motor.MoveToPoint(point);
        OnDestinationChanged?.Invoke(point);
    }
}
