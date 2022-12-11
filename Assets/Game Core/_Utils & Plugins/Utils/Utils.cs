using MEC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public static class Utils {
    private static Camera _mainCam;
    public static Camera MainCam { get { if (_mainCam == null) _mainCam = Camera.main; return _mainCam; } }

    private static LayerMask _groundMask = 1 << LayerMask.NameToLayer("Ground");
    //movement ability
    private static LayerMask _movementAbilityCollisionLayer = 1 << LayerMask.NameToLayer("CollideWithAllNPC");
    private static readonly int _movementAbilityPositionSearchCount = 6;
    private static readonly float _movementAbilitySearchExtenderTreshold = 4;
    private static readonly float _aboveGroundMaxRange = 5f;

    //spawn pos algorithm
    private static readonly int _perRadiusSearchTries = 8;
    private static readonly float _maxSearchRadius = 25f;
    private static readonly Dictionary<(Vector3 position, LayerMask collisionMask), float> _posSearchBuffer = new Dictionary<(Vector3 position, LayerMask collisionMask), float>();

    private static ObstacleCarveDisabler _obstacleCarveDisabler;
    private static ChargeHandler _chargeHandler;

    private static string _playerTagString = "Player";

    private static HashSet<Action> emulatedUpdates = new HashSet<Action>();
    private static HashSet<IEmulatedUpdate> emulatedUpdatesViaInterface = new HashSet<IEmulatedUpdate>();
    private static CoroutineHandle updateEmulatorHandle;
    private static float _AOBJReleaseYDefaultOffset;

    private static int _mainThreadId;
    public static bool IsMainThread
        => System.Threading.Thread.CurrentThread.ManagedThreadId == _mainThreadId;

    public static readonly string DirectoryFrienlyDateTimeFormat = "yyyy-dd-M HH-mm-ss";

    static Utils() {
        _mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        updateEmulatorHandle = Timing.RunCoroutine(UpdateEmulator());
    }

    public static void InitUtils(LayerMask groundLayer, LayerMask movementAbilityCollisionLayer,
        ObstacleCarveDisabler obstacleCarveDisabler, ChargeHandler chargeHandler, float AOBJReleaseYDefaultOffset) {
        _groundMask = groundLayer;
        _movementAbilityCollisionLayer = movementAbilityCollisionLayer;
        _obstacleCarveDisabler = obstacleCarveDisabler;
        _chargeHandler = chargeHandler;
        _AOBJReleaseYDefaultOffset = AOBJReleaseYDefaultOffset;
    }

    #region Delegate helpers

    public static void SafeInvoke(this Action action) {
        try {
            action.Invoke();
        } catch (Exception exc) {
            Debug.LogError($"SafeInvoke for Action threw an error. Message: {exc.Message}");
        }
    }

    public static void SafeInvoke<T>(this Action<T> action, T item) {
        try {
            action.Invoke(item);
        } catch (Exception exc) {
            Debug.LogError($"SafeInvoke for Action threw an error. Parameter type: {typeof(T)} {Environment.NewLine} Message: {exc.Message}");
        }
    }

    public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 item1, T2 item2) {
        try {
            action.Invoke(item1, item2);
        } catch (Exception exc) {
            Debug.LogError($"SafeInvoke for Action threw an error. Parameters type: {typeof(T1)}, {typeof(T2)} {Environment.NewLine} Message: {exc.Message}");
        }
    }

    public static T SafeInvoke<T>(this Func<T> func) {
        T result = default;

        try {
            result = func.Invoke();
        } catch (Exception exc) {
            Debug.LogError($"SafeInvoke for Action threw an error. Message: {exc.Message}");
        }

        return result;
    }

    #endregion

    #region Raycast helpers 

    public static Vector3 MouseToWorldSpace() {
        Ray ray = MainCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100, _groundMask)) {
            return hit.point;
        }
        return Vector3.zero;
    }

    #endregion

    #region Unity GameObjects and Transforms functions

    public static GameObject GetPlayer() {
        return UnityEngine.GameObject.FindGameObjectWithTag(_playerTagString);
    }

    public static bool IsPrefab(this GameObject obj) {
        if (obj == null) return false;

        return obj.scene == default;
    }

    public static bool IsPrefab(this Component comp) {
        if (comp == null) return false;

        return comp.gameObject.scene == default;
    }

    /// <summary>
    /// Finds gameobject in parent object with certain type and returns it
    /// </summary>
    /// <param name="parent">Parent object</param>
    /// <param name="tag">Tag of desired child object</param>
    /// <returns>Child object with desired tag</returns>
    public static GameObject FindChildObjectInParentByTag(GameObject parent, string tag) {
        Transform t = parent.transform;
        foreach (Transform transform in t) {
            if (transform.CompareTag(tag)) {
                return transform.gameObject;
            }
        }
        return null;
    }

    public static GameObject FindChildObjectInTaggedParentByName(string parentTag, string objectName) {
        Transform parentTransform = GameObject.FindGameObjectWithTag(parentTag).transform;

        foreach (Transform transform in parentTransform) {
            if (transform.name == objectName) {
                return transform.gameObject;
            }
        }
        return null;
    }


    public static Transform[] GetTransformsFromArrayOfComponents<T>(T[] array) where T : Component {
        Transform[] resultArr = new Transform[array.Length];

        for (int i = 0; i < array.Length; i++) {
            if (array[i]?.transform != null) {
                resultArr[i] = array[i].transform;
            }
        }

        return resultArr;
    }

    /// <summary>
    /// Keeps the object above ground based on stated range from 0 to max 5 units
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="aboveGroundRange"></param>
    public static void StayAboveGround(Transform transform, float aboveGroundRange) {
        aboveGroundRange = Mathf.Clamp(aboveGroundRange, 0, _aboveGroundMaxRange);

        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, _aboveGroundMaxRange + 1.1f, _groundMask)) {
            if (hit.distance - 1f == aboveGroundRange) return;

            transform.position -= Vector3.up * (hit.distance - 1f - aboveGroundRange);
        }
    }

    public static T[] GetTypeFromGameMonos<T>() {
        MonoBehaviour[] monos = GameObject.FindObjectsOfType<MonoBehaviour>();

        List<T> result = new List<T>();

        for (int i = 0; i < monos.Length; i++) {
            if (monos[i] is T desired) {
                result.Add(desired);
            }
        }

        return result.ToArray();
    }

    public static T[] GetTypeFromSceneMonos<T>(Scene scene) {
        List<T> result = new List<T>();

        foreach (var gameObj in scene.GetRootGameObjects()) {
            MonoBehaviour[] foundMonos = gameObj.GetComponentsInChildren<MonoBehaviour>();

            foreach (var mono in foundMonos) {
                if (mono is T desired) {
                    result.Add(desired);
                }
            }
        }

        return result.ToArray();
    }

    public static CoroutineHandle GetAllComponentsOfTypeOverTime<T>(List<T> resultList, int waitFrameAfterIterations = 3) {
        if (resultList == null) {
            Debug.LogError("List for GetAllComponentsOfTypeFromScenesOverTime is null");
            return default;
        }

        if (waitFrameAfterIterations <= 0) {
            Debug.LogError($"{nameof(waitFrameAfterIterations)} is lower or equal to 0 for GetAllComponentsOfTypeFromScenesOverTime. This is not allowed.");
            return default;
        }

        return Timing.RunCoroutine(GetAllComponentsOfTypeCoroutine(resultList, waitFrameAfterIterations));
    }

    private static IEnumerator<float> GetAllComponentsOfTypeCoroutine<T>(List<T> resultList, int waitFrameAfterIterations) {
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++) {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);

            var rootObjs = scene.GetRootGameObjects();

            int currentlyChecked = 0;
            foreach (var gameObject in rootObjs) {
                foreach (var comp in gameObject.GetComponentsInChildren<T>(true))
                    resultList.Add(comp);

                currentlyChecked++;
                if (currentlyChecked % waitFrameAfterIterations == 0) {
                    currentlyChecked = 0;
                    yield return Timing.WaitUntilDone(Utils.WaitFrames(1));
                }
            }
        }
    }

    #endregion

    #region Instance helper functions

    public static T CreateInstanceWithParams<T>(params object[] parameters) {
        return (T)Activator.CreateInstance(typeof(T), parameters);
    }

    #endregion

    #region Layer functions

    /// <summary>
    /// Converts LayerMask's bitmask to layer in integer
    /// </summary>
    /// <param name="layerMask">LayerMask to convert (do not use LayerMask.value)</param>
    /// <returns>Layer in integer</returns>
    public static int LayerMaskToLayer(LayerMask layerMask) {
        int bitmask = layerMask.value;

        int result = bitmask > 0 ? 0 : 31;

        while (bitmask > 1) {
            bitmask >>= 1;
            result++;
        }

        return result;
    }

    public static void SetLayerRecursively(this GameObject obj, int layer, bool checkForExclude = false) {
        if (obj == null) {
            return;
        }

        if (checkForExclude) {
            if (!obj.CompareTag("ExcludeFromLayerSet"))
                obj.layer = layer;
        } else {
            obj.layer = layer;
        }

        foreach (Transform child in obj.transform) {
            if (child == null) {
                continue;
            }

            child.gameObject.SetLayerRecursively(layer, checkForExclude);
        }
    }

    #endregion

    #region Quaternion functions

    public static Quaternion DifferenceLocal(this Quaternion to, Quaternion from) {
        return Quaternion.Inverse(from) * to;
    }

    public static Quaternion DifferenceGlobal(this Quaternion to, Quaternion from) {
        return to * Quaternion.Inverse(from);
    }

    public static void AddLocal(this Transform addTo, Quaternion addQuaternion) {
        addTo.rotation *= addQuaternion;
    }

    public static void AddGlobal(this Transform addTo, Quaternion addQuaternion) {
        addTo.rotation = addQuaternion * addTo.rotation;
    }

    /// <summary>
    /// This method is used for rotating projectiles so that they aim at the same height based on cast point,
    /// even when they are not going in the same direction.
    /// </summary>
    /// <param name="transformToRotate">Transform of the object that needs to be rotated</param>
    /// <param name="releaseTransform">Transform on which the object spawned. Only used to get local position y coordinate. 
    /// If not present, default constant for height offset is used (1.25f). Height offset is used to offset the castpoint position vertically,
    /// so that projectiles or similar objects don't fire at the ground directly.</param>
    /// <returns>Return rotation, when multiplying by this rotation you get the correct final rotation of your object</returns>
    public static Quaternion GetXRotationBasedOnCastPoint(Transform transformToRotate, Vector3 castPoint, Transform releaseTransform = null) {

        if (castPoint != Vector3.zero && Vector3.Distance(transformToRotate.position, castPoint) > 2f) {
            Vector3 direction = (castPoint + (Vector3.up *
                (releaseTransform == null ? _AOBJReleaseYDefaultOffset : releaseTransform.transform.localPosition.y))
                - transformToRotate.position).normalized;

            Quaternion rotation = Quaternion.LookRotation(direction);

            Quaternion difference = rotation.DifferenceLocal(transformToRotate.rotation);
            difference.y = 0f;
            difference.z = 0f;

            return difference;
        }

        return Quaternion.identity;

        /*if (castPoint != Vector3.zero && Vector3.Distance(transformToRotate.position, castPoint) > 2) {
           Vector3 direction = (castPoint + (Vector3.up *
               (releaseTransform == null ? _AOBJReleaseYDefaultOffset : releaseTransform.transform.localPosition.y))
               - transformToRotate.position).normalized;

           Quaternion rotation = Quaternion.LookRotation(direction);
           rotation.y = 0;
           rotation.z = 0;
           return rotation;
       }*/
    }

    public static Quaternion GetXZLookRotation(Vector3 target, Vector3 from) {
        Vector3 direction = (target - from).normalized;
        return Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
    }

    public static void SetTransformsYRotationsByTypeCone(Transform[] transforms, Vector3 castPoint, Vector3 castFrom, Quaternion xRot) {
        if (transforms.Length <= 0) { return; }

        float coneDefaultAngleDeg = 90f;
        float coneMinAngleDeg = 30f;

        float maxConeSpreadDistance = 7f;

        float coneCurrentAngleDeg = coneDefaultAngleDeg -
            (Mathf.Clamp(Vector3.Distance(castPoint, castFrom), 0f, maxConeSpreadDistance) / maxConeSpreadDistance
            * (coneDefaultAngleDeg - coneMinAngleDeg));

        float singleTransformRotDeg = coneCurrentAngleDeg / (transforms.Length > 1 ? transforms.Length - 1 : 1);
        float currentAngleDeg = -coneCurrentAngleDeg * 0.5f;

        for (int i = 0; i < transforms.Length; i++) {
            transforms[i].AddLocal(xRot);
            transforms[i].rotation *= Quaternion.AngleAxis(currentAngleDeg, Vector3.up);
            currentAngleDeg += singleTransformRotDeg;
        }
    }

    public static void SetTransformsYRotationsByTypeSpread180(Transform[] transforms, Quaternion xRot, bool resetXRotForUnappliedXRot = false) {
        if (transforms.Length <= 0) { return; }

        float singleTransformRotDeg = 180f / (transforms.Length > 1 ? transforms.Length - 1 : 1);
        float currentAngleDeg = -90f;

        for (int i = 0; i < transforms.Length; i++) {
            if (currentAngleDeg >= -70f && currentAngleDeg <= 70f) {
                transforms[i].AddLocal(xRot);
            } else if (resetXRotForUnappliedXRot) {
                ResetRotationOnAxis(Axis.X, transforms[i]);
            }

            transforms[i].rotation *= Quaternion.AngleAxis(currentAngleDeg, Vector3.up);
            currentAngleDeg += singleTransformRotDeg;
        }
    }

    public static void SetTransformsYRotationsByTypeSpread360(Transform[] transforms, Quaternion xRot, bool resetXRotForUnappliedXRot = false) {
        if (transforms.Length <= 0) { return; }

        float singleTransformRotDeg = 360f / transforms.Length;
        float currentAngleDeg = 0f;

        for (int i = 0; i < transforms.Length; i++) {
            if ((currentAngleDeg >= 0f && currentAngleDeg < 70f) || (currentAngleDeg > 290f && currentAngleDeg <= 360f)) {
                transforms[i].AddLocal(xRot);
            } else if (resetXRotForUnappliedXRot) {
                ResetRotationOnAxis(Axis.X, transforms[i]);
            }

            transforms[i].rotation *= Quaternion.AngleAxis(currentAngleDeg, Vector3.up);

            currentAngleDeg += singleTransformRotDeg;
        }
    }

    public static void ResetRotationOnAxis(Axis axis, Transform transform) {
        switch (axis) {
            case Axis.X:
                transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                return;
            case Axis.Y:
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 0f, transform.rotation.eulerAngles.z);
                return;
            case Axis.Z:
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0f);
                return;
        }

        Debug.LogError("You should not send Axis.None to ResetRotationOnAxis.");
    }

    public static Quaternion GetRandomAxisRotation(Axis axis) {
        switch (axis) {
            case Axis.X:
                return Quaternion.AngleAxis(UnityEngine.Random.Range(0, 361), Vector3.left);
            case Axis.Y:
                return Quaternion.AngleAxis(UnityEngine.Random.Range(0, 361), Vector3.up);
            case Axis.Z:
                return Quaternion.AngleAxis(UnityEngine.Random.Range(0, 361), Vector3.forward);
        }

        Debug.LogError("You should not send Axis.None to GetRandomAxisRotation.");
        return Quaternion.identity;
    }

    #endregion

    #region Unity Animator functions

    public static void ResetAllAnimatorTriggers(Animator anim) {
        foreach (var param in anim.parameters) {
            if (param.type == AnimatorControllerParameterType.Trigger) {
                anim.ResetTrigger(param.name);
            }
        }
    }

    public static void ResetAllAnimatorTriggersExceptExceptions(Animator anim, int[] exceptions = null) {
        AnimatorControllerParameter[] animParameters = anim.parameters;
        if (exceptions != null) {
            for (int i = 0; i < animParameters.Length; i++) {
                int animHash = animParameters[i].nameHash;
                if (animParameters[i].type == AnimatorControllerParameterType.Trigger && !Array.Exists(exceptions, x => x == animHash)) {
                    anim.ResetTrigger(animHash);
                }
            }
        } else {
            for (int i = 0; i < animParameters.Length; i++) {
                if (animParameters[i].type == AnimatorControllerParameterType.Trigger) {
                    anim.ResetTrigger(animParameters[i].name);
                }
            }
        }
    }

    public static float GetClipLengthFromAnimator(Animator anim, string clipName) {
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;

        for (int i = 0; i < clips.Length; i++) {
            if (clips[i].name == clipName) {
                return clips[i].length;
            }
        }

        return 0f;
    }

    #endregion

    #region Unity Assets functions

#if UNITY_EDITOR

    public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
        for (int i = 0; i < guids.Length; i++) {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null) {
                assets.Add(asset);
            }
        }
        return assets;
    }

#endif

    #endregion

    #region Unity find position functions

    public static bool GetMovementAbilityPosition(Vector3 fromPosition, Vector3 desiredPosition, float range, float rangeTolerance,
        out Vector3 position, float collisionRadius = 0.5f, LayerMask? npcCollisionLayer = null) {

        /*Debug.Log(movementAbilityGroundLayer.value);
        Debug.Log(movementAbilityCollisionLayer.value);*/
        /*Debug.Log(fromPosition);
        Debug.Log(desiredPosition);*/

        if (desiredPosition == Vector3.zero) {
            position = Vector3.zero;
            return false;
        }

        LayerMask collisionMask;
        if (npcCollisionLayer == null) {
            collisionMask = _movementAbilityCollisionLayer;
        } else {
            collisionMask = (LayerMask)npcCollisionLayer;
        }
        //Debug.Log(collisionMask.value);

        int searches = _movementAbilityPositionSearchCount;
        float toleratedRangeOffsetStep = range - (range * rangeTolerance);

        if (toleratedRangeOffsetStep > _movementAbilitySearchExtenderTreshold) {
            toleratedRangeOffsetStep /= _movementAbilityPositionSearchCount * 2 + 1;
            searches *= 2;
        } else {
            toleratedRangeOffsetStep /= _movementAbilityPositionSearchCount + 1;
        }

        toleratedRangeOffsetStep /= range;

        Vector3 finalPos = desiredPosition;
        Vector3 direction = (desiredPosition - fromPosition).normalized;
        if (Vector3.Distance(fromPosition, desiredPosition) > range) {
            finalPos = fromPosition + (direction * range);
        }
        finalPos += Vector3.up * 10;

        //Debug.Log(finalPos);
        //Debug.DrawLine(finalPos, finalPos + Vector3.down * 20, Color.blue, 2f);
        //Vector3 test1 = finalPos;
        for (int i = 0; i < searches + 1; i++) {
            //Debug.DrawLine(finalPos, finalPos + Vector3.down * 20, Color.blue, 2f);
            if (Physics.Raycast(finalPos, Vector3.down, out RaycastHit hit, 20f, _groundMask)) {
                if (!Physics.CheckSphere(hit.point, collisionRadius, collisionMask)) {
                    if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 0.1f, NavMesh.AllAreas)) {
                        position = navHit.position;
                        return true;
                    }
                }
            }
            finalPos -= range * toleratedRangeOffsetStep * direction;
        }
        /*Vector3 test2 = finalPos;
        Debug.Log(Vector3.Distance(test1, test2));*/

        position = Vector3.zero;
        return false;
    }

    public static bool GetObjectSpawnPos(Vector3 initialPosition, out Vector3 position, LayerMask collisionLayers, float radius = 0.5f, float searchRadiusGrowth = 1.5f) {
        if (initialPosition == Vector3.zero) {
            position = Vector3.zero;
            return false;
        }

        if (radius <= 0) radius = 0.5f;

        float searchRadius = GetSearchBufferedSearchRadius(initialPosition, collisionLayers);

        Vector3 resultPos = initialPosition;
        bool foundFinalPos = false;

        if (PerformSearch(GetRandomCoordinate(searchRadius, initialPosition, maxRadius: searchRadius + radius), out Vector3 foundPos)) {
            resultPos = foundPos;
            foundFinalPos = true;
        }

        if (!foundFinalPos) {
            while (!foundFinalPos) {
                for (int i = 0; i < _perRadiusSearchTries; i++) {
                    if (PerformSearch(GetRandomCoordinate(searchRadius, initialPosition), out Vector3 foundPosLoopSearch)) {
                        resultPos = foundPosLoopSearch;
                        foundFinalPos = true;
                        break;
                    }
                }

                if (searchRadius > _maxSearchRadius) break;

                if (!foundFinalPos)
                    searchRadius += searchRadiusGrowth;
            }
        }

        var bufferKey = (initialPosition, collisionLayers);

        if (searchRadius > 0f) {
            if (_posSearchBuffer.ContainsKey(bufferKey)) {
                _posSearchBuffer[bufferKey] = searchRadius;
            } else {
                _posSearchBuffer.Add(bufferKey, searchRadius);
            }
        }

        position = resultPos;
        return true;

        bool PerformSearch(Vector3 tryPos, out Vector3 foundPos) {
            if (Physics.Raycast(tryPos, Vector3.down, out RaycastHit hit, 25f, _groundMask)) {
                if (!Physics.CheckSphere(hit.point, radius, collisionLayers)) {
                    if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 0.1f, NavMesh.AllAreas)) {
                        foundPos = navHit.position;
                        return true;
                    }
                }
            }

            foundPos = Vector3.zero;
            return false;
        }

        Vector3 GetRandomCoordinate(float minRadius, Vector3 position, float maxRadius = 0f) {
            return GetRandomPointInAnnulusXZ(position, minRadius, maxRadius == 0f ? minRadius + searchRadiusGrowth : maxRadius) + Vector3.up * 5f;
        }
    }

    private static float GetSearchBufferedSearchRadius(Vector3 position, LayerMask collisionLayer) {
        _ = _posSearchBuffer.TryGetValue((position, collisionLayer), out float foundRadius);
        return foundRadius;
    }

    public static void ClearObjectSpawnSearchBuffer(Vector3 position, LayerMask collisionLayers) {
        ValueTuple<Vector3, LayerMask> bufferKey = (position, collisionLayers);

        if (_posSearchBuffer.ContainsKey(bufferKey)) {
            _posSearchBuffer.Remove(bufferKey);
        }
    }

    /// <summary>
    /// Returns point on walkable navmesh or returns the sent vector that is set to the closest ground
    /// </summary>
    /// <param name="desired"></param>
    /// <returns></returns>
    public static Vector3 GetWalkablePoint(Vector3 desired) {
        if (desired == Vector3.zero) return Vector3.zero;

        if (Physics.Raycast(desired + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 25f, _groundMask)) {
            for (int i = 0; i < 5; i++) {
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit hitNav, 2f, NavMesh.AllAreas)) {
                    return hitNav.position;
                }
            }
        }

        return Vector3.zero;
    }

    #endregion

    #region String functions

    public static string TimeToStringText(float seconds) {
        if (seconds < 0) return string.Empty;

        string result = string.Empty;

        if (seconds > 86400f) {
            result = (int)(seconds / 86400f) + "d";
        } else if (seconds > 3600f) {
            result = ((int)(seconds / 3600)) + "h " + ((int)(seconds % 3600 / 60)) + "m";
        } else if (seconds > 60f) {
            result = (int)(seconds / 60f) + "m " + ((int)(seconds % 60)) + "s";
        } else {
            result = string.Format("{0:0.0}s", seconds);
        }

        return result;
    }

    public static string TimeToStringTextDetailed(float seconds) {
        if (seconds < 0) return string.Empty;

        string result = string.Empty;

        if (seconds > 86400f) {
            result = (int)(seconds / 86400f) + " days" + ((int)(seconds % 86400f / 3600f)) + " hours" + ((int)(seconds % 86400f % 3600f / 60)) + " minutes " +
                ((int)(seconds % 86400f % 3600f % 60f)) + " seconds";
        } else if (seconds > 3600f) {
            result = ((int)(seconds / 3600f)) + " hours " + ((int)(seconds % 3600f / 60f)) + " minutes" + ((int)(seconds % 3600 % 60)) + " seconds";
        } else if (seconds > 60f) {
            result = (int)(seconds / 60f) + " minutes " + ((int)(seconds % 60f)) + " seconds";
        } else {
            result = string.Format("{0:0.0} seconds", seconds);
        }

        return result;
    }

    public static string PadBoth(this string str, int length) {
        int spaces = length - str.Length;
        int padLeft = spaces / 2 + str.Length;
        return str.PadLeft(padLeft).PadRight(length);
    }

    public static int GetHighestFileNumbering(string[] files) {
        int highest = int.MinValue;

        for (int i = 0; i < files.Length; i++) {
            if (!int.TryParse(GetFromStringInBrackets(files[i]), out int current)) continue;

            if (current > highest) {
                highest = current;
            }
        }

        if (highest == int.MinValue) highest = 0;

        return highest;
    }

    public static string GetFromStringInBrackets(string text) {
        if (string.IsNullOrEmpty(text))
            throw new Exception("String sent to GetFromStringInBrackets is null!");

        string[] result = text.Split(new char[] { '(', ')' }, StringSplitOptions.None);

        if (result.Length <= 1) {
            return string.Empty;
        }

        return result[1];
    }

    public static string ToDirectoryFriendlyDateTime(this DateTime dateTime) {
        return dateTime.ToString(DirectoryFrienlyDateTimeFormat);
    }

    #endregion

    #region Custom Math functions

    //Unity

    public static Vector2 GetRandomPointInAnnulus(float minRadius, float maxRadius) {
        return UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minRadius, maxRadius);
    }

    public static Vector2 GetRandomPointInAnnulus(float minRadius, float maxRadius, Vector2 position) {
        return position + UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minRadius, maxRadius);
    }

    public static Vector3 GetRandomPointInAnnulusXZ(Vector3 position, float minRadius, float maxRadius) {
        Vector3 randomPoint = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minRadius, maxRadius);
        randomPoint.z = randomPoint.y;
        randomPoint.y = 0f;
        return randomPoint + position;
    }

    public static void ScaleTransform(Transform transform, Vector3 defaultScaleVector, float currentScale, float primaryScale) {
        if (transform == null) return;
        transform.localScale = DefaultScaling(currentScale, primaryScale) * defaultScaleVector;
    }

    public static float DefaultScaling(float current, float primary) {
        return primary + ((current - primary) / primary);
    }

    //C# in general

    public static int RandomIntMinToMax() {
        return UnityEngine.Random.Range(int.MinValue, int.MaxValue);
    }

    public static float GetAverage(float value, int elements) {
        return value / elements;
    }

    public static float Color32ToVal01(byte value) => Mathf.Clamp01((float)value / byte.MaxValue);

    public static float Color32ValTo01(int value) => Mathf.Clamp01((float)value / byte.MaxValue);

    public static float Color32ValTo01(float value) => Mathf.Clamp01(value / byte.MaxValue);

    public static float CalculateDesiredAnimationSpeedModifier(float animationDuration, float desiredDuration) {
        return animationDuration / desiredDuration;
    }

    public static string RandomString(int length) {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[UnityEngine.Random.Range(0, chars.Length)]).ToArray());
    }

    #endregion

    #region Unity Material functions

    /// <summary>
    /// Destroys material that is replaced!
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="newMaterial"></param>
    public static void ReplaceMainMaterial(Renderer renderer, Material newMaterial) {
        if (renderer == null || newMaterial == null) return;

        UnityEngine.Object.Destroy(renderer.material);

        renderer.material = newMaterial;
    }

    public static void ReplaceMainMaterials(Renderer[] renderers, Material newMaterial) {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++) {
            ReplaceMainMaterial(renderers[i], newMaterial);
        }
    }

    public static void AddMaterial(Renderer renderer, Material material) {
        if (renderer == null || material == null) return;

        Material[] materials = renderer.sharedMaterials;
        Material[] newMaterials = new Material[materials.Length + 1];

        for (int i = 0; i < materials.Length; i++) {
            newMaterials[i] = materials[i];
        }

        newMaterials[newMaterials.Length - 1] = material;

        renderer.materials = newMaterials;
    }

    public static void AddMaterials(Renderer[] renderers, Material material) {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++) {
            AddMaterial(renderers[i], material);
        }
    }

    /// <summary>
    /// Destroys material that is removed!
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="materialname"></param>
    public static void RemoveMaterialByName(Renderer renderer, string materialname) {
        if (renderer == null || string.IsNullOrEmpty(materialname)) return;

        Material[] materials = renderer.sharedMaterials;
        int newSize = materials.Length - 1;
        if (newSize <= 0) return;

        Material[] newMaterials = new Material[newSize];

        int newMatIndex = 0;
        bool materialRemoved = false;

        for (int i = 0; i < materials.Length && newMatIndex < newMaterials.Length; i++, newMatIndex++) {
            if (materialRemoved || !materials[i].name.Contains(materialname)) {
                newMaterials[newMatIndex] = materials[i];
            } else {
                UnityEngine.Object.Destroy(materials[i]);
                newMatIndex--;
                materialRemoved = true;
            }
        }

        renderer.materials = newMaterials;
    }

    /// <summary>
    /// Destroys material that is removed!
    /// </summary>
    /// <param name="renderers"></param>
    /// <param name="materialName"></param>
    public static void RemoveMaterialsByName(Renderer[] renderers, string materialName) {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++) {
            RemoveMaterialByName(renderers[i], materialName);
        }
    }

    public static Material GetMaterialByName(Renderer renderer, string materialName) {
        if (renderer == null || string.IsNullOrEmpty(materialName)) return null;

        Material[] materials = renderer.sharedMaterials;

        for (int i = 0; i < materials.Length; i++) {
            if (materials[i] != null && materials[i].name.Contains(materialName)) return materials[i];
        }

        return null;
    }

    public static Material[] GetMaterialsByName(Renderer[] renderers, string materialName) {
        if (renderers == null) return null;

        Material[] materials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++) {
            materials[i] = GetMaterialByName(renderers[i], materialName);
        }

        return materials;
    }

    /// <summary>
    /// Destroys material that is replaced!
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="material"></param>
    public static void ReplaceMaterial(Renderer renderer, Material material) {
        if (renderer == null || material == null) return;

        Material[] materials = renderer.sharedMaterials;

        for (int i = 0; i < materials.Length; i++) {
            if (materials[i] != null && materials[i].name.Contains(material.name)) {
                UnityEngine.Object.Destroy(materials[i]);
                materials[i] = material;
                break;
            }
        }

        renderer.materials = materials;
    }

    /// <summary>
    /// Destroys material that is replaced!
    /// </summary>
    /// <param name="renderers"></param>
    /// <param name="material"></param>
    public static void ReplaceMaterials(Renderer[] renderers, Material material) {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++) {
            ReplaceMaterial(renderers[i], material);
        }
    }

    public static Material CreateNewMaterialInstance(Material material) {
        if (material == null) return null;

        Material newMaterial = new Material(material);
        newMaterial.name += " (myInstance)";

        return newMaterial;
    }

    public static CoroutineHandle? LerpMaterials(CoroutineHandle? coroutineHandle, Material[] materials, byte lerpTo, float materialLerpSpeed, Action executeOnFinish = null) {
        if (materials == null || materials.Length == 0) return null;

        if (coroutineHandle != null) {
            Timing.KillCoroutines((CoroutineHandle)coroutineHandle);
        }

        CoroutineHandle lerpCoroutine = Timing.RunCoroutine(LerpMaterialsCoroutine(materials, lerpTo, materialLerpSpeed));

        if (executeOnFinish != null) {
            _ = WaitUntilDoneThenExecute(lerpCoroutine, executeOnFinish);
        }

        return lerpCoroutine;
    }

    private static IEnumerator<float> LerpMaterialsCoroutine(Material[] materials, byte lerpTo, float materialLerpSpeed) {
        Color color;
        float time = 0f;

        if (materials[0] == null) {
            Debug.LogError($"Material on position {0} is null in LerpMaterialsCoroutine!");
            yield break;
        }

        float lerpFrom01 = materials[0].color.a;
        float lerpTo01 = Color32ToVal01(lerpTo);

        while (time < 1f) {
            for (int i = 0; i < materials.Length; i++) {
                if (materials[i] == null) {
                    Debug.LogError($"Material on position {i} is null in LerpMaterialsCoroutine!");
                    yield break;
                }

                color = materials[i].color;
                color.a = Mathf.Lerp(lerpFrom01, lerpTo01, time);
                materials[i].color = color;
            }

            time += Time.deltaTime * materialLerpSpeed;

            yield return Timing.WaitForOneFrame;
        }
    }

    [Obsolete]
    public static CoroutineHandle? LerpMaterialsPermanently(Material[] materials, byte minMaterialAlpha, float materialLerpSpeed,
       ValueWrapper<float> time, ValueWrapper<bool> interruptFirstStageLerp, ValueWrapper<float> lerpFrom, ValueWrapper<float> lerpTo) {
        if (materials == null || materials.Length == 0) return null;

        return Timing.RunCoroutine(MaterialsLerpCoroutine(materials, minMaterialAlpha, materialLerpSpeed, time, interruptFirstStageLerp, lerpFrom, lerpTo));
    }

    [Obsolete]
    private static IEnumerator<float> MaterialsLerpCoroutine(Material[] materials, byte minMaterialAlpha, float materialLerpSpeed,
        ValueWrapper<float> time, ValueWrapper<bool> interruptFirstStageLerp, ValueWrapper<float> lerpFrom, ValueWrapper<float> lerpTo) {
        Color color;
        time.Value = 0f;

        float startValLerpTo = Utils.Color32ValTo01(minMaterialAlpha);

        while (time.Value < 1f && !interruptFirstStageLerp.Value) {
            Lerp(0f, startValLerpTo, materials);
            yield return Timing.WaitForOneFrame;
        }

        while (true) {
            if (time.Value < 1f) {
                if (lerpFrom.Value == -1f) lerpFrom.Value = materials[0].color.a;
                Lerp(lerpFrom.Value, lerpTo.Value, materials);
            }

            yield return Timing.WaitForOneFrame;
        }

        void Lerp(float from, float to, Material[] materials) {
            for (int i = 0; i < materials.Length; i++) {
                color = materials[i].color;
                color.a = Mathf.Lerp(from, to, time.Value);
                materials[i].color = color;
            }

            time.Value += Time.deltaTime * materialLerpSpeed;
        }
    }

    [Obsolete]
    public static CoroutineHandle? LerpMaterialsEnding(Renderer[] renderers, Material removeWhenEnd, Material[] materials, float materialLerpSpeed,
           ValueWrapper<bool> interrupt) {
        if (materials == null || materials.Length == 0) return null;

        return Timing.RunCoroutine(MaterialsEndingLerpCoroutine(renderers, removeWhenEnd, materials, materialLerpSpeed, interrupt));
    }

    [Obsolete]
    private static IEnumerator<float> MaterialsEndingLerpCoroutine(Renderer[] renderers, Material removeWhenEnd, Material[] materials, float materialLerpSpeed,
        ValueWrapper<bool> interrupt) {

        Color color;
        float time = 0f;
        float lerpFrom = 0f;

        try {
            lerpFrom = materials[0].color.a;
        } catch (Exception exc) {
            Debug.LogError(exc.Message);
            interrupt.Value = true;
        }

        while (time < 1f && !interrupt.Value) {
            for (int i = 0; i < materials.Length; i++) {
                color = materials[i].color;
                color.a = Mathf.Lerp(lerpFrom, 0f, time);
                materials[i].color = color;
            }

            time += Time.deltaTime * materialLerpSpeed;

            yield return Timing.WaitForOneFrame;
        }

        Utils.RemoveMaterialsByName(renderers, removeWhenEnd.name);
    }

    #endregion

    #region Array functions

    public static bool NullOrEmpty(this Array array) {
        return array == null || array.Length == 0;
    }

    public static (bool wasAdded, int arrayIndexChanged) AddToArrayOnNull<T>(T[] array, T item) where T : class {

        for (int i = 0; i < array.Length; i++) {
            if (array[i] == null) {
                array[i] = item;
                return (true, i);
            }
        }

        return (false, -1);
    }

    public static (bool wasRemoved, int arrayIndexChanged) RemoveFromArray<T>(T[] array, T item) where T : class {

        for (int i = 0; i < array.Length; i++) {
            if (array[i] == item) {
                array[i] = null;
                return (true, i);
            }
        }

        return (false, -1);
    }

    public static T[] CopyArray<T>(T[] array) {
        if (array == null) return null;

        T[] newArray = new T[array.Length];

        for (int i = 0; i < array.Length; i++) {
            newArray[i] = array[i];
        }

        return newArray;
    }

    public static bool ArrayContainsNullValue<T>(T[] array) where T : class {
        if (array == null) return true;

        for (int i = 0; i < array.Length; i++) {
            if (array[i] == null) return true;
        }

        return false;
    }

    public static T[] MergeArrays<T>(T[] array1, T[] array2) {
        if (array1 == null && array2 != null) return array2;
        if (array2 == null && array1 != null) return array1;
        if (array1 == null && array2 == null) return null;

        T[] mergedArr = new T[array1.Length + array2.Length];

        for (int i = 0; i < array1.Length; i++) {
            mergedArr[i] = array1[i];
        }

        int mergedArrIndex = array1.Length;

        for (int i = 0; i < array2.Length; i++, mergedArrIndex++) {
            mergedArr[mergedArrIndex] = array2[i];
        }

        return mergedArr;
    }

    public static T GetOfTypeFromArrayOfObjects<T>(object[] array) {
        if (array == null) {
            throw new NullReferenceException("Array sent to GetFromArrayOfObjects is null.");
        }

        for (int i = 0; i < array.Length; i++) {
            if (array[i].GetType() == typeof(T)) return (T)array[i];
        }

        return default;
    }

    #endregion

    #region HashSet functions

    public static bool AddToHashSetItemCountLimit<T>(HashSet<T> hashset, int limit, T item) {
        if (hashset == null || hashset.Count >= limit) return false;

        hashset.Add(item);

        return true;
    }

    public static float GetHashSetAverage(HashSet<float> hashset) {
        if (hashset == null) return 0f;

        float result = 0f;

        foreach (float val in hashset) {
            result += val;
        }

        return result / hashset.Count;
    }

    #endregion

    #region Dictionary functions
    public static U DictGetAndExecuteOrAddAndExecute<T, U>(Dictionary<T, U> dictionary, T key, Func<U, U> func, out bool hadValue, U defaultValue = default) {
        if (dictionary == null) {
            throw new NullReferenceException("Dictionary sent to DictAddOrExecuteAction is null!");
        }

        if (dictionary.TryGetValue(key, out var value)) {
            hadValue = true;
            return dictionary[key] = func(value);
        }

        U result = func(defaultValue);
        dictionary.Add(key, result);

        hadValue = false;
        return result;
    }

    public static U DictTryGetOrDefault<T, U>(Dictionary<T, U> dict, T key, U defaultVal = default, bool throwErrorIfNotPresent = false) {
        if (dict == null) {
            throw new Exception("Dictionary cannot be null for DictTryGetOrDefault method.");
        }

        if (dict.TryGetValue(key, out U value)) {
            return value;
        }

        if (throwErrorIfNotPresent) {
            throw new Exception("DictTryGetOrDefault call yields no value.");
        }

        return defaultVal;
    }

    #endregion

    #region Dictionary with List as Value functions

    public static bool AddToDictList<T, U>(Dictionary<T, List<U>> dictionary, T key, U item) {
        if (dictionary == null) return false;

        if (dictionary.TryGetValue(key, out var values)) {
            values.Add(item);
        } else {
            dictionary.Add(key, new List<U>() { item });
        }

        return true;
    }

    public static bool AddToDictList<T, U>(Dictionary<T, List<U>> dictionary, T key, U item, out bool listAlreadyPresent) {
        if (dictionary == null) {
            listAlreadyPresent = false;
            return false;
        }

        if (dictionary.TryGetValue(key, out var values)) {
            values.Add(item);
            listAlreadyPresent = true;
        } else {
            dictionary.Add(key, new List<U>() { item });
            listAlreadyPresent = false;
        }

        return true;
    }

    public static bool RemoveFromDictList<T, U>(Dictionary<T, List<U>> dictionary, T key, U item) {
        if (dictionary == null) return false;

        if (!dictionary.TryGetValue(key, out var values)) return false;

        values.Remove(item);
        if (values.Count == 0)
            dictionary.Remove(key);

        return true;
    }

    #endregion

    #region List functions

    public static bool NullOrEmpty<T>(this List<T> list) {
        return list == null || list.Count == 0;
    }

    public static bool GetOfTypeFromList<T, U>(List<U> list, out T ofType)
        where T : U
        where U : class {

        if (list == null) {
            ofType = default;
            return false;
        }

        for (int i = 0; i < list.Count; i++) {
            if (list[i].GetType() == typeof(T)) {
                ofType = (T)list[i];
                return true;
            }
        }

        ofType = default;
        return false;
    }

    public static List<T> CopyListValues<T>(List<T> source) {
        if (source == null) return null;

        List<T> result = new List<T>();

        foreach (T item in source) {
            result.Add(item);
        }

        return result;
    }

    #endregion

    #region Enumerable functions

    public static float Average(IEnumerable<float> collection) {
        float total = 0;
        int count = 0;

        foreach (var value in collection) {
            total += value;
            count++;
        }

        return total / count;
    }

    #endregion

    #region Mec Coroutines Helpers

    public static CoroutineHandle WaitFrames(int frames) {
        return Timing.RunCoroutine(WaitFramesCoroutine(frames));
    }

    private static IEnumerator<float> WaitFramesCoroutine(int frames) {
        for (int i = 0; i < frames; i++) {
            yield return Timing.WaitForOneFrame;
        }
    }

    public static CoroutineHandle? WaitUntilDoneThenExecute(CoroutineHandle waitFor, Action executeOnFinish) {
        if (executeOnFinish == null) return null;

        return Timing.RunCoroutine(WaitUntilDoneThenExecuteCoroutine(waitFor, executeOnFinish));
    }

    private static IEnumerator<float> WaitUntilDoneThenExecuteCoroutine(CoroutineHandle waitFor, Action executeOnFinish) {
        yield return Timing.WaitUntilDone(waitFor, false);
        executeOnFinish.SafeInvoke();
    }

    private static IEnumerator<float> UpdateEmulator() {
        while (true) {
            try {
                foreach (var updateAction in emulatedUpdates) {
                    updateAction();
                }

                foreach (var emulatorInterface in emulatedUpdatesViaInterface) {
                    emulatorInterface.EmulatedUpdate();
                }
            } catch (Exception exc) {
                Debug.LogError($"Emulated update threw an error. Message: {exc.Message}");
            }

            yield return Timing.WaitForOneFrame;
        }
    }

    #region Emulate Update

    public static void EmulateUpdate(Action action) {
        if (action == null) {
            Debug.LogError("Action sent to EmulateUpdate is null.");
            return;
        }

        emulatedUpdates.Add(action);
    }

    public static void StopEmulatingUpdate(Action action) {
        if (action == null) {
            Debug.LogError("Action sent to StopEmulatingUpdate is null.");
            return;
        }

        if (!emulatedUpdates.Remove(action)) {
            Debug.LogError("Action for which you are trying to stop update emulation is not update emulated.");
        }
    }

    public static void EmulateUpdate(IEmulatedUpdate emulatedUpdate) {
        if (emulatedUpdate == null) {
            Debug.LogError("Interface sent to StopEmulatingUpdate is null (has no instance of class).");
            return;
        }

        emulatedUpdatesViaInterface.Add(emulatedUpdate);
    }

    public static void StopEmulatingUpdate(IEmulatedUpdate emulatedUpdate) {
        if (emulatedUpdate == null) {
            Debug.LogError("Interface sent to StopEmulatingUpdate is null (has no instance of class).");
            return;
        }

        if (!emulatedUpdatesViaInterface.Remove(emulatedUpdate)) {
            Debug.LogError("Interface for which you are trying to stop update emulation is not update emulated.");
        }
    }

    public static void EmulateUpdate(GameObject gameObject) {
        if (gameObject == null) {
            Debug.LogError("GameObject sent to StopEmulatingUpdate is null.");
            return;
        }

        var comps = gameObject.GetComponentsInChildren<IEmulatedUpdate>();

        for (int i = 0; i < comps.Length; i++) {
            EmulateUpdate(comps[i]);
        }
    }

    public static void StopEmulatingUpdate(GameObject gameObject) {
        if (gameObject == null) {
            Debug.LogError("GameObject sent to StopEmulatingUpdate is null.");
            return;
        }

        var comps = gameObject.GetComponentsInChildren<IEmulatedUpdate>();

        for (int i = 0; i < comps.Length; i++) {
            StopEmulatingUpdate(comps[i]);
        }
    }


    #endregion

    #endregion

    #region Canvas functions

    public static CoroutineHandle FadeCanvasGroup(CanvasGroup canvasGroup, bool makeVisible, CoroutineHandle coroutineHandle, float speed = 5, bool setProperties = true) {
        Timing.KillCoroutines(coroutineHandle);

        return Timing.RunCoroutine(FadeCanvasGroupCoroutine(canvasGroup, makeVisible, speed, setProperties));
    }

    private static IEnumerator<float> FadeCanvasGroupCoroutine(CanvasGroup canvasGroup, bool makeVisible, float speed, bool setProperties) {
        float target = 0;
        float step = speed;

        if (setProperties) {
            canvasGroup.interactable = !canvasGroup.interactable;
            canvasGroup.blocksRaycasts = !canvasGroup.blocksRaycasts;
        }

        if (makeVisible) {
            target = 1;
            while (canvasGroup.alpha < target) {
                canvasGroup.alpha += step * Time.deltaTime;
                yield return Timing.WaitForOneFrame;
            }
        } else {
            while (canvasGroup.alpha > target) {
                canvasGroup.alpha -= step * Time.deltaTime;
                yield return Timing.WaitForOneFrame;
            }
        }
    }

    public static CoroutineHandle ScaleCanvasRectPreservedAspect(Transform transform, float scaleDuration, float finalScale, CoroutineHandle coroutineHandle) {

        Timing.KillCoroutines(coroutineHandle);

        return Timing.RunCoroutine(ScaleCanvasRectPreservedAspectCoroutine(transform, scaleDuration, finalScale));
    }

    private static IEnumerator<float> ScaleCanvasRectPreservedAspectCoroutine(Transform transform, float scaleDuration, float finalScale) {
        if (transform.localScale.x > finalScale) {
            float step = (transform.localScale.x - finalScale) / scaleDuration;
            while (transform.localScale.x > finalScale) {
                float deltaTime = Time.deltaTime;
                transform.localScale -= new Vector3(step * deltaTime, step * deltaTime, 0f);
                yield return Timing.WaitForOneFrame;
            }
        } else {
            float step = (finalScale - transform.localScale.x) / scaleDuration;
            while (transform.localScale.x < finalScale) {
                float deltaTime = Time.deltaTime;
                transform.localScale += new Vector3(step * deltaTime, step * deltaTime, 0f);
                yield return Timing.WaitForOneFrame;
            }
        }
        transform.localScale = new Vector3(finalScale, finalScale, 1f);
    }

    #endregion

    #region NavMesh Obstacle functions

    public static ObstacleCarveDisabler DisableObstaclesInArea(Vector3 spawnPos, NavMeshAgent deactivator, Transform parent = null) {
        if (deactivator == null) {
            throw new Exception("ObstacleCarveDisabled can't take null deactivator(NavMeshAgent)");
        }

        ObstacleCarveDisabler spawnedDisabler;

        if (parent == null) {
            spawnedDisabler = UnityEngine.Object.Instantiate(_obstacleCarveDisabler, spawnPos, Quaternion.identity);
        } else {
            spawnedDisabler = UnityEngine.Object.Instantiate(_obstacleCarveDisabler, parent);
        }

        spawnedDisabler.PreloadObstacleCarver(deactivator);

        return spawnedDisabler;
    }

    #endregion

    #region Movement functions 

    public static CoroutineHandle? JumpCharacterToLocation(NavMeshAgent agent, Vector3 destination, float jumpTime, Transform objectToAnimate,
            CoroutineHandle? coroutineHandle = null, float jumpHeight = 2f, ValueWrapper<bool> pause = null, ValueWrapper<bool> kill = null, Action onCompleteAction = null) {

        if (destination == Vector3.zero || agent == null || objectToAnimate == null || jumpTime <= 0f) return null;

        if (coroutineHandle != null) {
            Timing.KillCoroutines((CoroutineHandle)coroutineHandle);
        }

        return Timing.RunCoroutine(JumpCharacterToLocationCoroutine(agent, destination, jumpTime, objectToAnimate, jumpHeight, pause, kill, onCompleteAction));
    }

    private static IEnumerator<float> JumpCharacterToLocationCoroutine(NavMeshAgent agent, Vector3 destination, float jumpTime,
            Transform objectToAnimate, float jumpHeight, ValueWrapper<bool> pause, ValueWrapper<bool> kill, Action onCompleteAction) {
        float slerpTime = 0f;
        Transform characterTransform = agent.transform;
        Vector3 startPos = characterTransform.position;
        Vector3 animateStartPos = objectToAnimate.localPosition;
        float timeSpeedModifier = 1f / jumpTime;

        DisableObstaclesInArea(destination, agent);

        while (slerpTime < 1f) {
            if (kill != null && kill.Value) break;
            if (pause != null && pause.Value) {
                yield return Timing.WaitForOneFrame;
                continue;
            }

            if (agent.enabled) agent.enabled = false;

            characterTransform.position = Vector3.Slerp(startPos, destination, slerpTime);

            float yOffset = jumpHeight * 4f * (slerpTime - slerpTime * slerpTime);
            objectToAnimate.localPosition = yOffset * Vector3.up;

            slerpTime += Time.deltaTime * timeSpeedModifier;
            yield return Timing.WaitForOneFrame;
        }

        objectToAnimate.localPosition = animateStartPos;
        onCompleteAction?.Invoke();
        agent.enabled = true;
    }

    public static ChargeHandler ChargeCharacterToLocation(Vector3 destination, NavMeshAgent agent, string collisionLayerName, Action completeAction,
        float chargeDuration, float chargeSpeed, int pierceCount, bool pierceAll, float collisionRadius = 0.5f,
        float stayAboveGroundRange = 0f, float stayAboveGroundCallRate = 0.1f, float raycastCheckRate = 0.1f) {

        if (collisionLayerName == null || agent == null) {
            throw new NullReferenceException("Agent and collisionLayerName cannot be null for charge utility.");
        }

        ChargeHandler ch = UnityEngine.Object.Instantiate(_chargeHandler);
        ch.LoadChargeHandler(destination, agent, collisionLayerName, completeAction, chargeDuration, chargeSpeed, pierceCount, pierceAll,
            collisionRadius, stayAboveGroundRange, stayAboveGroundCallRate, raycastCheckRate);

        return ch;
    }

    #endregion

    #region Particle System functions

    public static void SetParticleSimulationSpeed(float primaryModifier, float currentModifier, ParticleSystem ps, float defaultSpeed) {
        if (ps == null) return;

        float modifier = primaryModifier / currentModifier;
        var particleMain = ps.main;
        particleMain.simulationSpeed = defaultSpeed * modifier;
    }

    public static void SetParticleSystemStopAction(this ParticleSystem ps, ParticleSystemStopAction particleSystemStopAction) {
        if (ps == null) return;

        var main = ps.main;
        main.stopAction = particleSystemStopAction;
    }

    #endregion

    #region Async functions

    public static async Task AwaitCoroutine(CoroutineHandle coroutine) {
        ValueWrapper<bool> isDone = new ValueWrapper<bool>(false);

        Scheduler.ExecuteOnMainThread(() => {
            _ = Timing.RunCoroutine(WaitCoroutine(coroutine, isDone));
        });

        while (!isDone.Value) {
            await Task.Delay(5);
        }
    }

    private static IEnumerator<float> WaitCoroutine(CoroutineHandle coroutine, ValueWrapper<bool> isDone) {
        yield return Timing.WaitUntilDone(coroutine, false);
        isDone.Value = true;
    }

    #endregion

    #region I/O functions

    public static async Task WriteFileAsync(string path, string text, FileMode fileMode = FileMode.Create) {
        byte[] encodedText = Encoding.UTF8.GetBytes(text);

        using (FileStream sourceStream = new FileStream(path,
            fileMode, FileAccess.Write, FileShare.None,
            bufferSize: 4096, useAsync: true)) {
            await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
        };
    }

    public static async Task<string> ReadFileAsync(string path) {
        using (FileStream sourceStream = new FileStream(path,
        FileMode.Open, FileAccess.Read, FileShare.Read,
        bufferSize: 4096, useAsync: true)) {
            StringBuilder sb = new StringBuilder();

            byte[] buffer = new byte[0x1000];
            int numRead;
            while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0) {
                string text = Encoding.UTF8.GetString(buffer, 0, numRead);
                sb.Append(text);
            }

            return sb.ToString();
        }
    }

    public static List<string> GetAllFilesWithString(string path, SearchOption searchOption, params string[] excludeDirectories) {
        return GetAllFilesWithString(path, string.Empty, searchOption, excludeDirectories);
    }

    public static List<string> GetAllFilesWithString(string path, string contains, SearchOption searchOption, params string[] excludeDirectories) {
        List<string> results = new List<string>();

        var files = Directory.GetFiles(path, $"{contains}*", searchOption);

        foreach (var file in files) {
            bool containsDirectory = false;

            foreach (var directory in excludeDirectories) {
                if (file.Contains(directory ?? string.Empty)) {
                    containsDirectory = true;
                }
            }

            if (!containsDirectory) {
                results.Add(file);
            }
        }

        return results;
    }

    #endregion
}

#region Utils enums

public enum Axis { None = -1, X, Y, Z }

#endregion

#region Value Wrappers

public class ValueWrapper<T> {
    public T Value { get; set; }

    public ValueWrapper() { }

    public ValueWrapper(T value) {
        Value = value;
    }
}

#endregion

#region Encapsulated Action

public sealed class EncapsulatedAction<T> {
    private event Action<T> Action;

    public static EncapsulatedAction<T> operator +(EncapsulatedAction<T> action, Action<T> subscriber) {
        action ??= new EncapsulatedAction<T>();

        action.Action -= subscriber;
        action.Action += subscriber;
        return action;
    }

    public static EncapsulatedAction<T> operator -(EncapsulatedAction<T> action, Action<T> subscriber) {
        action ??= new EncapsulatedAction<T>();

        action.Action -= subscriber;
        return action;
    }

    public void Invoke(T arg) {
        Action?.Invoke(arg);
    }
}

public sealed class EncapsulatedAction<T1, T2> {
    private event Action<T1, T2> Action;

    public static EncapsulatedAction<T1, T2> operator +(EncapsulatedAction<T1, T2> action, Action<T1, T2> subscriber) {
        action ??= new EncapsulatedAction<T1, T2>();

        action.Action -= subscriber;
        action.Action += subscriber;
        return action;
    }

    public static EncapsulatedAction<T1, T2> operator -(EncapsulatedAction<T1, T2> action, Action<T1, T2> subscriber) {
        action ??= new EncapsulatedAction<T1, T2>();

        action.Action -= subscriber;
        return action;
    }

    public void Invoke(T1 arg1, T2 arg2) {
        Action?.Invoke(arg1, arg2);
    }
}

public sealed class EncapsulatedAction<T1, T2, T3> {
    private event Action<T1, T2, T3> Action;

    public static EncapsulatedAction<T1, T2, T3> operator +(EncapsulatedAction<T1, T2, T3> action, Action<T1, T2, T3> subscriber) {
        action ??= new EncapsulatedAction<T1, T2, T3>();

        action.Action -= subscriber;
        action.Action += subscriber;
        return action;
    }

    public static EncapsulatedAction<T1, T2, T3> operator -(EncapsulatedAction<T1, T2, T3> action, Action<T1, T2, T3> subscriber) {
        action ??= new EncapsulatedAction<T1, T2, T3>();

        action.Action -= subscriber;
        return action;
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3) {
        Action?.Invoke(arg1, arg2, arg3);
    }
}

#endregion

#region Constrained Queue

public class CountConstrainedQueue<T> : IEnumerable<T> {
    private readonly Queue<T> queue = new Queue<T>();

    public int Capacity { get; private set; }

    public int Count { get => queue.Count; }

    public bool AtMaxCapacity { get => queue.Count >= Capacity; }

    public CountConstrainedQueue(int capacity) {
        Capacity = capacity;
    }

    /// <summary>
    /// Change capacity of queue, return array of items overlapping if the new capacity is lower then current queue items count, else returns null.
    /// </summary>
    /// <param name="newCapacity"></param>
    /// <returns></returns>
    public T[] ChangeCapacity(int newCapacity) {
        Capacity = newCapacity;

        int overlap = Capacity - queue.Count;
        if (overlap < 0) {
            overlap = -overlap;

            T[] values = new T[overlap];
            for (int i = 0; i < overlap; i++) {
                values[i] = queue.Dequeue();
            }

            return values;
        }

        return null;
    }

    public bool Enqueue(T item) {
        if (AtMaxCapacity) return false;

        queue.Enqueue(item);
        return true;
    }

    public T Dequeue() {
        return queue.Dequeue();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    public Queue<T>.Enumerator GetEnumerator() => queue.GetEnumerator();
}

#endregion

#region Constrained List

public class CountConstrainedList<T> : IEnumerable<T> {
    private readonly List<T> list = new List<T>();

    public int MaxItems { get; private set; }

    public int Count => list.Count;

    public bool AtMaxCapacity { get => list.Count >= MaxItems; }

    public CountConstrainedList(int maxItems) {
        MaxItems = maxItems;
    }

    public bool Add(T item) {
        if (AtMaxCapacity) return false;

        list.Add(item);
        return true;
    }

    public void Remove(T item) => list.Remove(item);

    public bool Contains(T item) => list.Contains(item);

    public bool Exists(Predicate<T> predicate) => list.Exists(predicate);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    public List<T>.Enumerator GetEnumerator() => list.GetEnumerator();
}

#endregion

#region Custom Yield Instructions

public class AwaitAsyncOperations : CustomYieldInstruction {

    public override bool keepWaiting => KeepWaiting();

    private IEnumerable<AsyncOperation> operations;

    public AwaitAsyncOperations(AsyncOperation operation) {
        operations = new[] { operation };
    }

    public AwaitAsyncOperations(IEnumerable<AsyncOperation> operations) {
        this.operations = operations;
    }

    private bool KeepWaiting() {
        foreach (var operation in operations) {
            if (!operation.isDone)
                return true;
        }

        return false;
    }
}

public class AwaitTasks : CustomYieldInstruction {

    public override bool keepWaiting => KeepWaiting();

    private readonly IEnumerable<Task> tasks;

    private List<Task> tasksFaulted;

    public AwaitTasks(Task task) {
        tasks = new[] { task };
    }

    public AwaitTasks(IEnumerable<Task> tasks) {
        this.tasks = tasks;
    }

    private bool TaskFaultAlreadyReported(Task task) {
        if (tasksFaulted == null) return false;

        return tasksFaulted.Contains(task);
    }

    private void TaskFaultReport(Task task) {
        tasksFaulted ??= new List<Task>();
        tasksFaulted.Add(task);
    }

    private bool KeepWaiting() {
        bool taskNotDone = false;

        foreach (var task in tasks) {
            if (!task.IsCompleted) {
                taskNotDone = true;
            }

            if (task.IsFaulted && !TaskFaultAlreadyReported(task)) {
                foreach (var ex in task.Exception.InnerExceptions) {
                    Debug.LogError($"Task has failed it's execution. Exception: {ex.Message}");
                }

                TaskFaultReport(task);
            }
        }

        return taskNotDone;
    }
}

public class AwaitCoroutines : CustomYieldInstruction {

    public override bool keepWaiting => !coroutinesDone;

    private readonly List<CoroutineHandle> handles = new List<CoroutineHandle>();
    private bool coroutinesDone;

    public AwaitCoroutines(IEnumerable<IEnumerator<float>> coroutines) {
        foreach (var coroutine in coroutines) {
            handles.Add(Timing.RunCoroutine(coroutine));
        }

        _ = Timing.RunCoroutine(AwaitCoroutinesCoroutine());
    }

    public AwaitCoroutines(IEnumerable<Func<IEnumerator<float>>> coroutines) {
        foreach (var coroutine in coroutines) {
            handles.Add(Timing.RunCoroutine(coroutine?.Invoke()));
        }

        _ = Timing.RunCoroutine(AwaitCoroutinesCoroutine());
    }

    public AwaitCoroutines(IEnumerable<CoroutineHandle> handles) {
        foreach(var handle in handles) {
            this.handles.Add(handle);
        }

        _ = Timing.RunCoroutine(AwaitCoroutinesCoroutine());
    }

    private IEnumerator<float> AwaitCoroutinesCoroutine() {
        coroutinesDone = false;

        foreach (var handle in handles) {
            yield return Timing.WaitUntilDone(handle, false);
        }

        coroutinesDone = true;
    }
}

#endregion