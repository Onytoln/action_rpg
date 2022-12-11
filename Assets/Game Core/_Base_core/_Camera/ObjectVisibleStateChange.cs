using System;
using UnityEngine;

public class ObjectVisibleStateChange : MonoBehaviour
{
    public Action onBecameVisible;
    public Action onBecameInvisible;

    public void OnBecameVisible() {
        onBecameVisible?.Invoke();
    }

    public void OnBecameInvisible() {
        onBecameInvisible?.Invoke();
    }
}
