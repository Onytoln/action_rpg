using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StandaloneCastParameter {
    public abstract void PreCast(SkillTemplate skill, Transform releasePoint);
    public abstract void PostCast(SkillTemplate skill, Transform releasePoint);
}

public class RegularStandAloneCastParameter : StandaloneCastParameter {
 
    public override void PreCast(SkillTemplate skill, Transform releasePoint) { }

    public override void PostCast(SkillTemplate skill, Transform releasePoint) {
        skill.ConsumeResource();
        skill.ApplyCooldown();
        skill.TriggerOnSkillFiredEventExternally();
    }
}