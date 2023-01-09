using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOnChange<out T> {
    event Action<T> OnChanged;
}
