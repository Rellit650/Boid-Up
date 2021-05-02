using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetMessage_AdminCommand : NetworkingMessages   //Inheriting from networking messages
{
    //first 8 bits for message IDs
    public uint adminCommandNumber; //ID for command
    public float commandVariable;   //New variable to be set

    public NetMessage_AdminCommand()
    {
        msgID = MessageIDs.ADMIN_COMMAND;
    }

    public NetMessage_AdminCommand(DataStreamReader reader)
    {
        msgID = MessageIDs.ADMIN_COMMAND;
        Deserialize(reader);
    }

    public NetMessage_AdminCommand(uint commandNumber, float variableChange)
    {
        msgID = MessageIDs.ADMIN_COMMAND;
        adminCommandNumber = commandNumber;
        commandVariable = variableChange;
    }


    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)msgID);  //Most space-efficient way of handling messages, could also do write int
        writer.WriteUInt(adminCommandNumber);
        writer.WriteFloat(commandVariable);
    }

    public override void Deserialize(DataStreamReader reader)   //Read all data from data stream to clear
    {
        //First byte already read on server to handle IDs, so no worries
        adminCommandNumber = reader.ReadUInt();
        commandVariable = reader.ReadFloat();
    }


    public override void ReceivedOnClient()
    {
        //Debug.Log("Client recieved message: " + chatMsg);
        //Display the message on UI
    }
}
