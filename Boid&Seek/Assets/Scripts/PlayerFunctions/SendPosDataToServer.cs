using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendPosDataToServer : MonoBehaviour
{
    private float sendingTimer;
    public float updateTime = 0.25f;
    private PlayerScript client;
    // Start is called before the first frame update
    void Start()
    {
        client = FindObjectOfType<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - sendingTimer > updateTime)  //If it has been longer than [insert time], send position data to server
        {
            NetMessage_PlayerPos playerPos = new NetMessage_PlayerPos(client.getPlayerID(), transform.position.x, transform.position.z);
            client.SendMessage(playerPos);
            sendingTimer = Time.time;
        }
    }
}
