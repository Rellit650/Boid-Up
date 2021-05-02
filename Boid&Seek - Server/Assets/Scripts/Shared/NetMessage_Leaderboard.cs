using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetMessage_Leaderboard : NetworkingMessages   //Inheriting from networking messages
{
    //first 8 bits for message IDs
    public uint playerNumber;
    public float playerTime;

    public NetMessage_Leaderboard()
    {
        msgID = MessageIDs.LEADERBOARD_UPDATE;
    }

    public NetMessage_Leaderboard(DataStreamReader reader)
    {
        msgID = MessageIDs.LEADERBOARD_UPDATE;
        Deserialize(reader);
    }

    public NetMessage_Leaderboard(uint pNumber, float pTime)
    {
        msgID = MessageIDs.LEADERBOARD_UPDATE;
        playerNumber = pNumber;
        playerTime = pTime;
    }


    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)msgID);  //Most space-efficient way of handling messages, could also do write int
        writer.WriteUInt(playerNumber);
        writer.WriteFloat(playerTime);
    }

    public override void Deserialize(DataStreamReader reader)   //Read all data from data stream to clear
    {
        //First byte already read on server to handle IDs, so no worries
        playerNumber = reader.ReadUInt();
        playerTime = reader.ReadFloat();
    }


    public override void ReceivedOnClient()
    {
        //Debug.Log("Client recieved message: " + chatMsg);
    }
}
