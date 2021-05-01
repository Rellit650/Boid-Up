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
    public const int MAX_CONNECTIONS = 2;
    public gameSize sizeOfGame;
    public List<GameObject> flocks;
    public GameObject boid;
    public float spawnRangeX, spawnRangeZ, neighborhoodSize, separateRadius, distanceFromCenter, AlignWeight, CohesionWeight, SeparateWeight, ReturnToCenterWeight;
    public int numBoidsInFlocks;

    public NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;
    //private int[] playerIDArray = new int[MAX_CONNECTIONS];
    private GameObject[] playerGameObjectArray = new GameObject[MAX_CONNECTIONS];
    float tagDistance = 3.0f;
    bool[] playerRoleArray = new bool[MAX_CONNECTIONS];
    float[] playerTimers = new float[MAX_CONNECTIONS];
    bool gameBegin = false;
    uint currentSeekerIndex;

    float leaderboardSendTimer = 0.0f;
    float maxLeaderboardTime = 0.5f;

    float roleTimer = 0.0f;
    float maxTimer = 5.0f;

    float boidTimer = 0.0f;
    float maxBoidTimer = 2.5f;

    void Start()
    {
        m_Driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 9000;
        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind to port 9000");
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(MAX_CONNECTIONS, Allocator.Persistent);

        //set playerIds to -1
        //resetPlayerIDs();
        switch (sizeOfGame)
        {
            case gameSize.Small:
                {
                    spawnRangeX = 20;
                    spawnRangeZ = 20;
                    numBoidsInFlocks = 120;
                    neighborhoodSize = 6f;
                    separateRadius = 2.8f;
                    distanceFromCenter = 40f;
                    AlignWeight = 1f;
                    CohesionWeight = 1f;
                    SeparateWeight = 1.1f;
                    ReturnToCenterWeight = 10f;
                    break;
                }
            case gameSize.Large:
                {
                    spawnRangeX = 30;
                    spawnRangeZ = 30;
                    numBoidsInFlocks = 200;
                    neighborhoodSize = 6f;
                    separateRadius = 2.8f;
                    distanceFromCenter = 30f;
                    AlignWeight = 1f;
                    CohesionWeight = 1f;
                    SeparateWeight = 1.1f;
                    ReturnToCenterWeight = 10f;
                    break;
                }
            case gameSize.Average:
            default:
                {
                    spawnRangeX = 25;
                    spawnRangeZ = 25;
                    numBoidsInFlocks = 160;
                    neighborhoodSize = 6f;
                    separateRadius = 2.8f;
                    distanceFromCenter = 25f;
                    AlignWeight = 1f;
                    CohesionWeight = 1f;
                    SeparateWeight = 1.1f;
                    ReturnToCenterWeight = 10f;
                    break;
                }
        }

        for (int j = 0; j < numBoidsInFlocks; ++j)
        {
            GameObject Boid = Instantiate(boid, new Vector3(Random.Range(-spawnRangeX, spawnRangeX), 0, Random.Range(-spawnRangeZ, spawnRangeZ)), Quaternion.identity);
            Boid.transform.parent = this.gameObject.transform;
            int tag = j % 2;
            switch (tag)
            {
                case 1:
                    {
                        Boid.tag = "Flock 1";
                        break;
                    }
                case 2:
                    {
                        Boid.tag = "Flock 2";
                        break;
                    }
                case 3:
                    {
                        Boid.tag = "Flock 3";
                        break;
                    }
                default:
                    {
                        Boid.tag = "Flock 4";
                        break;
                    }
            }
            //Come on dude
            FlockAI aiComponent = Boid.GetComponent<FlockAI>();
            aiComponent.neighborhoodSize = neighborhoodSize;
            aiComponent.separateRadius = separateRadius;
            aiComponent.distanceFromCenter = distanceFromCenter;
            aiComponent.AlignWeight = AlignWeight;
            aiComponent.CohesionWeight = CohesionWeight;
            aiComponent.SeparateWeight = SeparateWeight;
            aiComponent.ReturnToCenterWeight = ReturnToCenterWeight;
            flocks.Add(Boid);
        }
        //Setting initial timer values. Not sure if needed, but just to be safe
        playerTimers[0] = 0.0f;
        playerTimers[1] = 0.0f;
        StartCoroutine(RemoveStationaryGround());
    }
    IEnumerator RemoveStationaryGround()
    {
        yield return new WaitForSeconds(3f);
        Destroy(transform.GetChild(0).gameObject);
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
        if(roleTimer < maxTimer)
        {
            roleTimer += Time.deltaTime;
        }
        if (boidTimer < maxBoidTimer)
        {
            boidTimer += Time.deltaTime;
        }
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
            HandlePlayerJoin(c);
            Debug.Log("Accepted a connection");

            if (m_Connections.Length >= 2) 
            {
                for (int i = 0; i < playerGameObjectArray.Length; i++) 
                {
                    NetworkingMessages message = new NetMessage_GameStart((NetMessage_GameStart.Role)i);
                    SendMessage(m_Connections[i], message);
                    gameBegin = true;
                }
                //Preliminary roles set (true = seeker)
                playerRoleArray[0] = true;
                playerRoleArray[1] = false;
            }
        }
        SendBoidUpdate();
        HandleMessages();
        HandleFlock();
        if(gameBegin)
        {
            CheckPlayerDistance();
            HandleLeaderboardData(Time.deltaTime);
        }
    }

    void HandleLeaderboardData(float deltaTime)
    {
        leaderboardSendTimer += deltaTime;
        for (int i = 0; i < playerRoleArray.Length; i++)
        {
            if(playerRoleArray[i])  //If the player in this slot is a seeker, increment timer
            {
                playerTimers[i] += deltaTime;
            }
        }
        if(leaderboardSendTimer >= maxLeaderboardTime)
        {
            //Send Leaderboard data
            NetMessage_Leaderboard leaderboardUpdate = new NetMessage_Leaderboard(currentSeekerIndex, playerTimers[currentSeekerIndex]);
            Broadcast(leaderboardUpdate);
            leaderboardSendTimer = 0.0f;
        }
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
                    //Handle spawning other players to correct position
                    for(int i = 0; i < playerGameObjectArray.Length; i++) //Is i going to be the player ID index? If so, this should work but it feels wrong
                    {
                        //Loop through all connections, send a "Player Joined message back to the new player so that all the current players spawn in
                        if (castRef.playerIDNum != i && playerGameObjectArray[i] != null)   //If the connection is not the sender and it has been created already
                        {
                            NetMessage_PlayerJoin addPlayer = new NetMessage_PlayerJoin(i, playerGameObjectArray[i].transform.position.x, playerGameObjectArray[i].transform.position.y,playerGameObjectArray[i].transform.position.z);
                            SendMessage(sender, addPlayer);
                        }
                    }

                    //Handle spawning of flock

                    Vector3[] boidPositions = new Vector3[numBoidsInFlocks]; 

                    for (int i = 0; i < numBoidsInFlocks; i++) 
                    {
                        boidPositions[i] = flocks[i].transform.position;
                    }

                    NetMessage_BoidSpawn addBoids = new NetMessage_BoidSpawn(boidPositions);
                    SendMessage(sender, addBoids);

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

    void SendBoidUpdate() 
    {
        if (boidTimer >= maxBoidTimer) 
        {
            boidTimer = 0.0f;
            Debug.LogWarning("Boid Update: " + boidTimer);
            for (int i = 0; i < MAX_CONNECTIONS; i++) 
            {
                if (m_Connections[i] != null)
                {
                    Vector3[] boidPositions = new Vector3[numBoidsInFlocks];

                    for (int j = 0; j < numBoidsInFlocks; j++)
                    {
                        boidPositions[j] = flocks[j].transform.position;
                    }
                    NetMessage_BoidUpdate boidUpdate = new NetMessage_BoidUpdate(boidPositions);
                    SendMessage(m_Connections[i], boidUpdate);
                }
            }
           
        }
    }

    void CheckPlayerDistance()
    {
        if(Vector3.Distance(playerGameObjectArray[0].transform.position, playerGameObjectArray[1].transform.position) <= tagDistance && roleTimer >= maxTimer)
        {
            Debug.Log("Tag!");
            for(int i = 0; i < MAX_CONNECTIONS; i++)
            {
                //If seeker (true), set to hider (false), and vice versa
                playerRoleArray[i] = !playerRoleArray[i];
                if(playerRoleArray[i])
                {
                    //Send change to new seeker
                    NetMessage_ChangeRole newRole = new NetMessage_ChangeRole(NetMessage_ChangeRole.Role.Seeker);
                    SendMessage(m_Connections[i], newRole);
                    currentSeekerIndex = (uint)i;
                }
                else
                {
                    //Send change to new hider
                    NetMessage_ChangeRole newRole = new NetMessage_ChangeRole(NetMessage_ChangeRole.Role.Hidder);
                    SendMessage(m_Connections[i], newRole);
                }
            }
            roleTimer = 0.0f;
        }
    }
}