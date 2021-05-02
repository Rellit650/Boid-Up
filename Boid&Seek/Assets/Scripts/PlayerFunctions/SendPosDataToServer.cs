using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendPosDataToServer : MonoBehaviour
{
    private float sendingTimer;
    public float updateTime = 0.07f;
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
            float divisor = HubnerDC_GetDivisor(transform.position);
            Vector3 compressedVec = HubnerDC_Compression(transform.position, divisor);
            NetMessage_PlayerPos playerPos = new NetMessage_PlayerPos(client.getPlayerID(), compressedVec.x, compressedVec.y , compressedVec.z, divisor);
            client.SendMessage(playerPos);
            sendingTimer = Time.time;
        }
    }

    //Data Compression
    float HubnerDC_GetDivisor(Vector3 position)
    {
        float tempValue = 0.0f;
        for (int i = 0; i < 3; i++)
        {
            if (Mathf.Abs(position[i]) > tempValue)
            {
                tempValue = Mathf.Abs(position[i]);
            }
        }
        return tempValue;
    }

    Vector3 HubnerDC_Compression(Vector3 position, float compDivisor)
    {
        Vector3 compressed = Vector3.zero;
        for (int i = 0; i < 3; i++) //Loop through each position
        {
            compressed[i] = (position[i] / compDivisor) * 511.0f;    //Since we are converting this to a short, 511 is the scale we want to use
        }
        return compressed;
    }
}
