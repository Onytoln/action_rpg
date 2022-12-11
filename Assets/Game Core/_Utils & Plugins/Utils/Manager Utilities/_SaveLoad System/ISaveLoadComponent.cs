using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveLoadComponent { }

public interface ISaveLoadComponentProvider : ISaveLoadComponent {
    IEnumerable<ISaveLoadComponent> GetSaveLoadComponents();
}

public interface ISaveable : ISaveLoadComponent {

    string SaveableID { get; }

    object Save();
    void Load(object saveData);
}

public interface IGlobalSaveable : ISaveable {
    string SaveFileName { get; }
    string GlobalSaveableContext { get; }
}

public interface ISaveLoadPlayerContext : ISaveLoadComponent {
    string SaveFileNameSuffix { get; }

    object GetContextData();
    void SetContextData(object contextData);
}