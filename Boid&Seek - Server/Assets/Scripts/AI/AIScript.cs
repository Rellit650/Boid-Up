using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class AIScript : MonoBehaviour
{
    public float moveSpeed = 9f, bonusMoveSpeed = 3f;
    public bool isSeeker = false;
    [HideInInspector]
    public GameObject destination, theServer;
    public List<GameObject> inVision;
    [HideInInspector]
    public List<GameObject> allPlayers, allAI;
    GameObject theSeeker = null;
    NavMeshAgent agent;
    Vector3 wander;
    float luckValue, visionRadius = 200;
    float WanderWeight = 1, AIweight = 2;
    public GameObject obj;
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
        agent.speed = isSeeker ? moveSpeed : moveSpeed + bonusMoveSpeed;
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
        if (Vector2.Distance(new Vector2(destination.transform.position.x, destination.transform.position.z), new Vector2(transform.position.x, transform.position.z)) < 4)
        {
            wander = Vector3.zero;
            wander = new Vector3(Random.Range(-20, 20), 0, Random.Range(-20, 20));
        }
        else
        {
            
        }
        //wander = transform.position + transform.forward + new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y);
        //wander = transform.position + transform.forward + new Vector3(Random.Range(-7, 7), 0, Random.Range(-7, 7));
        wander -= transform.position;
        return new Vector3(wander.x, 0, wander.z);
    }

    //Pursue a character
    Vector3 Pursuit()
    {
        Vector3 pursuit = Vector3.zero;
        //GameObject obj = null;
        if (inVision.Count == 1)
            obj = inVision[0];
        if (inVision.Count > 1)
        {
            obj = findClosest(inVision);
        }
        if (obj != null)
        {
            pursuit = obj.transform.position - transform.position;
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

    GameObject findClosest(List<GameObject> list)
    {
        GameObject closest = list[0];
        Vector2 myPos = new Vector2(transform.position.x, transform.position.z);
        for (int i = 0; i < list.Count; ++i)
        {
            if (Vector2.Distance(new Vector2(closest.transform.position.x, closest.transform.position.z), myPos) > Vector2.Distance(new Vector2(list[i].transform.position.x, list[i].transform.position.z), myPos))
                closest = list[i];
        }

        return closest;
    }
}
