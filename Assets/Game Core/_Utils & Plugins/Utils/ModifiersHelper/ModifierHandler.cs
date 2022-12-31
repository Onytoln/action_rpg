using System;
using System.Collections.Generic;

[System.Serializable]
public class ModifierHandler<T> where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable {

    [NonSerialized] private readonly List<T> _modifiers = new List<T>();

    private readonly Func<IEnumerable<T>, T> _sumHandler;

    private T _sumVal;
    public T SumVal {
        get {
            if (_isDirty) {
                _isDirty = false;
                _sumVal = _sumHandler(_modifiers);
            }

            return _sumVal;
        }
    }

    [NonSerialized] private bool _isDirty;

    public ModifierHandler(Func<IEnumerable<T>, T> sumHandler) {
        _sumHandler = sumHandler;
    }

    public void Add(T modifier) {
        _isDirty = true;
        _modifiers.Add(modifier);
    }

    public bool Remove(T modifier) {
        _isDirty = true;
        return _modifiers.Remove(modifier);
    }

    public bool Replace(T modifier, T replaceWith) {
        int index = _modifiers.FindIndex(x => x.Equals(replaceWith));
        if (index == -1) return false;

        _modifiers[index] = modifier;
        return true;
    }
}
