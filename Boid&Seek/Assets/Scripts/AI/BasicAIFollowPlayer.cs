using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;
public class BasicAIFollowPlayer : MonoBehaviour
{
    public NavMeshAgent agent;
    public GameObject player;
    float timeNow = 0f;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        timeNow += Time.deltaTime;
        if(timeNow > 2f)
            agent.SetDestination(player.transform.position);
    }
}
