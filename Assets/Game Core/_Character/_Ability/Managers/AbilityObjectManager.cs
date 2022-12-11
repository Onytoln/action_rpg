using MEC;
using System.Collections.Generic;
using UnityEngine;

public class AbilityObjectManager : MonoBehaviour {
    #region Singleton
    public static AbilityObjectManager Instance { get; private set; }
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
    }
    #endregion

    private EventManager eventManager;

    void Start() {
        eventManager = EventManager.Instance;
    }

    #region Fire Projectiles

    public ProjectilesFireInstanceMaster FireProjectiles(GameObject projectile, AbilityProperties abilityProperties, Character character, Transform releaseObj, Vector3 castPoint,
        SkillTemplate skillLogic = null, GameObject target = null, bool setXRot = true) {

        if (projectile == null || abilityProperties == null) return null;
        if (!(abilityProperties is IProjectile projectileProperties)) return null;
        if (projectileProperties.ProjectileCount.GetValue() <= 0) return null;

        //Profiler.BeginSample("Instantiate ability object data");
        CoreAbilityData coreAbilityData = new CoreAbilityData(character, abilityProperties, abilityProperties.GetValuesCopy(),
            character.CharacterStats.CoreStats.GetStatsValuesCopy(), character.CharacterCombat, null, skillLogic, castPoint, target);
        //Profiler.EndSample();
        ProjectilesFireInstanceMaster projectilesFireInstanceMaster = new ProjectilesFireInstanceMaster(
            projectile,
            coreAbilityData,
            setXRot,
            projectileProperties.ProjectileCount.GetValue(),
            releaseObj,
            castPoint
        );

        switch (projectileProperties.ProjectileFireType) {
            case ProjectileFireType.InLine:
                Timing.RunCoroutine(FireProjectilesInLine(projectilesFireInstanceMaster));
                break;
            case ProjectileFireType.Cone:
                FireProjectilesCone(projectilesFireInstanceMaster);
                break;
            case ProjectileFireType.Spread180:
                FireProjectilesSpread180(projectilesFireInstanceMaster);
                break;
            case ProjectileFireType.Spread360:
                FireProjectilesSpread360(projectilesFireInstanceMaster);
                break;
            default:
                Timing.RunCoroutine(FireProjectilesInLine(projectilesFireInstanceMaster));
                break;
        }

        return projectilesFireInstanceMaster;
    }

    private Quaternion GetXRotation(bool setXRot, Transform objToRotate, Vector3 castPoint, Transform releaseObj) {
        if (setXRot) {
            return Utils.GetXRotationBasedOnCastPoint(objToRotate, castPoint, releaseObj);
        } 
            
        return Quaternion.identity;
    }

    private IEnumerator<float> FireProjectilesInLine(ProjectilesFireInstanceMaster projectilesFireInstanceMaster) {

        ObjectPoolManager objectPoolManager = ObjectPoolManager.Instance;

        Vector3 releasePoint = projectilesFireInstanceMaster.releaseObj.position;
        Quaternion releaseRotation = projectilesFireInstanceMaster.releaseObj.rotation;

        Quaternion xRot = GetXRotation(projectilesFireInstanceMaster.setXRot, projectilesFireInstanceMaster.releaseObj,
            projectilesFireInstanceMaster.castPoint, projectilesFireInstanceMaster.releaseObj);

        for (int i = 0; i < projectilesFireInstanceMaster.projectileCount; i++) {
            GameObject instantiatedSkillObject = objectPoolManager.GetPooledObject(projectilesFireInstanceMaster.projectile.name,
                projectilesFireInstanceMaster.projectile, releasePoint, releaseRotation);

            instantiatedSkillObject.transform.AddLocal(xRot);

            _ = ProcessSpawnedAbilityObject(instantiatedSkillObject, projectilesFireInstanceMaster.coreAbilityData, true, projectilesFireInstanceMaster);
            yield return Timing.WaitForSeconds(0.15f);
        }
    }

    private void FireProjectilesCone(ProjectilesFireInstanceMaster projectilesFireInstanceMaster) {

        if (projectilesFireInstanceMaster.projectileCount < 2) {
            Timing.RunCoroutine(FireProjectilesInLine(projectilesFireInstanceMaster));
            return;
        }

        ObjectPoolManager objectPoolManager = ObjectPoolManager.Instance;

        Transform[] firedProjectilesTransforms = new Transform[projectilesFireInstanceMaster.projectileCount];

        for (int i = 0; i < projectilesFireInstanceMaster.projectileCount; i++) {

            GameObject instantiatedSkillObject = objectPoolManager.GetPooledObject(projectilesFireInstanceMaster.projectile.name,
                projectilesFireInstanceMaster.projectile, projectilesFireInstanceMaster.releaseObj.position, projectilesFireInstanceMaster.releaseObj.rotation);

            AbilityObject instantiatedProjectile = ProcessSpawnedAbilityObject(instantiatedSkillObject, projectilesFireInstanceMaster.coreAbilityData,
                true, projectilesFireInstanceMaster);

            firedProjectilesTransforms[i] = instantiatedProjectile.transform;
        }

        Utils.SetTransformsYRotationsByTypeCone(
            firedProjectilesTransforms,
            projectilesFireInstanceMaster.coreAbilityData.CastPoint,
            projectilesFireInstanceMaster.releaseObj.position,
            GetXRotation(projectilesFireInstanceMaster.setXRot, projectilesFireInstanceMaster.releaseObj,
            projectilesFireInstanceMaster.castPoint, projectilesFireInstanceMaster.releaseObj));
    }

    private void FireProjectilesSpread180(ProjectilesFireInstanceMaster projectilesFireInstanceMaster) {

        if (projectilesFireInstanceMaster.projectileCount < 3) {
            FireProjectilesCone(projectilesFireInstanceMaster);
            return;
        }

        ObjectPoolManager objectPoolManager = ObjectPoolManager.Instance;

        Transform[] firedProjectiles = new Transform[projectilesFireInstanceMaster.projectileCount];

        for (int i = 0; i < projectilesFireInstanceMaster.projectileCount; i++) {

            GameObject instantiatedSkillObject = objectPoolManager.GetPooledObject(projectilesFireInstanceMaster.projectile.name,
              projectilesFireInstanceMaster.projectile, projectilesFireInstanceMaster.releaseObj.position, projectilesFireInstanceMaster.releaseObj.rotation);

            AbilityObject instantiatedProjectile = ProcessSpawnedAbilityObject(instantiatedSkillObject, projectilesFireInstanceMaster.coreAbilityData,
                true, projectilesFireInstanceMaster);

            firedProjectiles[i] = instantiatedProjectile.transform;
        }

        Utils.SetTransformsYRotationsByTypeSpread180(firedProjectiles, GetXRotation(projectilesFireInstanceMaster.setXRot, projectilesFireInstanceMaster.releaseObj,
            projectilesFireInstanceMaster.castPoint, projectilesFireInstanceMaster.releaseObj), true);
    }

    private void FireProjectilesSpread360(ProjectilesFireInstanceMaster projectilesFireInstanceMaster) {

        if (projectilesFireInstanceMaster.projectileCount < 4) {
            FireProjectilesSpread180(projectilesFireInstanceMaster);
            return;
        }

        ObjectPoolManager objectPoolManager = ObjectPoolManager.Instance;

        Transform[] firedProjectiles = new Transform[projectilesFireInstanceMaster.projectileCount];

        Transform spawnTrans = projectilesFireInstanceMaster.coreAbilityData.CharacterComponent.transform;
        Vector3 spawnPos = spawnTrans.position + (Vector3.up * projectilesFireInstanceMaster.releaseObj.localPosition.y);
        Quaternion spawnRot = spawnTrans.rotation;

        for (int i = 0; i < projectilesFireInstanceMaster.projectileCount; i++) {

            GameObject instantiatedSkillObject = objectPoolManager.GetPooledObject(projectilesFireInstanceMaster.projectile.name,
               projectilesFireInstanceMaster.projectile, spawnPos, spawnRot);

            AbilityObject instantiatedProjectile = ProcessSpawnedAbilityObject(instantiatedSkillObject, projectilesFireInstanceMaster.coreAbilityData, 
                true, projectilesFireInstanceMaster);

            firedProjectiles[i] = instantiatedProjectile.transform;
        }

        Utils.SetTransformsYRotationsByTypeSpread360(firedProjectiles, GetXRotation(projectilesFireInstanceMaster.setXRot, spawnTrans,
            projectilesFireInstanceMaster.castPoint, projectilesFireInstanceMaster.releaseObj));
    }

    #endregion

    #region Process Ability Object

    //no params
    public static AbilityObject ProcessSpawnedAbilityObject(GameObject gameObj, AbilityProperties abilityProp, Character character,
        SkillTemplate skillLogic = null, Vector3 castPoint = default, GameObject target = null, bool activateObject = true) {
        if (gameObj == null) return null;

        if (!gameObj.TryGetComponent<AbilityObject>(out var obj)) return null;

        obj.Initialize(
            new CoreAbilityData(character,
            abilityProp,
            abilityProp.GetValuesCopy(),
            character.CharacterStats.CoreStats.GetStatsValuesCopy(),
            character.CharacterCombat,
            obj,
            skillLogic,
            castPoint,
            target));

        if (activateObject && !obj.gameObject.activeSelf) {
            obj.gameObject.SetActive(true);
        }

        return obj;
    }

    //params
    public static AbilityObject ProcessSpawnedAbilityObject(GameObject gameObj, AbilityProperties abilityProp, Character character,
        SkillTemplate skillLogic = null, Vector3 castPoint = default, GameObject target = null, bool activateObject = true, params object[] parameters) {
        if (gameObj == null) return null;

        if (!gameObj.TryGetComponent<AbilityObject>(out var obj)) return null;

        obj.Initialize(
            new CoreAbilityData(character,
            abilityProp,
            abilityProp.GetValuesCopy(),
            character.CharacterStats.CoreStats.GetStatsValuesCopy(),
            character.CharacterCombat,
            obj,
            skillLogic,
            castPoint,
            target));

        obj.Initialize(parameters);

        if (activateObject && !obj.gameObject.activeSelf) {
            obj.gameObject.SetActive(true);
        }

        return obj;
    }

    //no params
    public static AbilityObject ProcessSpawnedAbilityObject(GameObject gameObj, AbilityProperties abilityProp, CoreStatsValuesContainer stats,
        Character character, SkillTemplate skillLogic = null, Vector3 castPoint = default, GameObject target = null, bool activateObject = true) {
        if (gameObj == null) return null;

        if (!gameObj.TryGetComponent<AbilityObject>(out var obj)) return null;

        obj.Initialize(
            new CoreAbilityData(character,
            abilityProp,
            abilityProp.GetValuesCopy(),
            stats,
            character.CharacterCombat,
            obj,
            skillLogic,
            castPoint,
            target));

        if (activateObject && !obj.gameObject.activeSelf) {
            obj.gameObject.SetActive(true);
        }

        return obj;
    }

    //params
    public static AbilityObject ProcessSpawnedAbilityObject(GameObject gameObj, AbilityProperties abilityProp, CoreStatsValuesContainer stats,
        Character character, SkillTemplate skillLogic = null, Vector3 castPoint = default, GameObject target = null, bool activateObject = true, params object[] parameters) {
        if (gameObj == null) return null;

        if (!gameObj.TryGetComponent<AbilityObject>(out var obj)) return null;

        obj.Initialize(
            new CoreAbilityData(character,
            abilityProp,
            abilityProp.GetValuesCopy(),
            stats,
            character.CharacterCombat,
            obj,
            skillLogic,
            castPoint,
            target));

        obj.Initialize(parameters);

        if (activateObject && !obj.gameObject.activeSelf) {
            obj.gameObject.SetActive(true);
        }

        return obj;
    }

    //no params
    public static AbilityObject ProcessSpawnedAbilityObject(GameObject gameObj, CoreAbilityData coreAbilityData, bool activateObject = true) {
        if (gameObj == null || coreAbilityData == null) return null;

        if (!gameObj.TryGetComponent<AbilityObject>(out var obj)) return null;

        coreAbilityData.SetAbilityObject(obj);

        obj.Initialize(coreAbilityData);

        if (activateObject && !obj.gameObject.activeSelf) {
            obj.gameObject.SetActive(true);
        }

        return obj;
    }

    //params
    public static AbilityObject ProcessSpawnedAbilityObject(GameObject gameObj, CoreAbilityData coreAbilityData, bool activateObject = true, params object[] parameters) {
        if (gameObj == null || coreAbilityData == null) return null;

        if (!gameObj.TryGetComponent<AbilityObject>(out var obj)) return null;

        coreAbilityData.SetAbilityObject(obj);

        obj.Initialize(coreAbilityData);
        obj.Initialize(parameters);

        if (activateObject && !obj.gameObject.activeSelf) {
            obj.gameObject.SetActive(true);
        }

        return obj;
    }

    #endregion

    /*public static AbilityObject ProcessSpawnedAbilityObject(GameObject gameObj, SkillTemplate skillTmp) {
        if (gameObj == null) { return null; }

        AbilityObject obj = gameObj.GetComponent<AbilityObject>();

        if (obj == null) { return null; }

        //Assign copy of current character stats to the projectile
        obj.casterStats = skillTmp.characterComponent.CharacterStats.coreStats.GetCopy();
        //Assign copy of current skill properties to the projectile
        obj.abilityProperties = skillTmp.skillProperties.GetCopy<SkillProperties>();
        //Assign the object of projectile to the projectile skill properties - WARNING! on destruction, the object becomes null
        obj.abilityProperties.skillObject = obj.gameObject;
        //sets cast point of the skill
        obj.abilityProperties.castPoint = skillTmp.castPoint;
        //sets target of the skill
        obj.abilityProperties.target = skillTmp.target;
        //sets caster data (data of caster object Player | Enemy)
        obj.abilityProperties.casterHitLayers = skillTmp.casterHitLayers;
        //Assign combat of the object that the projectile is going to use to attack
        obj.sourceCombat = skillTmp.characterComponent.CharacterCombat;

        if (!obj.gameObject.activeSelf) {
            obj.gameObject.SetActive(true);
        }

        return obj;
    }*/

    /*
    /// <summary>
    /// Assigns properties to object of any specified type in the method logic, accepts any type
    /// </summary>
    /// <typeparam name="T">Component with variables that need to be assigned</typeparam>
    /// <typeparam name="U">Component from which the values are taken</typeparam>
    /// <param name="type">Any type</param>
    /// <param name="assignFrom">Any type to assign from</param>
    /// <returns>Object with data assigned</returns>
    public static T AssignPropertiesToSpawnedSkillObject<T, U>(T type, U assignFrom) {
        try {
            AbilityObject abilityObj = (AbilityObject)(object)type;
            if (abilityObj != null) {
                SkillTemplate skillTmp = (SkillTemplate)(object)assignFrom;
                if (skillTmp != null) {
                    //Assign copy of current character stats to the projectile
                    abilityObj.casterStats = skillTmp.characterComponent.CharacterStats.coreStats.GetCopy();
                    //Assign copy of current skill properties to the projectile
                    abilityObj.abilityProperties = skillTmp.skillProperties.GetCopy<SkillProperties>();
                    //Assign the object of projectile to the projectile skill properties - WARNING! on destruction, the object becomes null
                    abilityObj.abilityProperties.skillObject = abilityObj.gameObject;
                    //sets cast point of the skill
                    abilityObj.abilityProperties.castPoint = skillTmp.castPoint;
                    //sets target of the skill
                    abilityObj.abilityProperties.target = skillTmp.target;
                    //sets caster data (data of caster object Player | Enemy)
                    abilityObj.abilityProperties.casterHitLayers = skillTmp.casterHitLayers;
                    //Assign combat of the object that the projectile is going to use to attack
                    abilityObj.sourceCombat = skillTmp.characterComponent.CharacterCombat;

                    return (T)(object)abilityObj;
                }
            }
        } catch (InvalidCastException exc) { Debug.Log(exc); }


        return (T)(object)null;
    }*/
}
