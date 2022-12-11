using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoEPropertiesContainerStandAlone : IAoEValues {
    public SkillStatContainer Scale { get; private set; }
   
    public AoEPropertiesContainerStandAlone(IAoE aoe) {
        Scale = new SkillStatContainer(aoe.Scale);
    }

    public SkillStatContainer ScaleValues => Scale;
}
