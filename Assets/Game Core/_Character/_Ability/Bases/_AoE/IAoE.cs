using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAoE {
    public SkillStat Scale { get; }
}

public interface IAoEValues {
    public SkillStatContainer ScaleValues { get; }
}