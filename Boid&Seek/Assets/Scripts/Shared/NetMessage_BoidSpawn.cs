using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetMessage_BoidSpawn : NetworkingMessages   //Inheriting from networking messages
{
    //first 8 bits for message IDs
    //rest of bits of size TBD for chat message string
    public short numBoids { get; set; }
    public float[] xPos { get; set; }
    public float[] zPos { get; set; }
    public Vector3[] readBoids { get; set; }


    public NetMessage_BoidSpawn()
    {
        msgID = MessageIDs.BOID_SPAWN;
    }

    public NetMessage_BoidSpawn(DataStreamReader reader)
    {
        msgID = MessageIDs.BOID_SPAWN;
        Deserialize(reader);
    }

    public NetMessage_BoidSpawn(Vector3[] boids)
    {
        msgID = MessageIDs.BOID_SPAWN;
        numBoids = (short)boids.Length;
        xPos = new float[numBoids];
        zPos = new float[numBoids];
        //this will cause data loss, that's the point
        for (int i = 0; i < numBoids; i++)
        {
            xPos[i] = boids[i].x;
            zPos[i] = boids[i].z;
        }
    }


    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)msgID);  //Most space-efficient way of handling messages, could also do write int
        writer.WriteShort(numBoids);
        //Compressing: CHANGE TO SHORTS SO NEGATIVES WORK
        for (int i = 0; i < numBoids; i++)
        {
            writer.WriteFloat(xPos[i]);
            writer.WriteFloat(zPos[i]);
        }
    }

    public override void Deserialize(DataStreamReader reader)   //Read all data from data stream to clear
    {
        //First byte already read on server to handle IDs, so no worries
        numBoids = reader.ReadShort();

        readBoids = new Vector3[numBoids];
        for (int i = 0; i < numBoids; i++)
        {
            readBoids[i].x = reader.ReadFloat();
            readBoids[i].z = reader.ReadFloat();
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