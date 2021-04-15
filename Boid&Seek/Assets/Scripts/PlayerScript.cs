using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class PlayerScript : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public bool m_Done;

    void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 9000;
        m_Connection = m_Driver.Connect(endpoint);
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            if (!m_Done)
                Debug.Log("Something went wrong during connect");
            return;
        }
        HandleMessages();
    }

    void HandleMessages()
    {
        //Handle Messages
        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server");
                /*
                uint value = 1;
                DataStreamWriter writer;
                m_Driver.BeginSend(NetworkPipeline.Null, m_Connection, out writer, 0);
                writer.WriteUInt(value);
                m_Driver.EndSend(writer);
                */
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                uint value = stream.ReadUInt();
                Debug.Log("Got the value = " + value + " back from the server");
                m_Done = true;
                m_Connection.Disconnect(m_Driver);
                m_Connection = default(NetworkConnection);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                m_Connection = default(NetworkConnection);
            }
        }
    }

    public virtual void SendMessage(NetworkingMessages msg)
    {
        DataStreamWriter writer;
        m_Driver.BeginSend(m_Connection, out writer);
        msg.Serialize(ref writer);
        m_Driver.EndSend(writer);
    }
}