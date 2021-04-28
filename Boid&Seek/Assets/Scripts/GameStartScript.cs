using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartScript : MonoBehaviour
{
    //public GameObject canvas;
    public float areaSpawningRange = 45f;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    public void SeekerStart(GameObject player) 
    {
        float x = Random.Range(-areaSpawningRange, areaSpawningRange);
        float z = Random.Range(-areaSpawningRange, areaSpawningRange);

        Vector3 newPos = player.transform.position;
        newPos.x = x;
        newPos.z = z;
        player.transform.position = newPos;
        Debug.Log("Seeker Start");
    }
    public void HidderStart(GameObject player) 
    {
        float x = Random.Range(-areaSpawningRange, areaSpawningRange);
        float z = Random.Range(-areaSpawningRange, areaSpawningRange);

        Vector3 newPos = player.transform.position;
        newPos.x = x;
        newPos.z = z;
        player.transform.position = newPos;
        Debug.Log("Hidder Start");
    }
}
