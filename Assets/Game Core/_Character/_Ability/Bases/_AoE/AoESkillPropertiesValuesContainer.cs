using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoESkillPropertiesValuesContainer : SkillPropertiesValuesContainer, IAoEValues
{
    public SkillStatContainer Scale { get; private set; }

    public AoESkillPropertiesValuesContainer(AoESkillProperties aoeProp) : base(aoeProp) {
        Scale = new SkillStatContainer(aoeProp.Scale);
    }

    public override IAoEValues TryGetAoEPropertiesValues() { return this; }

    public SkillStatContainer ScaleValues => Scale;

}
