using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using Unity.Mathematics;

public class NetMessage_AIUpdate : NetworkingMessages
{
    //first 8 bits for message IDs
    public short numAI { get; set; }
    public float[] xPos { get; set; }
    public float[] yPos { get; set; }
    public float[] zPos { get; set; }
    public Vector3[] readAI { get; set; }


    public NetMessage_AIUpdate()
    {
        msgID = MessageIDs.AI_UPDATE;
    }

    public NetMessage_AIUpdate(DataStreamReader reader)
    {
        msgID = MessageIDs.AI_UPDATE;
        Deserialize(reader);
    }

    public NetMessage_AIUpdate(Vector3[] AI)
    {
        msgID = MessageIDs.AI_UPDATE;
        numAI = (short)AI.Length;
        xPos = new float[numAI];
        yPos = new float[numAI];
        zPos = new float[numAI];
        //this will cause data loss, that's the point
        for (int i = 0; i < numAI; i++)
        {
            xPos[i] = AI[i].x;
            yPos[i] = AI[i].y;
            zPos[i] = AI[i].z;
        }
    }


    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)msgID);
        writer.WriteShort(numAI);
        //Compressing: CHANGE TO SHORTS SO NEGATIVES WORK
        for (int i = 0; i < numAI; i++)
        {
            half compressX = (half)xPos[i];
            half compressY = (half)yPos[i];
            half compressZ = (half)zPos[i];

            writer.WriteUShort(compressX.value);
            writer.WriteUShort(compressY.value);
            writer.WriteUShort(compressZ.value);
            //writer.WriteFloat(xPos[i]);
            //writer.WriteFloat(zPos[i]);
        }
    }

    public override void Deserialize(DataStreamReader reader)   //Read all data from data stream to clear
    {
        //First byte already read on server to handle IDs, so no worries
        numAI = reader.ReadShort();
        half decompressX, decompressY, decompressZ;
        readAI = new Vector3[numAI];
        for (int i = 0; i < numAI; i++)
        {
            decompressX.value = reader.ReadUShort();
            decompressY.value = reader.ReadUShort();
            decompressZ.value = reader.ReadUShort();
            readAI[i].x = decompressX;
            readAI[i].y = decompressY;
            readAI[i].z = decompressZ;
        }
    }


    public override void ReceivedOnClient()
    {
        //Debug.Log("Client recieved player pos: " + playerXPos + " " + playerZPos + "ID: " + playerIDNum);
    }
}