using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class StatChangeNotifyClassWide : Attribute {
    public readonly string[] methodNames;

    public StatChangeNotifyClassWide(string methodName) {
        this.methodNames = new string[] { methodName };
    }

    public StatChangeNotifyClassWide(params string[] methodNames) {
        this.methodNames = methodNames;
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class StatChangeNotify : Attribute {

    public readonly string[] methodNames;
    public bool NotifyOnChange { get; set; } = true;

    public StatChangeNotify(string methodName) {
        this.methodNames = new string[] { methodName };
    }

    public StatChangeNotify(params string[] methodNames) {
        this.methodNames = methodNames;
    }

    public static void ObserveStatChanges(object statsSource) {
        _ = Task.Run(() => ObserveStatChangesAsync(statsSource))
            .ContinueWith((x) => { Debug.LogError($"Observing stat changes initialization has failed. Exception: {x.Exception.Message}"); }, TaskContinuationOptions.OnlyOnFaulted);
    }

    private static void ObserveStatChangesAsync(object statsSource) {
        var statsSourceType = statsSource.GetType();

        var observableProperties = statsSourceType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => x.PropertyType.IsSameOrSubclassOf(typeof(StatBase)));

        var observableFields = statsSourceType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => x.FieldType.IsSameOrSubclassOf(typeof(StatBase)))
            .ToList();

        if (!observableProperties.Any() && observableFields.Count == 0) return;

        Dictionary<string, Action> methods = new Dictionary<string, Action>();
        List<string> defaultMethods = new List<string>();
        HashSet<StatBase> invokesDefined = new HashSet<StatBase>();

        var classAttribute = statsSourceType.GetCustomAttribute<StatChangeNotifyClassWide>();
        if (classAttribute != null) {
            for (int i = 0; i < classAttribute.methodNames.Length; i++) {
                if (methods.TryGetValue(classAttribute.methodNames[i], out var _)) continue;

                var methodInfo = statsSourceType.GetMethod(classAttribute.methodNames[i]);

                if (methodInfo == null) {
                    Debug.LogError($"Method of name {classAttribute.methodNames[i]} does not exist for {nameof(StatChangeNotifyClassWide)} attribute.");
                    continue;
                }

                if (methodInfo.GetParameters().Length > 0) {
                    Debug.LogError($"Cannot use {nameof(StatChangeNotifyClassWide)} attribute to call methods with parameters.");
                    continue;
                }

                Action defaultMethod = ReflectionExt.CreateDelegate<Action>(methodInfo, statsSource);
                defaultMethods.Add(classAttribute.methodNames[i]);
                methods.Add(classAttribute.methodNames[i], defaultMethod);
            }
        }

        foreach (var property in observableProperties) {
            Process(property, methods, statsSource, statsSourceType, defaultMethods, invokesDefined);
            observableFields.Add(ReflectionExt.GetBackingField(property));
        }

        foreach (var field in observableFields) {
            Process(field, methods, statsSource, statsSourceType, defaultMethods, invokesDefined);
        }
    }

    private static void Process<T>(T member, Dictionary<string, Action> methods, object statsSource, Type statsSourceType, List<string> defaultMethods, HashSet<StatBase> invokesDefined)
        where T : MemberInfo {
        var attribute = member.GetCustomAttribute<StatChangeNotify>();

        HashSet<string> toInvoke = new HashSet<string>(defaultMethods);

        if (attribute != null && attribute.NotifyOnChange) {
            foreach (var methodName in attribute.methodNames) {
                if (!methods.TryGetValue(methodName, out var _)) {
                    var methodInfo = statsSourceType.GetMethod(methodName);

                    if (methodInfo == null) {
                        Debug.LogError($"Method of name {methodName} does not exist for {nameof(StatChangeNotify)} attribute.");
                        continue;
                    }

                    if (methodInfo.GetParameters().Length > 0) {
                        Debug.LogError($"Cannot use {nameof(StatChangeNotify)} attribute to call methods with parameters.");
                        continue;
                    }

                    methods.Add(methodName, ReflectionExt.CreateDelegate<Action>(methodInfo, statsSource));
                }

                toInvoke.Add(methodName);
            }
        }

        DefineInvoke(member, methods, statsSource, toInvoke, invokesDefined);
    }

    private static void DefineInvoke<T>(T member, Dictionary<string, Action> methods, object statsSource, HashSet<string> toInvoke, HashSet<StatBase> invokesDefined)
        where T : MemberInfo {
        StatBase stat = null;

        if (member is PropertyInfo property) {
            stat = property.GetValue(statsSource, null) as StatBase;
        } else if (member is FieldInfo field) {
            stat = field.GetValue(statsSource) as StatBase;
        }

        if (stat == null || invokesDefined.Contains(stat)) return;

        Debug.Log("creating invoke");

        stat.OnStatChanged += OnStatChangeAction;
        invokesDefined.Add(stat);

        void OnStatChangeAction(StatBase statBase) {
            foreach (var methodName in toInvoke) {
                if (methods.TryGetValue(methodName, out var method))
                    method.SafeInvoke();
            }
        }
    }
}



