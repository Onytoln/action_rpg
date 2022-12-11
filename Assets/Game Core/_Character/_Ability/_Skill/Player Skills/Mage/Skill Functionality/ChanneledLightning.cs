using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChanneledLightning : MonoBehaviour
{
    public GameObject prefab;
    public Transform spawnAt;
    GameObject lightning;
    bool spawned;
  

    // Update is called once per frame
    void Update()
    {
        if (!spawned && Input.GetKeyDown(KeyCode.LeftShift)) {
            lightning = Instantiate(prefab, spawnAt.position, gameObject.transform.rotation, gameObject.transform);
            spawned = true;
        }

        if(spawned && Input.GetKeyUp(KeyCode.LeftShift)) {
            Destroy(lightning);
            spawned = false;
        }
    }
}
