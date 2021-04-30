using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetMessage_ChangeRole : NetworkingMessages
{
    public Role playerNewRole { get; set; }

    public enum Role
    {
        Seeker,
        Hidder,
    }

    public NetMessage_ChangeRole()
    {
        msgID = MessageIDs.CHANGE_ROLE;
    }
    public NetMessage_ChangeRole(DataStreamReader reader)
    {
        msgID = MessageIDs.CHANGE_ROLE;
        Deserialize(reader);
    }
    public NetMessage_ChangeRole(Role newRole)
    {
        msgID = MessageIDs.CHANGE_ROLE;
        playerNewRole = newRole;
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)msgID);
        writer.WriteByte((byte)playerNewRole);
    }

    public override void Deserialize(DataStreamReader reader)   //Read all data from data stream to clear
    {
        //First byte already read on server to handle IDs, so no worries
        playerNewRole = (Role)reader.ReadByte();
    }

    public override void ReceivedOnServer(ServerScript server)
    {
        //Debug.Log("Server recieved player pos: " + playerXPos + " " + playerZPos + "ID: " + playerIDNum);
    }

    public override void ReceivedOnClient()
    {

    }
}
