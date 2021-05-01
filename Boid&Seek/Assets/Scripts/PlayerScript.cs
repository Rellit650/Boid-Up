using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Networking.Transport;

public class PlayerScript : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public bool m_Done;
    public float m_DRDistance = 100.0f;

    public Text chatMsgText, leaderboardP1Text, leaderboardP2Text;

    [SerializeField]
    private GameObject NetworkedPlayerPrefab;

    private int playerID;

    //private List<GameObject> NetworkedPlayerList =  new List<GameObject>();
    private GameObject[] NetworkedPlayerList = new GameObject[2];
    private Vector3 desiredPos;

    public Material SeekerMat, HiderMat;

    public GameObject NetworkedBoidPrefab;
    private GameObject[] flock;
    private Vector3[] flockNetPositions;

    void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 9000;
        m_Connection = m_Driver.Connect(endpoint);

        StartCoroutine(DelayCommand());
    }

    IEnumerator DelayCommand() 
    {
        yield return new WaitForSeconds(0.7f);
        NetworkingMessages message = new NetMessage_PlayerJoin(0, gameObject.transform.position.x, gameObject.transform.position.y ,gameObject.transform.position.z);
        SendMessage(message);
    }

    public void OnDestroy()
    {
        if(m_Driver.IsCreated)
        {
            m_Driver.Dispose();
        }
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
        HandlePlayerDeadReckoning();
        if(flock != null)
        {
            HandleBoidsDeadReckoning();
        }
    }

    void HandleBoidsDeadReckoning()
    {
        for(int i = 0; i < flock.Length; i++)
        {
            if(Vector3.Distance(flock[i].transform.position, flockNetPositions[i]) < m_DRDistance)
            {
                flock[i].transform.position = Vector3.Lerp(flock[i].transform.position, flockNetPositions[i], 1.0f);
            }
            else
            {
                flock[i].transform.position = flockNetPositions[i];
            }
        }
    }

    void HandlePlayerDeadReckoning() 
    {
        if (playerID == 0)
        {
            if (NetworkedPlayerList[1] != null)
            {
                if (Vector3.Distance(NetworkedPlayerList[1].transform.position, desiredPos) < m_DRDistance)
                {
                    NetworkedPlayerList[1].transform.position = Vector3.Lerp(NetworkedPlayerList[1].transform.position, desiredPos, 0.0625f);
                }
                else
                {
                    NetworkedPlayerList[1].transform.position = desiredPos;
                }    
            }
        }
        else 
        {
            if (NetworkedPlayerList[0] != null)
            {
                if (Vector3.Distance(NetworkedPlayerList[0].transform.position, desiredPos) < m_DRDistance)
                {
                    NetworkedPlayerList[0].transform.position = Vector3.Lerp(NetworkedPlayerList[0].transform.position, desiredPos, 0.0625f);
                    Debug.Log("Lerp");
                }
                else
                {
                    NetworkedPlayerList[0].transform.position = desiredPos;
                    Debug.Log("TP");
                }
            }
        }
        
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
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                HandleMessageTypes(stream);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                m_Connection = default(NetworkConnection);
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
                    NetMessage_Chat castRef = (NetMessage_Chat)message;
                    chatMsgText.text = castRef.chatMsg.ToString();
                    break;
                }

            case MessageIDs.PLAYER_POS_UPDATE:
                {
                    message = new NetMessage_PlayerPos(stream);
                    NetMessage_PlayerPos castRef = (NetMessage_PlayerPos)message;
                    UpdateNetworkedPlayer(castRef.playerIDNum, castRef.playerXPos, castRef.playerYPos, castRef.playerZPos, castRef.playerCompressionScale);
                    break;
                }
            case MessageIDs.PLAYER_JOIN: 
                {
                    message = new NetMessage_PlayerJoin(stream);
                    NetMessage_PlayerJoin castRef = (NetMessage_PlayerJoin)message;
                    GameObject temp = Instantiate(NetworkedPlayerPrefab, new Vector3(castRef.playerXPos, castRef.playerYPos, castRef.playerZPos), Quaternion.identity);
                    //This is for handling other players so we set the id here for them
                    if (playerID == 0)
                    {
                        temp.GetComponent<NetworkedPlayerScript>().playerID = 1;
                        NetworkedPlayerList[1] = temp;
                    }
                    else 
                    {
                        temp.GetComponent<NetworkedPlayerScript>().playerID = 0;
                        NetworkedPlayerList[0] = temp;
                    }
                    
                    //NetworkedPlayerList.Add(temp);                  
                    break;
                }
            case MessageIDs.PLAYER_SETID: 
                {
                    message = new NetMessage_PlayerIDSet(stream);
                    NetMessage_PlayerIDSet castRef = (NetMessage_PlayerIDSet)message;
                    playerID = castRef.playerIDNum;
                    Debug.Log(castRef.playerIDNum);
                    break;
                }
            case MessageIDs.GAME_START: 
                {
                    message = new NetMessage_GameStart(stream);
                    NetMessage_GameStart castRef = (NetMessage_GameStart)message;

                    Debug.Log("Game start call");

                    if (castRef.playerRole == 0)
                    {
                        //Seeker
                        FindObjectOfType<GameStartScript>().SeekerStart(GameObject.FindGameObjectWithTag("Player"));
                        GameObject.FindGameObjectWithTag("Player").GetComponent<MeshRenderer>().material = SeekerMat;
                        //NetworkedPlayerPrefab.GetComponent<MeshRenderer>().material = HiderMat;
                        FindObjectOfType<NetworkedPlayerScript>().GetComponent<MeshRenderer>().material = HiderMat;
                    }
                    else 
                    {
                        //Hider
                        FindObjectOfType<GameStartScript>().HidderStart(GameObject.FindGameObjectWithTag("Player"));
                        GameObject.FindGameObjectWithTag("Player").GetComponent<MeshRenderer>().material = HiderMat;
                        //NetworkedPlayerPrefab.GetComponent<MeshRenderer>().material = SeekerMat;
                        FindObjectOfType<NetworkedPlayerScript>().GetComponent<MeshRenderer>().material = SeekerMat;
                    }
                    break;
                }
            case MessageIDs.CHANGE_ROLE:
                {
                    message = new NetMessage_ChangeRole(stream);
                    NetMessage_ChangeRole castRef = (NetMessage_ChangeRole)message;
                    if(castRef.playerNewRole == 0)
                    {
                        GameObject.FindGameObjectWithTag("Player").GetComponent<MeshRenderer>().material = SeekerMat;
                        //NetworkedPlayerPrefab.GetComponent<MeshRenderer>().material = HiderMat;
                        FindObjectOfType<NetworkedPlayerScript>().GetComponent<MeshRenderer>().material = HiderMat;
                    }
                    else
                    {
                        GameObject.FindGameObjectWithTag("Player").GetComponent<MeshRenderer>().material = HiderMat;
                        //NetworkedPlayerPrefab.GetComponent<MeshRenderer>().material = SeekerMat;
                        FindObjectOfType<NetworkedPlayerScript>().GetComponent<MeshRenderer>().material = SeekerMat;
                    }
                    break;
                }
            case MessageIDs.BOID_SPAWN: 
                {
                    message = new NetMessage_BoidSpawn(stream);

                    NetMessage_BoidSpawn castRef = (NetMessage_BoidSpawn)message;

                    flock = new GameObject[castRef.readBoids.Length];
                    flockNetPositions = new Vector3[castRef.readBoids.Length];

                    for (int i = 0; i < castRef.readBoids.Length; i++) 
                    {
                        flock[i] = Instantiate(NetworkedBoidPrefab,gameObject.transform);
                        flock[i].transform.position = castRef.readBoids[i];
                    }

                    break;
                }
            case MessageIDs.BOID_UPDATE: 
                {
                    message = new NetMessage_BoidUpdate(stream);

                    NetMessage_BoidUpdate castRef = (NetMessage_BoidUpdate)message;
                    Debug.LogWarning("Boid Update");
                    for (int i = 0; i < castRef.readBoids.Length; i++) 
                    {
                        //flock[i].transform.position = castRef.readBoids[i];
                        flockNetPositions[i] = castRef.readBoids[i];
                    }
                    break;
                }
            case MessageIDs.LEADERBOARD_UPDATE:
                {
                    message = new NetMessage_Leaderboard(stream);
                    NetMessage_Leaderboard castRef = (NetMessage_Leaderboard)message;
                    //Set timer text for specific player
                    if(castRef.playerNumber == 0)   //Player 1
                    {
                        leaderboardP1Text.text = "Player 1: " + castRef.playerTime.ToString();
                    }
                    else if(castRef.playerNumber == 1)   //Player 2
                    {
                        leaderboardP2Text.text = "Player 2: " + castRef.playerTime.ToString();
                    }
                    break;
                }
            default:
                {
                    Debug.Log("Recieved message has no ID");
                    break;
                }
        }

        message.ReceivedOnClient();
    }


    public virtual void SendMessage(NetworkingMessages msg)
    {
        DataStreamWriter writer;
        m_Driver.BeginSend(m_Connection, out writer);
        msg.Serialize(ref writer);
        m_Driver.EndSend(writer);
    }

    void UpdateNetworkedPlayer(int nPlayerID, short xPos, short yPos , short zPos, float compressionScale) 
    {
        Debug.Log("update network player");
        for (int i = 0; i < NetworkedPlayerList.Length; i++) 
        {
            if(NetworkedPlayerList[i] != null)
            {
                if (NetworkedPlayerList[i].GetComponent<NetworkedPlayerScript>().playerID == nPlayerID) 
                {
                    Vector3 newPos = Vector3.zero;
                    newPos.x = HubnerDC_Decompression(xPos, compressionScale);
                    newPos.y = HubnerDC_Decompression(yPos, compressionScale);
                    newPos.z = HubnerDC_Decompression(zPos, compressionScale);
                    //NetworkedPlayerList[i].transform.position = newPos;
                    desiredPos = newPos;
                    return;
                }
            }
        }
    }

    public int getPlayerID() 
    {
        return playerID;
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