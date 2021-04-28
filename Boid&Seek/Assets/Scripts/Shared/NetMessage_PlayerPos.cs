using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetMessage_PlayerPos : NetworkingMessages   //Inheriting from networking messages
{
    //first 8 bits for message IDs
    //rest of bits of size TBD for chat message string
    public int playerIDNum { get; set; }
    public short playerXPos {get;set;}
    public short playerZPos {get;set;}  //Since Y is vertical, I'm ignoring it for now
    public float playerCompressionScale { get; set; }


    public NetMessage_PlayerPos()
    {
        msgID = MessageIDs.PLAYER_POS_UPDATE;
    }

    public NetMessage_PlayerPos(DataStreamReader reader)
    {
        msgID = MessageIDs.PLAYER_POS_UPDATE;
        Deserialize(reader);
    }

    public NetMessage_PlayerPos(int playerID, float xPos, float zPos, float compression)
    {
        msgID = MessageIDs.PLAYER_POS_UPDATE;
        playerIDNum = playerID;
        //this will cause data loss, that's the point
        playerXPos = (short)xPos;
        playerZPos = (short)zPos;
        playerCompressionScale = compression;
    }


    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)msgID);  //Most space-efficient way of handling messages, could also do write int
        writer.WriteInt(playerIDNum);
        //Compressing: CHANGE TO SHORTS SO NEGATIVES WORK
        writer.WriteShort(playerXPos); //Causing issues with negative numbers here?
        writer.WriteShort(playerZPos);
        writer.WriteFloat(playerCompressionScale);
    }

    public override void Deserialize(DataStreamReader reader)   //Read all data from data stream to clear
    {
        //First byte already read on server to handle IDs, so no worries
        playerIDNum = reader.ReadInt();
        playerXPos = reader.ReadShort();
        playerZPos = reader.ReadShort();
        playerCompressionScale = reader.ReadFloat();
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
