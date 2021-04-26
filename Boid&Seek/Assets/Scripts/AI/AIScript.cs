using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum Difficulty
{
    easy,
    medium,
    hard
};

public class AIScript : MonoBehaviour
{
    public float moveSpeed = 3f;
    public bool isSeeker = false;
    public Difficulty difficulty;
    public GameObject destination;
    public List<GameObject> allPlayers, allAI, inVision;
    GameObject theSeeker = null;
    NavMeshAgent agent;
    

    public float luckValue, visionRadius = 3;
    float WanderWeight = 1, AIweight = 2;
    Vector2 wanderPointXZ;
    // Start is called before the first frame update
    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();

        //set random rotation so AIs wander in different directions
        transform.forward = new Vector3(Random.Range(-180, 180), 0, Random.Range(-180, 180));

        //find all AI and Players in game
        foreach (GameObject g in GameObject.FindObjectsOfType<GameObject>())
        {
            if (g.gameObject.name.Equals("Player"))
                allPlayers.Add(g.gameObject);
            else if (g.gameObject.name.Equals("AI") && g.gameObject != gameObject)
                allAI.Add(g.gameObject);
        }

        //Detect if a player is the seeker, or set random AI as seeker
        //Player script info not yet implemented
        /*
        foreach (GameObject p in allPlayers)
        {
            if (p.gameObject.GetComponent<PlayerScript>().isSeeker)
                theSeeker = p.gameObject;
        }
        if(theSeeker == null)
        {
            int seeker = Random.Range(0, allAI.Count);
            allAI[seeker].GetComponent<AIScript>().isSeeker = true;
            theSeeker = allAI[seeker];
        }
        */

        //Adjust AI difficulty
        switch (difficulty)
        {
            case Difficulty.easy:
                {
                    luckValue = 5;
                    visionRadius = 5;
                    break;
                }
            case Difficulty.hard:
                {
                    luckValue = 15;
                    visionRadius = 10;
                    break;
                }
            case Difficulty.medium:
            default:
                {
                    luckValue = 10;
                    visionRadius = 7.5f;
                    break;
                }
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckVisionRadius();
        Move();
    }

    void CheckVisionRadius()
    {
        foreach(GameObject p in allPlayers)
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
            AIweight = 0;
        }
        else if (inVision.Count >= 1)
        {
            WanderWeight = 1;
            AIweight = 2;
        }
    }

    void Move()
    {
        Vector3 vel, AImove;
        if (isSeeker)
            AImove = Pursuit().normalized;
        else
            AImove = Flee().normalized;

        vel = (Wander().normalized * WanderWeight) + (AImove * AIweight);
        vel.Normalize();
        vel.y = 1.5f;
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
            pursuit = obj.transform.position - transform.position;

        return new Vector3(pursuit.x, 0, pursuit.z);
    }

    //Run away from all other characters, avoid seeker extra hard
    Vector3 Flee()
    {
        Vector3 flee = Vector3.zero;
        int fleeCount = 0;

        foreach (GameObject p in inVision)
        {
            flee -= p.transform.position;
            ++fleeCount;
            if (theSeeker == p.gameObject)
            {
                flee -= p.transform.position;
                ++fleeCount;
            }
        }

        flee /= (fleeCount);

        return new Vector3(flee.x, 0, flee.z);
    }


}
