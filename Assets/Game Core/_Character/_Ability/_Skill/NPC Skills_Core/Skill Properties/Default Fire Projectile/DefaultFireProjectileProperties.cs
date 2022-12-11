using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Default Fire Projectile Properties", menuName = "Skill/Enemy Skills Default/Default Fire Projectile Properties")]
public class DefaultFireProjectileProperties : ProjectileSkillProperties {
    public override AbilityPropertiesValuesContainer GetValuesCopy() {
        return new DefaultNPCFireProjectilePropertiesValuesContainer(this);
    }
}
