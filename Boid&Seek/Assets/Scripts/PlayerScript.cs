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
    public float m_DRDistance = 25.0f;

    public Text chatMsgText, leaderboardP1Text, leaderboardP2Text;

    [SerializeField]
    private GameObject NetworkedPlayerPrefab;

    private int playerID;
    public bool isSeeker;

    //private List<GameObject> NetworkedPlayerList =  new List<GameObject>();
    private GameObject[] NetworkedPlayerList = new GameObject[2];
    private Vector3 desiredPos, startingPosition;

    GameObject player;
    public Material SeekerMat, HiderMat;

    public GameObject NetworkedBoidPrefab;
    public GameObject AIPrefab;
    private GameObject[] flock;
    private List<GameObject> ai;
    private Vector3[] flockNetPositions;
    private List<Vector3> aiNetPositions;

    void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        NetworkEndPoint endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 9000;
        m_Connection = m_Driver.Connect(endpoint);
        /*
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        NetworkEndPoint endpoint;

        if (NetworkEndPoint.TryParse("65.183.134.40", 9000, out endpoint))
        {
            m_Connection = m_Driver.Connect(endpoint);
        }
        */
        player = GameObject.FindGameObjectWithTag("Player");

        ai = new List<GameObject>();
        aiNetPositions = new List<Vector3>();

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

        player.GetComponent<MeshRenderer>().material = isSeeker ? SeekerMat : HiderMat;

        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            if (!m_Done)
                Debug.Log("Something went wrong during connect");
            return;
        }
        HandleMessages();
        HandlePlayerLerpCorrection();
        if (ai != null)
        { 
            HandleAILerpCorrection();
        }
        if (flock != null)
        {
            HandleBoidsLerpCorrection();
        }
    }

    void HandleBoidsLerpCorrection()
    {
        for(int i = 0; i < flock.Length; i++)
        {
            if(Vector3.Distance(flock[i].transform.position, flockNetPositions[i]) < m_DRDistance)
            {
                flock[i].transform.position = Vector3.Lerp(flock[i].transform.position, flockNetPositions[i], 0.05f);
            }
            else
            {
                flock[i].transform.position = flockNetPositions[i];
            }
        }
    }

    void HandleAILerpCorrection()
    {
        for (int i = 0; i < ai.Count; i++)
        {
            if (Vector3.Distance(ai[i].transform.position, aiNetPositions[i]) < m_DRDistance)
            {
                ai[i].transform.position = Vector3.Lerp(ai[i].transform.position, aiNetPositions[i], 0.05f);
            }
            else
            {
                ai[i].transform.position = flockNetPositions[i];
            }
        }
    }

    void HandlePlayerLerpCorrection() 
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
                        player.GetComponent<MeshRenderer>().material = SeekerMat;
                        isSeeker = true;
                        //NetworkedPlayerPrefab.GetComponent<MeshRenderer>().material = HiderMat;
                        //GameObject.FindGameObjectWithTag("NetworkedPlayer").GetComponent<MeshRenderer>().material = HiderMat;
                        ChangeNetworkedPlayerMat(HiderMat);
                    }
                    else 
                    {
                        //Hider
                        FindObjectOfType<GameStartScript>().HidderStart(GameObject.FindGameObjectWithTag("Player"));
                        player.GetComponent<MeshRenderer>().material = HiderMat;
                        isSeeker = false;
                        //NetworkedPlayerPrefab.GetComponent<MeshRenderer>().material = SeekerMat;
                        //GameObject.FindGameObjectWithTag("NetworkedPlayer").GetComponent<MeshRenderer>().material = SeekerMat;
                        ChangeNetworkedPlayerMat(SeekerMat);
                    }
                    break;
                }
            case MessageIDs.CHANGE_ROLE:
                {
                    message = new NetMessage_ChangeRole(stream);
                    NetMessage_ChangeRole castRef = (NetMessage_ChangeRole)message;
                    if(castRef.playerNewRole == 0)
                    {
                        isSeeker = true;
                        GameObject.FindGameObjectWithTag("Player").GetComponent<MeshRenderer>().material = SeekerMat;
                        //NetworkedPlayerPrefab.GetComponent<MeshRenderer>().material = HiderMat;
                        //GameObject.FindGameObjectWithTag("NetworkedPlayer").GetComponent<MeshRenderer>().material = HiderMat;
                        ChangeNetworkedPlayerMat(HiderMat);
                    }
                    else
                    {
                        isSeeker = false;
                        GameObject.FindGameObjectWithTag("Player").GetComponent<MeshRenderer>().material = HiderMat;
                        //NetworkedPlayerPrefab.GetComponent<MeshRenderer>().material = SeekerMat;
                        //GameObject.FindGameObjectWithTag("NetworkedPlayer").GetComponent<MeshRenderer>().material = SeekerMat;
                        ChangeNetworkedPlayerMat(SeekerMat);
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
                    if(flockNetPositions != null)
                    {
                        NetMessage_BoidUpdate castRef = (NetMessage_BoidUpdate)message;
                        for (int i = 0; i < castRef.readBoids.Length; i++) 
                        {
                             //flock[i].transform.position = castRef.readBoids[i];
                            flockNetPositions[i] = castRef.readBoids[i];
                        }
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
            case MessageIDs.ADMIN_COMMAND:
                {
                    message = new NetMessage_AdminCommand(stream);
                    NetMessage_AdminCommand castRef = (NetMessage_AdminCommand)message;

                    HandleCommands(ref castRef);
                    break;
                }
            case MessageIDs.AI_SPAWN:
                {
                    message = new NetMessage_AISpawn(stream);

                    NetMessage_AISpawn castRef = (NetMessage_AISpawn)message;

                    //ai = new GameObject[castRef.readAI.Length];
                    //aiNetPositions = new Vector3[castRef.readAI.Length];
                    aiNetPositions.Add(Vector3.zero);

                    ai.Add(Instantiate(AIPrefab, gameObject.transform));
                    ai[ai.Count - 1].transform.position = castRef.readAI[ai.Count - 1];
                    /*for (int i = 0; i < castRef.readAI.Length; i++)
                    {
                        ai.Add(Instantiate(AIPrefab, gameObject.transform));
                        ai[i].transform.position = castRef.readAI[i];
                    }*/

                    break;
                }
            case MessageIDs.AI_UPDATE:
                {
                    message = new NetMessage_AIUpdate(stream);
                    if (aiNetPositions != null)
                    {
                        NetMessage_AIUpdate castRef = (NetMessage_AIUpdate)message;
                        for (int i = 0; i < castRef.readAI.Length; i++)
                        {
                            //flock[i].transform.position = castRef.readBoids[i];
                            aiNetPositions[i] = castRef.readAI[i];
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

        message.ReceivedOnClient();
    }

    void ChangeNetworkedPlayerMat(Material newMat)
    {
        for (int i = 0; i < NetworkedPlayerList.Length; i++)
        {
            if (NetworkedPlayerList[i] != null)
            {
                //NetworkedPlayerList[i].GetComponent<MeshRenderer>().material = newMat;
                //NetworkedPlayerList[i].GetComponent<MeshRenderer>().material.color = newMat.color;
                NetworkedPlayerList[i].GetComponent<MeshRenderer>().material.SetColor("_Color", newMat.color);
            }
        }
    }


    void HandleCommands(ref NetMessage_AdminCommand command)    //Just figured it should be a reference so it wouldn't make a new net message
    {
        switch (command.adminCommandNumber)
        {
            case 1: //Set speed
                {
                    GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().moveSpeed = command.commandVariable;
                    break;
                }
            case 2: //Set jump force
                {
                    GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().jumpForce = command.commandVariable;
                    break;
                }
            default:
                {
                    Debug.Log("Player command with ID " + command.adminCommandNumber.ToString() + " received");
                    break;
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
                    startingPosition = NetworkedPlayerList[i].transform.position;
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