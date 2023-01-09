using System;
using UnityEngine;

public interface IActionBarSlottable : ISlottable, ICooldown
{
    public KeyCode AssignedKey { get; set; }
    public Action<int> OnUsesAmountChanged { get; set; }

    public bool CheckForDistance { get; set; }
    public Vector3 UsePoint { get; set; }
    public GameObject UseTarget { get; set; }

    bool UseSlottable();
    bool CanUse(bool overrideDistanceCheck = true);
    public int GetUsesAmount();
    public int GetMaxUsesAmount();
    public float GetUseRange();
}
