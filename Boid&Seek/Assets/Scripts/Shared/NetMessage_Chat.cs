using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetMessage_Chat : NetworkingMessages   //Inheriting from networking messages
{
    //first 8 bits for message IDs
    //rest of bits of size TBD for chat message string

    public FixedString128 chatMsg { set; get; }

    public NetMessage_Chat()
    {
        msgID = MessageIDs.CHAT_MSG;
    }

    public NetMessage_Chat(DataStreamReader reader)
    {
        msgID = MessageIDs.CHAT_MSG;
        Deserialize(reader);
    }

    public NetMessage_Chat(string message)
    {
        msgID = MessageIDs.CHAT_MSG;
        chatMsg = message;
    }


    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)msgID);  //Most space-efficient way of handling messages, could also do write int
        writer.WriteFixedString128(chatMsg);
    }

    public override void Deserialize(DataStreamReader reader)   //Read all data from data stream to clear
    {
        //First byte already read on server to handle IDs, so no worries
        chatMsg = reader.ReadFixedString128();
    }

    public override void ReceivedOnServer(ServerScript server)
    {
        Debug.Log("Server recieved message: " + chatMsg);
        server.Broadcast(this); //Broadcasts message recieved to all
    }

    public override void ReceivedOnClient()
    {
        Debug.Log("Client recieved message: " + chatMsg);
    }
}
