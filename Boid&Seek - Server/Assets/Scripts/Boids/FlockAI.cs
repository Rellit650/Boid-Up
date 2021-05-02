using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FlockAI : MonoBehaviour
{
    //[HideInInspector]
    public float neighborhoodSize, separateRadius, distanceFromCenter, AlignWeight, CohesionWeight, SeparateWeight, ReturnToCenterWeight, speedMultipier;
    float sqrSepRadius;
    Vector2 center;
    public List<GameObject> neighbors;
    // Start is called before the first frame update
    void Start()
    {
        separateRadius *= (.35f * gameObject.transform.localScale.x);
        sqrSepRadius = separateRadius * separateRadius;
        neighbors = new List<GameObject>();
        center = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        neighbors.Clear();
        Collider[] col = Physics.OverlapSphere(gameObject.transform.position, neighborhoodSize);
        foreach(Collider c in col)
        {
            if (c.gameObject.name.Contains("Boid") && c.gameObject != this.gameObject && c.CompareTag(tag) )
                neighbors.Add(c.gameObject);
        }
    }

    public void flock()
    {
        Vector3 vel;

        if (neighbors.Count > 2)
        {
            vel = (Align().normalized * AlignWeight) + (Cohesion().normalized * CohesionWeight) + (Separate().normalized * SeparateWeight) + (ReturnToCenter().normalized * ReturnToCenterWeight);
            vel.Normalize();

            transform.forward = vel;
            transform.position += vel * Time.deltaTime * speedMultipier;
        }
        else
        {
            vel = ReturnToCenter().normalized;
            transform.forward = vel;
            transform.position += vel * Time.deltaTime * speedMultipier;
        }
    }

    Vector3 Align()
    {
        Vector3 align = Vector3.zero;
        foreach(GameObject boid in neighbors)
        {
            align += boid.transform.forward;
        }
        align /= neighbors.Count;

        return new Vector3(align.x, 0, align.z);
    }

    Vector3 Cohesion()
    {
        Vector3 cohesion = Vector3.zero;
        foreach(GameObject boid in neighbors)
        {
            cohesion += boid.transform.position;
        }
        cohesion /= neighbors.Count;
        cohesion -= new Vector3(transform.position.x, 0, transform.position.z);

        return new Vector3(cohesion.x, 0, cohesion.z);
    }

    Vector3 Separate()
    {
        Vector3 separate = Vector3.zero;
        int numAvoid = 0;
        foreach(GameObject boid in neighbors)
        {
            if(Vector2.SqrMagnitude(new Vector2(transform.position.x, transform.position.z) - new Vector2(boid.transform.position.x, boid.transform.position.z)) < sqrSepRadius)
            {
                ++numAvoid;
                separate += (transform.position - boid.transform.position);
            }
        }
        if(numAvoid > 0)
            separate /= numAvoid;

        return new Vector3(separate.x, 0, separate.z);
    }

    Vector3 ReturnToCenter()
    {
        Vector3 retCent = center - new Vector2(transform.position.x, transform.position.z);
        float t = retCent.magnitude / distanceFromCenter;
        if (t < 0.9f)
            return Vector3.zero;

        return new Vector3(retCent.x, 0, retCent.z);//  * t  * t;
    }
}
