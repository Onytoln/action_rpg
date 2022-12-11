using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilesFireInstanceMaster
{
    public readonly CoreAbilityData coreAbilityData;
    public readonly GameObject projectile;
    public readonly bool setXRot;
    public readonly int projectileCount;
    public readonly Transform releaseObj;
    public readonly Vector3 castPoint;

    public readonly Dictionary<Collider, int> collectiveHitDict = new Dictionary<Collider, int>();

    public ProjectilesFireInstanceMaster(GameObject projectile, CoreAbilityData coreAbilityData, bool setXRot,
        int projectileCount, Transform releaseObj, Vector3 castPoint) {

        this.projectile = projectile;
        this.coreAbilityData = coreAbilityData;
        this.setXRot = setXRot;
        this.projectileCount = projectileCount;
        this.releaseObj = releaseObj;
        this.castPoint = castPoint;
    }
}
