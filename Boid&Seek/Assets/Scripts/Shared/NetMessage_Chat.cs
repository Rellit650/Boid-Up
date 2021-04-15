using Unity.Networking.Transport;
using UnityEngine;

public class NetMessage_Chat : NetworkingMessages   //Inheriting from networking messages
{
    //first 8 bits for message IDs
    //rest of bits of size TBD for chat message string

    public string chatMsg { set; get; }

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

    public override void Deserialize()
    {

    }
}
