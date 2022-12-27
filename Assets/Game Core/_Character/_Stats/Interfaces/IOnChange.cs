using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOnChange<T> {
    event Action<T> OnChanged;
}
