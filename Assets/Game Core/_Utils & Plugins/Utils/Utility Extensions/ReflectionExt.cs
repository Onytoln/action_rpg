using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class ReflectionExt {
    public static T CreateDelegate<T>(this MethodInfo methodInfo, object target) {
        if (!typeof(T).IsSameOrSubclassOf(typeof(Delegate))) {
            Debug.LogError($"{nameof(CreateDelegate)} generic type must be of type Delegate.");
            return default;
        }

        var parmTypes = methodInfo.GetParameters().Select(parm => parm.ParameterType);
        var parmAndReturnTypes = parmTypes.Append(methodInfo.ReturnType).ToArray();
        var delegateType = Expression.GetDelegateType(parmAndReturnTypes);

        if (methodInfo.IsStatic)
            return (T)(object)methodInfo.CreateDelegate(delegateType);

        return (T)(object)methodInfo.CreateDelegate(delegateType, target);
    }

    public static bool IsSameOrSubclassOf(this Type potentialDescendant, Type baseClass) {
        return potentialDescendant.IsSubclassOf(baseClass) || potentialDescendant == baseClass;
    }

    public static FieldInfo GetBackingField(PropertyInfo propertyInfo) {
        if (!propertyInfo.CanRead || !propertyInfo.GetGetMethod(nonPublic: true).IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
            return null;

        var backingField = propertyInfo.DeclaringType.GetField($"<{propertyInfo.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

        if (backingField == null)
            return null;

        if (!backingField.IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
            return null;

        return backingField;
    }
}
