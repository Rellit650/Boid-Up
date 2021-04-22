using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetMessage_PlayerIDSet : NetworkingMessages   //Inheriting from networking messages
{
    //first 8 bits for message IDs
    //rest of bits of size TBD for chat message string
    public int playerIDNum { get; set; }

    public NetMessage_PlayerIDSet()
    {
        msgID = MessageIDs.PLAYER_SETID;
    }

    public NetMessage_PlayerIDSet(DataStreamReader reader)
    {
        msgID = MessageIDs.PLAYER_SETID;
        Deserialize(reader);
    }

    public NetMessage_PlayerIDSet(int playerID)
    {
        msgID = MessageIDs.PLAYER_SETID;
        playerIDNum = playerID;
    }


    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)msgID);  //Most space-efficient way of handling messages, could also do write int
        writer.WriteInt(playerIDNum);
    }

    public override void Deserialize(DataStreamReader reader)   //Read all data from data stream to clear
    {
        //First byte already read on server to handle IDs, so no worries
        playerIDNum = reader.ReadInt();
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
