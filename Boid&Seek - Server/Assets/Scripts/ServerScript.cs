using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Networking.Transport;

public enum gameSize
{
    //VerySmall,
    Small,
    Average,
    Large,
    VeryLarge
}

public class ServerScript : MonoBehaviour
{
    public const int MAX_CONNECTIONS = 2;
    public gameSize sizeOfGame;
    public List<GameObject> flocks;
    public GameObject boid;
    public float spawnRangeX, spawnRangeZ, numBoidsInFlocks, neighborhoodSize, separateRadius, distanceFromCenter, AlignWeight, CohesionWeight, SeparateWeight, ReturnToCenterWeight;

    public NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;
    //private int[] playerIDArray = new int[MAX_CONNECTIONS];
    private GameObject[] playerGameObjectArray = new GameObject[MAX_CONNECTIONS];

    void Start()
    {
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 9000;
        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind to port 9000");
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(MAX_CONNECTIONS, Allocator.Persistent);

        //set playerIds to -1
        //resetPlayerIDs();
        /*
        switch (sizeOfGame)
        {/*
            case gameSize.VerySmall:
                {
                    spawnRangeX = 7;
                    spawnRangeZ = 7;
                    numBoidsInFlocks = 30;
                    neighborhoodSize = 10f;
                    separateRadius = 2.5f;
                    distanceFromCenter = 10f;
                    AlignWeight = 1f;
                    CohesionWeight = 1f;
                    SeparateWeight = 1f;
                    ReturnToCenterWeight = 3f;
                    break;

                }
            case gameSize.Small:
                {
                    spawnRangeX = 8;
                    spawnRangeZ = 8;
                    numBoidsInFlocks = 50;
                    neighborhoodSize = 10f;
                    separateRadius = 2.5f;
                    distanceFromCenter = 15f;
                    AlignWeight = 1f;
                    CohesionWeight = 1f;
                    SeparateWeight = 1f;
                    ReturnToCenterWeight = 3f;
                    break;
                }
            case gameSize.Large:
                {
                    spawnRangeX = 10;
                    spawnRangeZ = 10;
                    numBoidsInFlocks = 90;
                    neighborhoodSize = 10f;
                    separateRadius = 2.5f;
                    distanceFromCenter = 15f;
                    AlignWeight = 1f;
                    CohesionWeight = 1f;
                    SeparateWeight = 1.2f;
                    ReturnToCenterWeight = 3f;
                    break;
                }
            case gameSize.VeryLarge:
                {
                    spawnRangeX = 11;
                    spawnRangeZ = 11;
                    numBoidsInFlocks = 110;
                    neighborhoodSize = 10f;
                    separateRadius = 2.5f;
                    distanceFromCenter = 20f;
                    AlignWeight = 1f;
                    CohesionWeight = 1f;
                    SeparateWeight = 1.3f;
                    ReturnToCenterWeight = 3f;
                    break;
                }
            case gameSize.Average:
            default:
                {
                    spawnRangeX = 9;
                    spawnRangeZ = 9;
                    numBoidsInFlocks = 70;
                    neighborhoodSize = 10f;
                    separateRadius = 2.5f;
                    distanceFromCenter = 15f;
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
        */
    }

    /*
    private void resetPlayerIDs()
    {
        for (uint i = 0; i < playerIDArray.Length; i++)
        {
            playerIDArray[i] = -1;
        }
    }
    */

    public void OnDestroy()
    {
        m_Connections.Dispose();
        m_Driver.Dispose();
    }

    void Update()
    {
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
            HandlePlayerJoin(c);
            Debug.Log("Accepted a connection");

            if (m_Connections.Length >= 2) 
            {
                Debug.Log("Do we get here");
                for (int i = 0; i < playerGameObjectArray.Length; i++) 
                {
                    NetworkingMessages message = new NetMessage_GameStart((NetMessage_GameStart.Role)i);
                    SendMessage(m_Connections[i], message);
                }               
            }
        }
        HandleMessages();
        //HandleFlock();
    }

    void HandlePlayerJoin(NetworkConnection joiner)
    {
        for (int i = 0; i < playerGameObjectArray.Length; i++)
        {
            if (playerGameObjectArray[i] == null)
            {
                //Set GameObject Data later
                playerGameObjectArray[i] = new GameObject();
                NetworkingMessages message = new NetMessage_PlayerIDSet(i);
                SendMessage(joiner, message);
                return;
            }
        }
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
                    HandleMessageTypes(stream, m_Connections[i]);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    m_Connections[i] = default(NetworkConnection);
                    Destroy(playerGameObjectArray[i]); 
                }
            }
        }
    }

    public virtual void HandleMessageTypes(DataStreamReader stream, NetworkConnection sender)
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
                    NetMessage_PlayerPos castRef = (NetMessage_PlayerPos)message;
                    storePlayer(castRef);
                    //broadcast to everyone but ourselves to update me on their clients
                    Broadcast(message, sender);
                    break;
                }

            case MessageIDs.PLAYER_JOIN:
                {
                    message = new NetMessage_PlayerJoin(stream);
                    //Need to figure out how to get all other players as well
                    Broadcast(message, sender);
                    //SpawnAllOtherPlayers

                    NetMessage_PlayerJoin castRef = (NetMessage_PlayerJoin)message;

                    for(int i = 0; i < playerGameObjectArray.Length; i++) //Is i going to be the player ID index? If so, this should work but it feels wrong
                    {
                        //Loop through all connections, send a "Player Joined message back to the new player so that all the current players spawn in
                        if (castRef.playerIDNum != i && playerGameObjectArray[i] != null)   //If the connection is not the sender and it has been created already
                        {
                            NetMessage_PlayerJoin addPlayer = new NetMessage_PlayerJoin(i, playerGameObjectArray[i].transform.position.x, playerGameObjectArray[i].transform.position.y,playerGameObjectArray[i].transform.position.z);
                            SendMessage(sender, addPlayer);
                        }
                    }
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

    /*
    void spawnAllOtherPlayers() 
    {
        for (int i = 0; i < playerGameObjectArray.Length; i++) 
        {
            if (playerGameObjectArray[i] != null) 
            {
                //SendMessage to joiner to make all other players
            }
        }
    }
    */
    void storePlayer(NetMessage_PlayerPos m) 
    {
        if (playerGameObjectArray[m.playerIDNum] != null)
        {
            playerGameObjectArray[m.playerIDNum].transform.position = new Vector3(HubnerDC_Decompression(m.playerXPos, m.playerCompressionScale), HubnerDC_Decompression(m.playerYPos, m.playerCompressionScale), HubnerDC_Decompression(m.playerZPos, m.playerCompressionScale));
        }
        else 
        {
            playerGameObjectArray[m.playerIDNum] = new GameObject();
            playerGameObjectArray[m.playerIDNum].transform.position = new Vector3(HubnerDC_Decompression(m.playerXPos, m.playerCompressionScale), HubnerDC_Decompression(m.playerYPos, m.playerCompressionScale), HubnerDC_Decompression(m.playerZPos, m.playerCompressionScale));
        }
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

    public virtual void Broadcast(NetworkingMessages message, NetworkConnection exclude)
    {
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (m_Connections[i].IsCreated && m_Connections[i] != exclude)
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
    float HubnerDC_Decompression(short position, float compDivisor)
    {
        float decompressed = (float)position;

        //Re-scaling value back to original
        decompressed = decompressed / 511.0f;
        decompressed = decompressed * compDivisor;

        return decompressed;
    }
}