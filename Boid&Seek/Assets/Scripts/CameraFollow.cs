using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject target;
    // Update is called once per frame
    void Update()
    {
        FollowPlayer();
    }

    void FollowPlayer() 
    {
        Vector3 newPos = Vector3.Lerp(gameObject.transform.position, target.transform.position, 0.0625f);
        newPos.y = gameObject.transform.position.y;
        gameObject.transform.position = newPos;
    }
}
