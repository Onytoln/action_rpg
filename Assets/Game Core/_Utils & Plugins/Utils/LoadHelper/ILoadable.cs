using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoadable {
    public bool IsLoaded { get; }
    public event Action<ILoadable> OnLoad;
}
