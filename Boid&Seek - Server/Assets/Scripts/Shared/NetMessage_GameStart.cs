using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetMessage_GameStart : NetworkingMessages   //Inheriting from networking messages
{
    //first 8 bits for message IDs
    //rest of bits of size TBD for chat message string
    public Role playerRole { get; set; }

    public enum Role
    {
        Seeker,
        Hidder,
    }


    public NetMessage_GameStart()
    {
        msgID = MessageIDs.GAME_START;
    }

    public NetMessage_GameStart(DataStreamReader reader)
    {
        msgID = MessageIDs.GAME_START;
        Deserialize(reader);
    }

    public NetMessage_GameStart(Role role)
    {
        msgID = MessageIDs.GAME_START;
        playerRole = role;
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)msgID);  //Most space-efficient way of handling messages, could also do write int
        writer.WriteByte((byte)playerRole);
    }

    public override void Deserialize(DataStreamReader reader)   //Read all data from data stream to clear
    {
        //First byte already read on server to handle IDs, so no worries
        playerRole = (Role)reader.ReadByte();
    }

    public override void ReceivedOnServer(ServerScript server)
    {
        //Debug.Log("Server recieved player pos: " + playerXPos + " " + playerZPos + "ID: " + playerIDNum);
    }

    public override void ReceivedOnClient()
    {

    }
}
