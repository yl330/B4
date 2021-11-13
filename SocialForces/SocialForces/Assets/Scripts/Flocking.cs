using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flocking : MonoBehaviour
{
    public float AlignmentStrength { get; set; }
    public float CohesionStrength { get; set; }
    public float SeparationStrength { get; set; }

    public List<Agent> agents = new List<Agent>();
    private Vector3 Averageposition;
    protected Vector3 AverageForward;
    public float FlockRadius;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 accel = Vector3.zero;
        float accelMultiplier = 0;

        CalculateAverages();

        foreach (Agent agent in agents)
        {
            if (Agent.click == false)
            {
                //agent.rb.velocity = new Vector3(0.5f, 0, 0.5f);

                accel = CalculateAlignmentAcceleration(agent);
                accel += CalculateCohesionAcceleration(agent);
                accel += CalculateSeparationAcceleration(agent);
                accelMultiplier = Parameters.maxSpeed / 2;
                accel *= accelMultiplier * Time.deltaTime;

                agent.rb.velocity += accel;

                if (agent.rb.velocity.magnitude > Parameters.maxSpeed / 2)
                {
                    Vector3.Normalize(agent.rb.velocity);
                    agent.rb.velocity *= Parameters.maxSpeed / 2;
                }
            }
        }
    }


    private void CalculateAverages()
    {
        Vector3 avgPos = Vector3.zero;
        Vector3 avgForward = Vector3.zero;
        for (int i = 0; i < agents.Count; i++)
        {
            avgPos += agents[i].transform.position;
            avgForward.x += Parameters.maxSpeed;
            //avgForward.y += Parameters.maxSpeed;
            avgForward.z += Parameters.maxSpeed;
        }

        Averageposition = avgPos / agents.Count;
        AverageForward = avgForward / agents.Count;


        return;
    }

    private Vector3 CalculateAlignmentAcceleration(Agent agent)
    {
        Vector3 pos = AverageForward / Parameters.maxSpeed;

        if (pos.magnitude > 1)
        {
            Vector3.Normalize(pos);
        }
        Debug.Log("Alignment" + AlignmentStrength);
        return pos * AlignmentStrength;
    }


    private Vector3 CalculateCohesionAcceleration(Agent agent)
    {
        Vector3 pos = Averageposition - agent.transform.position;
        float d = pos.magnitude;
        Vector3.Normalize(pos);

        if (d < FlockRadius)
        {
            pos *= d / FlockRadius;
        }
        Debug.Log("CohesionStrength" + CohesionStrength);
        return pos * CohesionStrength;
    }

    private Vector3 CalculateSeparationAcceleration(Agent agent)
    {
        Vector3 sum = Vector3.zero;

        foreach (Agent otheragent in agents)
        {
            Vector3 pos = agent.transform.position - otheragent.transform.position;
            float d = pos.magnitude;
            float safeDistance = agent.radius + otheragent.radius;
            safeDistance *= 10;
            if (d < safeDistance)
            {
                Vector3.Normalize(pos);
                pos *= (safeDistance - d) / safeDistance;
                sum += pos;
            }
        }

        if (sum.magnitude > 1.0f)
        {
            Vector3.Normalize(sum);
        }

        Debug.Log("SeparationStrenth" + sum*SeparationStrength);
        return sum * SeparationStrength;
    }

}

