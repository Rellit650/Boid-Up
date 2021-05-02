using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetMessage_AISpawn : NetworkingMessages   //Inheriting from networking messages
{
    //first 8 bits for message IDs
    //rest of bits of size TBD for chat message string
    public short numAI { get; set; }
    public float[] xPos { get; set; }
    public float[] yPos { get; set; }
    public float[] zPos { get; set; }
    public Vector3[] readAI { get; set; }


    public NetMessage_AISpawn()
    {
        msgID = MessageIDs.AI_SPAWN;
    }

    public NetMessage_AISpawn(DataStreamReader reader)
    {
        msgID = MessageIDs.AI_SPAWN;
        Deserialize(reader);
    }

    public NetMessage_AISpawn(Vector3[] boids)
    {
        msgID = MessageIDs.AI_SPAWN;
        numAI = (short)boids.Length;
        xPos = new float[numAI];
        yPos = new float[numAI];
        zPos = new float[numAI];
        //this will cause data loss, that's the point
        for (int i = 0; i < numAI; i++)
        {
            xPos[i] = boids[i].x;
            zPos[i] = boids[i].z;
        }
    }
    public NetMessage_AISpawn(List<Vector3> ai)
    {
        msgID = MessageIDs.AI_SPAWN;
        numAI = (short)ai.Count;
        xPos = new float[numAI];
        yPos = new float[numAI];
        zPos = new float[numAI];
        //this will cause data loss, that's the point
        for (int i = 0; i < numAI; i++)
        {
            xPos[i] = ai[i].x;
            yPos[i] = ai[i].y;
            zPos[i] = ai[i].z;
        }
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)msgID);  //Most space-efficient way of handling messages, could also do write int
        writer.WriteShort(numAI);
        //Compressing: CHANGE TO SHORTS SO NEGATIVES WORK
        for (int i = 0; i < numAI; i++)
        {
            writer.WriteFloat(xPos[i]);
            writer.WriteFloat(yPos[i]);
            writer.WriteFloat(zPos[i]);
        }
    }

    public override void Deserialize(DataStreamReader reader)   //Read all data from data stream to clear
    {
        //First byte already read on server to handle IDs, so no worries
        numAI = reader.ReadShort();

        readAI = new Vector3[numAI];
        for (int i = 0; i < numAI; i++)
        {
            readAI[i].x = reader.ReadFloat();
            readAI[i].z = reader.ReadFloat();
        }
    }

    public override void ReceivedOnServer(ServerScript server)
    {
        //Debug.Log("Server recieved player pos: " + playerXPos + " " + playerZPos + "ID: " + playerIDNum);
    }

    public override void ReceivedOnClient()
    {
        //Debug.Log("Client recieved player pos: " + playerXPos + " " + playerZPos + "ID: " + playerIDNum);
    }
}
