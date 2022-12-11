using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDelegatePerformance : MonoBehaviour
{
    public KeyCode keyCode;
    void Start()
    {
        InputManager.Instance.OnKeyDown += OnKeyPressed;
    }

    public void OnKeyPressed(KeyCode keyCode) {
        if(keyCode == this.keyCode) {
            Debug.Log("yes" + gameObject.name);
        }
    }
}
