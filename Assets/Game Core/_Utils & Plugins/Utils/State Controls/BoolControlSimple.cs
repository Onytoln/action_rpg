using System;
using System.Collections.Generic;
using UnityEngine;

public enum BoolType { False = 0, True = 1 };

public enum BoolCountEqualResult { Default, SentValue, OppositeOfDefault }

[System.Serializable]
public class BoolControlSimple {
    [SerializeField] private BoolType defaultValue;
    private bool defaultVal;

    private bool value;
    public bool Value {
        get {
            Initialize();
            return value;
        }
        private set {
            this.value = value;
        }
    }

    private bool prevValue;
    public bool PrevValue {
        get {
            Initialize();
            return prevValue;
        }
        private set {
            this.prevValue = value;
        }
    }

    public bool PrevEqualsCurrent => Value == PrevValue;

    private int oppositeCount;

    private bool? startWithValue;

    [System.NonSerialized] private bool initialized = false;

    public BoolControlSimple(BoolType defaultValue, bool? startWithValue = null) {
        this.defaultValue = defaultValue;
        this.startWithValue = startWithValue;
        Initialize();
        initialized = false;
    }

    void Initialize() {
        if (initialized) return;
        initialized = true;

        defaultVal = defaultValue == BoolType.False ? false : true;
        value = startWithValue ?? defaultVal;
        PrevValue = defaultVal;
    }

    public void Set(bool value) {
        Initialize();

        PrevValue = Value;

        if (value == defaultVal) {
            oppositeCount--;
            Clamp();
        } else {
            oppositeCount++;
        }

        Value = oppositeCount > 0 ? !defaultVal : defaultVal;

        void Clamp() {
            if (oppositeCount < 0) {
                oppositeCount = 0;
            }
        }
    }

}

[System.Serializable]
public class BoolControlComplex {
    [SerializeField] private BoolType defaultValue;
    private bool defaultVal;

    [SerializeField] private BoolCountEqualResult whenEqual;

    private bool value;
    public bool Value {
        get {
            Initialize();
            return value;
        }
        private set {
            this.value = value;
        }
    }

    private bool prevValue;
    public bool PrevValue {
        get {
            Initialize();
            return prevValue;
        }
        private set {
            this.prevValue = value;
        }
    }

    public bool PrevEqualsCurrent => Value == PrevValue;

    private int trueCount;
    private int falseCount;

    private bool? startWithValue;

    [System.NonSerialized] private bool initialized = false;

    public BoolControlComplex(BoolType defaultValue, BoolCountEqualResult whenEqual, bool? startWithValue = null) {
        this.defaultValue = defaultValue;
        this.whenEqual = whenEqual;
        this.startWithValue = startWithValue;
        Initialize();
        initialized = false;
    }

    void Initialize() {
        if (initialized) return;
        initialized = true;

        defaultVal = defaultValue == BoolType.False ? false : true;
        value = startWithValue ?? defaultVal;
        PrevValue = defaultVal;
    }

    public void Set(bool value) {
        Initialize();

        PrevValue = Value;

        if (value) {
            trueCount++;
        } else {
            falseCount++;
        }

        if (trueCount > falseCount) {
            Value = true;
        } else if (falseCount > trueCount) {
            Value = false;
        } else {
            SetWhenEqual(value);
        }
    }

    public void Unset(bool value) {
        Initialize();

        PrevValue = Value;

        if (value) {
            trueCount--;
        } else {
            falseCount--;
        }

        Clamp();

        if (trueCount > falseCount) {
            Value = true;
        } else if (falseCount > trueCount) {
            Value = false;
        } else {
            SetWhenEqual(value);
        }

        void Clamp() {
            if (trueCount < 0) {
                trueCount = 0;
            }

            if (falseCount < 0) {
                falseCount = 0;
            }
        }
    }

    void SetWhenEqual(bool sentVal) {
        switch (whenEqual) {
            case BoolCountEqualResult.Default:
                Value = defaultVal;
                break;
            case BoolCountEqualResult.SentValue:
                Value = sentVal;
                break;
            case BoolCountEqualResult.OppositeOfDefault:
                Value = !defaultVal;
                break;
        }
    }
}

[System.Serializable]
public class BoolControlSourced {
    [SerializeField] private BoolType defaultValue;
    private bool defaultVal;

    private bool value;
    public bool Value {
        get {
            Initialize();
            return value;
        }
        private set {
            this.value = value;
        }
    }

    private bool prevValue;
    public bool PrevValue {
        get {
            Initialize();
            return prevValue;
        }
        private set {
            this.prevValue = value;
        }
    }

    public bool PrevEqualsCurrent => Value == PrevValue;

    private bool? startWithValue;

    private readonly HashSet<object> sources = new HashSet<object>();

    [System.NonSerialized] private bool initialized = false;

    public BoolControlSourced(BoolType defaultValue, bool? startWithValue = null) {
        this.defaultValue = defaultValue;
        this.startWithValue = startWithValue;
        Initialize();
        initialized = false;
    }

    void Initialize() {
        if (initialized) return;
        initialized = true;

        defaultVal = defaultValue == BoolType.False ? false : true;
        value = startWithValue ?? defaultVal;
        PrevValue = value;
    }

    public void Set(bool value, object source) {
        Initialize();

        if (source is bool) {
            Debug.LogError("You cannot send a boolean as source parameter for BoolControlSourced!");
        }

        PrevValue = Value;

        if (value) {
            sources.Add(source);
        } else {
            sources.Remove(source);
        }

        Value = sources.Count > 0 ? !defaultVal : defaultVal;
    }

}
