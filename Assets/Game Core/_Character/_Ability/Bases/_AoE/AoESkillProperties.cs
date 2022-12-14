using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoESkillProperties : SkillProperties, IAoE {
    [field: SerializeField] public StatFloat Scale { get; private set; }
    
    public override AbilityPropertiesValuesContainer GetValuesCopy() {
        return new AoESkillPropertiesValuesContainer(this);
    }
}
