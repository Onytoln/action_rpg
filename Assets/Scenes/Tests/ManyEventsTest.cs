using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManyEventsTest : MonoBehaviour
{
    public bool acceptMessage = true;
    void Start()
    {
        EventManager.Instance.testEvent1 += DisplayMessage;
    }

    void DisplayMessage(string text) {
        if (acceptMessage) {
            Debug.Log(text);
        }
    }
}
