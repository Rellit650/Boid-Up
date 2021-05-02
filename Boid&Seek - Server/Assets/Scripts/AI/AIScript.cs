using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class AIScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool isSeeker = false;
    [HideInInspector]
    public GameObject destination, theServer;
    public List<GameObject> inVision;
    [HideInInspector]
    public List<GameObject> allPlayers, allAI;
    GameObject theSeeker = null;
    NavMeshAgent agent;
    public float playerDist;

    public float luckValue, visionRadius = 15;
    float WanderWeight = 1, AIweight = 2;
    Vector2 wanderPointXZ;
    // Start is called before the first frame update
    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        theServer = GameObject.Find("PlayerEvents");
        //theServer.GetComponent<ServerScript>().allAI.Add(this.gameObject);
        theServer.GetComponent<ServerScript>().spawnAI(this.gameObject);

        //set random rotation so AIs wander in different directions
        transform.forward = new Vector3(Random.Range(-180, 180), 0, Random.Range(-180, 180));

        //find all AI and Players in game
        updatePlayerCount();
        updateSeeker();
        
    }

    public void updatePlayerCount()
    {
        allPlayers.Clear();
        allAI.Clear();
        foreach (GameObject g in GameObject.FindObjectsOfType<GameObject>())
        {
            if (g.gameObject.name.Equals("New Game Object"))
                allPlayers.Add(g.gameObject);
            else if (g.gameObject.name.Equals("AI") && g.gameObject != gameObject)
                allAI.Add(g.gameObject);
        }
    }

    public void updateSeeker()
    {
        theSeeker = theServer.GetComponent<ServerScript>().allPlayerAndAI[theServer.GetComponent<ServerScript>().currentSeekerIndex];
    }

    // Update is called once per frame
    void Update()
    {
        updatePlayerCount();
        updateSeeker();
        CheckVisionRadius();
        Move();
    }

    void CheckVisionRadius()
    {
        inVision.Clear();
        foreach (GameObject p in allPlayers)
        {
            if(Vector2.Distance(new Vector2(p.transform.position.x, p.transform.position.z), new Vector2(transform.position.x, transform.position.z)) < visionRadius && !inVision.Contains(p))
            {
                inVision.Add(p.gameObject);
            }
        }
        foreach (GameObject ai in allAI)
        {
            if (Vector2.Distance(new Vector2(ai.transform.position.x, ai.transform.position.z), new Vector2(transform.position.x, transform.position.z)) < visionRadius && !inVision.Contains(ai))
            {
                inVision.Add(ai.gameObject);
            }
        }

        if(inVision.Count <= 0)
        {
            WanderWeight = 2;
            AIweight = 1;
        }
        else if (inVision.Count >= 1)
        {
            WanderWeight = 1;
            AIweight = inVision.Count + 1;
        }
    }

    void Move()
    {
        Vector3 vel, AImove;
        if (isSeeker)
            AImove = Pursuit().normalized;
        else
        {
            AImove = Flee().normalized * -1;
        }

        vel = (Wander().normalized * WanderWeight) + (AImove * AIweight);
        vel.Normalize();
        destination.transform.position = transform.position + vel;
        agent.SetDestination(destination.transform.position);
        /*
        transform.forward = vel;
        transform.position += vel * Time.deltaTime;*/
    }

    //Wander around until character is in vision radius
    Vector3 Wander()
    {
        Vector3 wander = Vector3.zero;
        wander = transform.position + transform.forward + new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y);
        //wander = transform.position + transform.forward + new Vector3(Random.Range(-7, 7), 0, Random.Range(-7, 7));
        wander -= transform.position;
        return new Vector3(wander.x, 0, wander.z);
    }

    //Pursue a character
    Vector3 Pursuit()
    {
        Vector3 pursuit = Vector3.zero;
        GameObject obj = null;
        if (inVision.Count >= 1)
            obj = inVision[0];
        if (inVision.Count > 1)
        {
            for (int i = 1; i < inVision.Count - 2; ++i)
            {
                if (Vector3.Distance(obj.transform.position, transform.position) > Vector3.Distance(inVision[i].transform.position, transform.position) )
                    obj = inVision[i];
            }
        }
        if (obj != null)
        {
            pursuit = obj.transform.position - transform.position;
            playerDist = Vector3.Distance(obj.transform.position, gameObject.transform.position);
        }
        

        return new Vector3(pursuit.x, 0, pursuit.z);
    }

    //Run away from all other characters, avoid seeker extra hard
    Vector3 Flee()
    {
        Vector3 flee = Vector3.zero;
        int fleeCount = 0;

        foreach (GameObject p in inVision)
        {
            flee += p.transform.position;
            ++fleeCount;
            if (theSeeker == p.gameObject)
            {
                flee += p.transform.position;
                ++fleeCount;
            }
        }

        flee /= (fleeCount);

        return new Vector3(flee.x, 0, flee.z);
    }


}
