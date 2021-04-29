using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Collections;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine.AI;


public enum gameSize
{
    Small,
    Average,
    Large,
}

public class ServerScript : MonoBehaviour
{
    public gameSize sizeOfGame;
    public List<GameObject> flocks;
    public GameObject boid;
    public float spawnRangeX, spawnRangeZ, numBoidsInFlocks, neighborhoodSize, separateRadius, distanceFromCenter, AlignWeight, CohesionWeight, SeparateWeight, ReturnToCenterWeight;

    public NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;


    void Start()
    {
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 9000;
        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind to port 9000");
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);


        switch (sizeOfGame)
        {
            case gameSize.Small:
                {
                    spawnRangeX = 9;
                    spawnRangeZ = 9;
                    numBoidsInFlocks = 100;
                    neighborhoodSize = 10f;
                    separateRadius = 2.4f;
                    distanceFromCenter = 20f;
                    AlignWeight = 1f;
                    CohesionWeight = 1f;
                    SeparateWeight = 1f;
                    ReturnToCenterWeight = 3f;
                    break;
                }
            case gameSize.Large:
                {
                    spawnRangeX = 13;
                    spawnRangeZ = 13;
                    numBoidsInFlocks = 200;
                    neighborhoodSize = 6f;
                    separateRadius = 2.8f;
                    distanceFromCenter = 35f;
                    AlignWeight = .9f;
                    CohesionWeight = .9f;
                    SeparateWeight = 1.2f;
                    ReturnToCenterWeight = 3f;
                    break;
                }
            case gameSize.Average:
            default:
                {
                    spawnRangeX = 11;
                    spawnRangeZ = 11;
                    numBoidsInFlocks = 150;
                    neighborhoodSize = 8f;
                    separateRadius = 2.6f;
                    distanceFromCenter = 27.5f;
                    AlignWeight = 1f;
                    CohesionWeight = 1f;
                    SeparateWeight = 1.1f;
                    ReturnToCenterWeight = 3f;
                    break;
                }
        }
        
        for (int j = 0; j < numBoidsInFlocks; ++j)
        {
            GameObject thing = Instantiate(boid, new Vector3(Random.Range(-spawnRangeX, spawnRangeX), 0, Random.Range(-spawnRangeZ, spawnRangeZ)), Quaternion.identity);
            thing.transform.parent = this.gameObject.transform;
            FlockAI theThingThing = thing.GetComponent<FlockAI>();
            theThingThing.neighborhoodSize = neighborhoodSize;
            theThingThing.separateRadius = separateRadius;
            theThingThing.distanceFromCenter = distanceFromCenter;
            theThingThing.AlignWeight = AlignWeight;
            theThingThing.CohesionWeight = CohesionWeight;
            theThingThing.SeparateWeight = SeparateWeight;
            theThingThing.ReturnToCenterWeight = ReturnToCenterWeight;
            flocks.Add(thing);
        }
        StartCoroutine(RemoveStationaryGround());
    }
    IEnumerator RemoveStationaryGround()
    {
        yield return new WaitForSeconds(3f);
        Destroy(transform.GetChild(0).gameObject);
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
        m_Connections.Dispose();
    }

    void Update()
    {
        gameObject.GetComponent<NavMeshSurface>().BuildNavMesh();
        m_Driver.ScheduleUpdate().Complete();

        // CleanUpConnections
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }
        // AcceptNewConnections
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection))
        {
            m_Connections.Add(c);
            Debug.Log("Accepted a connection");
        }
        HandleMessages();
        HandleFlock();
    }

    void HandleMessages()
    {
        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            Assert.IsTrue(m_Connections[i].IsCreated);

            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    HandleMessageTypes(stream);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    m_Connections[i] = default(NetworkConnection);
                }
            }
        }
    }

    public virtual void HandleMessageTypes(DataStreamReader stream)
    {
        NetworkingMessages message = null;
        MessageIDs msgID = (MessageIDs)stream.ReadByte();
        switch (msgID)
        {
            case MessageIDs.CHAT_MSG:
            {
                message = new NetMessage_Chat(stream);
                break;
            }
            
            case MessageIDs.PLAYER_POS_UPDATE:
            {
                message = new NetMessage_PlayerPos(stream);
                break;
            }
            default:
            {
                Debug.Log("Recieved message has no ID");
                break;
            }
        }

        message.ReceivedOnServer(this);
    }

    public virtual void Broadcast(NetworkingMessages message)
    {
        for(int i = 0; i<m_Connections.Length; i++)
        {
            if(m_Connections[i].IsCreated)
            {
                SendMessage(m_Connections[i], message);
            }
        }
    }

    public virtual void SendMessage(NetworkConnection singleConnection, NetworkingMessages msg)
    {
        DataStreamWriter writer;
        m_Driver.BeginSend(singleConnection, out writer);
        msg.Serialize(ref writer);
        m_Driver.EndSend(writer);
    }


    void HandleFlock()
    {
        for(int i = 0; i < numBoidsInFlocks; ++i)
        {
            flocks[i].GetComponent<FlockAI>().flock();
        }
    }
}