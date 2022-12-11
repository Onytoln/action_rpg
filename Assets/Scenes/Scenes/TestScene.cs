using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestScene : MonoBehaviour
{
    public bool sceneLoaded;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) {
            if (!sceneLoaded) {
                SceneManager.LoadScene(3, LoadSceneMode.Additive);
                sceneLoaded = true;
            } else {
                _ = SceneManager.UnloadSceneAsync(3);
                sceneLoaded = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            GameObject gm = GameObject.FindGameObjectWithTag("Fire");
           
            if(gm != null) {
                Transform transf = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
                if (transf != null) {
                    //gm.transform.RotateAround(transf.position, Vector3.up * 5, 5 * Time.deltaTime);
                    gm.transform.position = transf.position;
                } else {
                    Debug.Log("No such transform.");
                }
            } else {
                Debug.Log("No such object.");
            }
        }
    }
}
