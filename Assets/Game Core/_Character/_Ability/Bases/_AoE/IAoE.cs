using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAoE {
    public StatFloat Scale { get; }
}

public interface IAoEValues {
    public SkillStatContainer ScaleValues { get; }
}