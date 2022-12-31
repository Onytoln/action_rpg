using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class StatDefinitions
{
    public static readonly Func<IEnumerable<int>, int> intSumHandler = (enumerable) => enumerable.Sum();
    public static readonly Func<IEnumerable<float>, float> floatSumHandler = (enumerable) => enumerable.Sum();
}
