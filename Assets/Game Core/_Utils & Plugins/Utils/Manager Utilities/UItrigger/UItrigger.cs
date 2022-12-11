using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UItrigger : MonoBehaviour
{
    public bool BlockedByUI { get; private set; }
    private EventTrigger eventTrigger;

    public static UItrigger Instance { get; set; }

    private void Awake() {
        if (Instance == null) Instance = this;
;    }

    void Start()
    {
        eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger != null)
        {
            EventTrigger.Entry enterUIEntry = new EventTrigger.Entry();
            // Pointer Enter
            enterUIEntry.eventID = EventTriggerType.PointerEnter;
            enterUIEntry.callback.AddListener((eventData) => { EnterUI(false); });
            eventTrigger.triggers.Add(enterUIEntry);

            //Pointer Exit
            EventTrigger.Entry exitUIEntry = new EventTrigger.Entry();
            exitUIEntry.eventID = EventTriggerType.PointerExit;
            exitUIEntry.callback.AddListener((eventData) => { ExitUI(); });
            eventTrigger.triggers.Add(exitUIEntry);
        }
    }

    public void EnterUI(bool overrideCheck = true)
    {
        if (overrideCheck) {
            BlockedByUI = true;
        } else {
            var pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult> ();

            EventSystem.current.RaycastAll(pointerEventData, results);

            for (int i = 0; i < results.Count; i++) {
                if (results[i].gameObject.TryGetComponent(out UIElementBlockTriggerDisabler _)) {
                    BlockedByUI = false;
                    return;
                }
            }

            BlockedByUI = true;
        }
    }

    public void ExitUI()
    {
        BlockedByUI = false;
    }

}
