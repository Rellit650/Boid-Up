using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "NetworkedPlayer") 
        {
            NetMessage_Chat message = new NetMessage_Chat("Player detected");
            FindObjectOfType<PlayerScript>().SendMessage(message);
        }
    }
}
