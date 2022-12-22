using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class InputManager : MonoBehaviour, ILoadable {
    private class KeyData {
        public bool keyDown = false;
        public bool keyHeld = false;
        public bool keyUp = false;
    }

    private KeyCode[] keyCodes;
    [SerializeField] private List<KeyCode> excludeKeys;

    private KeyCode[] activeKeys;
    private readonly Dictionary<KeyCode, KeyData> activeKeysData = new Dictionary<KeyCode, KeyData>();
    private readonly HashSet<KeyCode> wasDownOrHeld = new HashSet<KeyCode>();

    //INPUT EVENTS
    public delegate void OnKeyEvent(KeyCode keycode);

    /// <summary>
    /// Event that sends info about which keys were pressed in the current frame
    /// </summary>
    public event OnKeyEvent OnKeyDown;
    /// <summary>
    /// Event that sends info about which keys were held in the current frame
    /// </summary>
    public event OnKeyEvent OnKeyHeld;
    /// <summary>
    /// Event that sends info about which keys were released in the current frame
    /// </summary>
    public event OnKeyEvent OnKeyUp;

    public static InputManager Instance { get; private set; }

    private Dictionary<KeyCode, string> keysStringDic;

    public bool IsLoaded { get; private set; } = false;

    public event Action<ILoadable> OnLoad;

    private void OnValidate() {
        if (excludeKeys == null || excludeKeys.Count == 0) {
            KeyCode[] codes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));

            excludeKeys = new List<KeyCode>();

            for (int i = 0; i < codes.Length; i++) {
                string codeStr = codes[i].ToString();
                if (codeStr.Contains("Joystick")) excludeKeys.Add(codes[i]);
            }
        }
    }

    private void Awake() {
        if (Instance == null) Instance = this;

        GameSceneManager.PreSceneLoadPhase.ExecuteTaskConcurrently(LoadInputsAsync, null, ExecuteAmount.Once, GameStage.AnyStage);
    }

    private Task LoadInputsAsync() => Task.Run(LoadInputs);

    private void LoadInputs() {
        keyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
        List<KeyCode> activeKeys = new List<KeyCode>();

        for (int i = 0; i < keyCodes.Length; i++) {
            if (!activeKeysData.ContainsKey(keyCodes[i]) && !excludeKeys.Contains(keyCodes[i])) {
                activeKeysData.Add(keyCodes[i], new KeyData());
                activeKeys.Add(keyCodes[i]);
            }
        }

        this.activeKeys = activeKeys.ToArray();

        keysStringDic = new Dictionary<KeyCode, string>();

        for (int i = 0; i < keyCodes.Length; i++) {
            if (!keysStringDic.ContainsKey(keyCodes[i])) {
                keysStringDic.Add(keyCodes[i], keyCodes[i].ToString());
            }
        }

        for (int i = 0; i < 10; i++) {
            keysStringDic[(KeyCode)((int)KeyCode.Alpha0 + i)] = i.ToString();
            keysStringDic[(KeyCode)((int)KeyCode.Keypad0 + i)] = i.ToString();
        }

        keysStringDic[KeyCode.Comma] = ",";
        keysStringDic[KeyCode.Escape] = "Esc";
        keysStringDic[KeyCode.CapsLock] = "Caps";
        keysStringDic[KeyCode.Mouse0] = "M1";
        keysStringDic[KeyCode.Mouse1] = "M2";
        keysStringDic[KeyCode.Mouse2] = "M3";
        keysStringDic[KeyCode.Mouse3] = "M4";
        keysStringDic[KeyCode.Mouse4] = "M5";

        Scheduler.ExecuteOnMainThread(() => {
            IsLoaded = true;
            OnLoad?.Invoke(this);
        });
    }

    void Update() {
        bool anyKey = Input.anyKey;

        for (int i = 0; i < activeKeys.Length; i++) {
            KeyCode kc = activeKeys[i];

            var (isDown, keyData) = IsKeyDownOrHeld(kc);
            if (isDown) {
                keyData.keyUp = false;
                continue;
            }

            if (!wasDownOrHeld.Contains(kc)) continue;
            wasDownOrHeld.Remove(kc);

            keyData = activeKeysData[kc];
            keyData.keyUp = Input.GetKeyUp(kc);
            if (keyData.keyUp) {
                OnKeyUp?.Invoke(kc);
            }
        }

        (bool isDown, KeyData keyData) IsKeyDownOrHeld(KeyCode kc) {
            if (!anyKey) return (false, null);

            KeyData kd;

            bool kdDown = Input.GetKeyDown(kc);
            if (kdDown) {
                kd = activeKeysData[kc];
                kd.keyDown = kdDown;
                wasDownOrHeld.Add(kc);
                OnKeyDown?.Invoke(kc);
                InvokeHeld();
                return (true, kd);
            }

            bool kdHeld = Input.GetKey(kc);
            if (kdHeld) {
                kd = activeKeysData[kc];
                kd.keyHeld = kdHeld;
                wasDownOrHeld.Add(kc);
                kd.keyDown = false;
                InvokeHeld();
                return (true, kd);
            }

            return (false, null);

            void InvokeHeld() {
                kd.keyHeld = true;
                OnKeyHeld?.Invoke(kc);
            }
        }
    }

    public bool TwoKeysDown(KeyCode keyCode1, KeyCode keyCode2) {
        return true;
    }

    /// <summary>
    /// Returns true if key was pressed this frame, else false
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsKeyDown(KeyCode key) {
        if (this.activeKeysData.TryGetValue(key, out KeyData keyData)) {
            return keyData.keyDown;
        }

        return false;
    }

    /// <summary>
    /// Return true if key is being held this frame, else false
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsKeyHeld(KeyCode key) {
        if (this.activeKeysData.TryGetValue(key, out KeyData keyData)) {
            return keyData.keyHeld;
        }

        return false;
    }

    /// <summary>
    /// Return true if key was released this frame, else false
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsKeyUp(KeyCode key) {
        if (this.activeKeysData.TryGetValue(key, out KeyData keyData)) {
            return keyData.keyUp;
        }

        return false;
    }

    public string GetKeyCodeAsFormattedString(KeyCode keyCode) {
        if (keysStringDic.TryGetValue(keyCode, out string keyText)) {
            return keyText;
        }

        return "";
    }

}
