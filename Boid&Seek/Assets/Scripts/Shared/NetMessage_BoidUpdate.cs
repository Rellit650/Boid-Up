using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using Unity.Mathematics;

public class NetMessage_BoidUpdate : NetworkingMessages   //Inheriting from networking messages
{
    //first 8 bits for message IDs
    public short numBoids { get; set; }
    public float[] xPos { get; set; }
    public float[] zPos { get; set; }
    public Vector3[] readBoids { get; set; }


    public NetMessage_BoidUpdate()
    {
        msgID = MessageIDs.BOID_UPDATE;
    }

    public NetMessage_BoidUpdate(DataStreamReader reader)
    {
        msgID = MessageIDs.BOID_UPDATE;
        Deserialize(reader);
    }

    public NetMessage_BoidUpdate(Vector3[] boids)
    {
        msgID = MessageIDs.BOID_UPDATE;
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
            half compressX = (half)xPos[i];
            half compressZ = (half)zPos[i];

            writer.WriteUShort(compressX.value);
            writer.WriteUShort(compressZ.value);
            //writer.WriteFloat(xPos[i]);
            //writer.WriteFloat(zPos[i]);
        }
    }

    public override void Deserialize(DataStreamReader reader)   //Read all data from data stream to clear
    {
        //First byte already read on server to handle IDs, so no worries
        numBoids = reader.ReadShort();
        half decompressX, decompressZ;
        readBoids = new Vector3[numBoids];
        for (int i = 0; i < numBoids; i++)
        {
            decompressX.value = reader.ReadUShort();
            decompressZ.value = reader.ReadUShort();
            readBoids[i].x = decompressX;
            readBoids[i].z = decompressZ;
        }
    }

    public override void ReceivedOnClient()
    {
        //Debug.Log("Client recieved player pos: " + playerXPos + " " + playerZPos + "ID: " + playerIDNum);
    }
}
