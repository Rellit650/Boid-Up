using Unity.Networking.Transport;
using UnityEngine;
//Don't need much of the unity default stuff
//Just default network message
public class NetworkingMessages
{
    public MessageIDs msgID { set; get; }

    public virtual void Serialize(ref DataStreamWriter writer)
    {

    }
    public virtual void Deserialize()
    {

    }
}
