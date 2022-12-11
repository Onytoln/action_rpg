using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMissilePropertiesValuesContainer : ProjectileSkillPropertiesValuesContainer {
    public SkillStatContainer CooldownOnHit { get; private set; }
    public SkillStatContainer DamageBonusByCooldown { get; private set; }
    public SkillStatContainer CooldownForMaxDamageBonus { get; private set; }

    public MagicMissilePropertiesValuesContainer(MagicMissileProperties ab) : base(ab) {
        CooldownOnHit = new SkillStatContainer(ab.cooldownOnHit);
        DamageBonusByCooldown = new SkillStatContainer(ab.damageBonusByCooldown);
        CooldownForMaxDamageBonus = new SkillStatContainer(ab.cooldownForMaxDamageBonus);
    }
}
