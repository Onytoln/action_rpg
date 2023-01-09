using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ModifierHandler<T> : IEnumerable<T> where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable {

    [NonSerialized] private List<T> _modifiers;
    private List<T> Modifiers => _modifiers ??= new List<T>(); //lazy loading

    private readonly Func<IEnumerable<T>, T> _sumHandler;

    private T _sumVal;
    public T SumVal {
        get {
            if (_isDirty) {
                _isDirty = false;
                _sumVal = _sumHandler(Modifiers);
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
        Modifiers.Add(modifier);
    }

    public bool Remove(T modifier) {
        _isDirty = true;
        return Modifiers.Remove(modifier);
    }

    public bool Replace(T modifier, T replaceWith) {
        int index = Modifiers.FindIndex(x => x.Equals(replaceWith));
        if (index == -1) return false;

        Modifiers[index] = modifier;
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    public List<T>.Enumerator GetEnumerator() => Modifiers.GetEnumerator();
}
