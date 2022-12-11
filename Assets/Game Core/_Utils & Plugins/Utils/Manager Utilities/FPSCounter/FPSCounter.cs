using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour, IFpsProvider
{
    #region Singleton
    public static FPSCounter Instance { get; private set; }

    void Awake() {
        if (Instance == null) Instance = this;
    }

    #endregion

    [SerializeField] int addDeltaTimePerMS = 60;
    [SerializeField] int deltaTimesMaxStoreCount = 50;

    private float[] deltaTimes;
    private int index = 0;

    private float realDeltaTimeRefreshRate;
    private float currentDeltaRefreshTime;

    private float currentFPS;
    public float CurrentFps => currentFPS;

    void Start()
    {
        realDeltaTimeRefreshRate = 1f / addDeltaTimePerMS;
        deltaTimes = new float[deltaTimesMaxStoreCount];
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        currentDeltaRefreshTime += deltaTime;
        if(currentDeltaRefreshTime >= realDeltaTimeRefreshRate) {
            currentDeltaRefreshTime = 0;
            deltaTimes[index] = deltaTime;
            index++;
        }

        if(index >= deltaTimesMaxStoreCount - 1) {
            index = 0;
            CalculateAVGFPS();
        }

        void CalculateAVGFPS() {
            float total = 0f;
            int count = 0;
            for (int i = 0; i < deltaTimes.Length; i++) {
                if (deltaTimes[i] > 0f) { total += deltaTimes[i]; count++; }
                deltaTimes[i] = 0f;
            }

            if(count != 0) currentFPS = 1f / (total / count);
        }
    }

    /*private void OnGUI() {
        GUI.contentColor = Color.green;
        GUI.backgroundColor = Color.black;
        if(currentFPS < 60) {
            GUI.contentColor = Color.red;   
        }
        GUI.Label(new Rect(960, 10, 150, 150), string.Format("FPS: {0:0.00}", currentFPS));
    }*/
}
