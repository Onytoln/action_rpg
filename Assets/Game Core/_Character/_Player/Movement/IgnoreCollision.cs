using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollision : MonoBehaviour
{
    public CapsuleCollider playerCollider;
    public CapsuleCollider collisionBlockerCollider;
    void Start() {
        Physics.IgnoreCollision(playerCollider, collisionBlockerCollider, true);
    }
}
