﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    public float radius;
    public float mass;
    public float perceptionRadius;

    private List<Vector3> path;
    private NavMeshAgent nma;
    private Rigidbody rb;

    public bool flag;
    private int index;
    public bool modify;


    private HashSet<GameObject> perceivedNeighbors = new HashSet<GameObject>();

    public static bool click;

    void Start()
    {
<<<<<<< Updated upstream
        modify = true;
=======
        click = false;
>>>>>>> Stashed changes
        path = new List<Vector3>();
        nma = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        gameObject.transform.localScale = new Vector3(2 * radius, 1, 2 * radius);
        nma.radius = radius;
        rb.mass = mass;
        GetComponent<SphereCollider>().radius = perceptionRadius / 2;
    }

    private void Update()
    {
        /*if (path.Count > 1 && Vector3.Distance(transform.position, path[0]) < 1.1f)
        {
            path.RemoveAt(0);
        } else if (path.Count == 1 && Vector3.Distance(transform.position, path[0]) < 2f)
        {
            path.RemoveAt(0);

            if (path.Count == 0)
            {
                gameObject.SetActive(false);
                AgentManager.RemoveAgent(gameObject);
            }
        }*/
        if(path[index].x==rb.position.x &&path[index].y==rb.position.y && path[index].z == rb.position.z)
        {
            index++;
        }
        if (index == path.Capacity-1)
        {
            //gameObject.SetActive(false);
            //AgentManager.RemoveAgent(gameObject);
        }

        #region Visualization

        if (false)
        {
            if (path.Count > 0)
            {
                Debug.DrawLine(transform.position, path[0], Color.green);
            }
            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(path[i], path[i + 1], Color.yellow);
            }
        }

        if (false)
        {
            foreach (var neighbor in perceivedNeighbors)
            {
                Debug.DrawLine(transform.position, neighbor.transform.position, Color.yellow);
            }
        }

        #endregion
    }

    #region Public Functions

    public void ComputePath(Vector3 destination)
    {
        nma.enabled = true;
        var nmPath = new NavMeshPath();
        nma.CalculatePath(destination, nmPath);
        path = nmPath.corners.Skip(1).ToList();
        //path = new List<Vector3>() { destination };
        //nma.SetDestination(destination);
        nma.enabled = false;
    }

    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }

    #endregion

    #region Incomplete Functions

    private Vector3 ComputeForce()
    {
<<<<<<< Updated upstream
        var force = CalculateGoalForce();
=======
       
        var force = CalculateGoalForce()+CalculateAgentForce();
>>>>>>> Stashed changes

        if (force != Vector3.zero)
        {
            return force.normalized * Mathf.Min(force.magnitude, Parameters.maxSpeed);
        } else
        {
            return Vector3.zero;
        }
    }
    
    private Vector3 CalculateGoalForce()
    {
<<<<<<< Updated upstream
        Vector3 res = new Vector3();
        if (modify)
        {
            return new Vector3(0.0f,0.0f,0.0f);
        }
        if (index == path.Capacity)
        {
            return Vector3.zero;
        }
        if (flag)
        {
            Vector3 direction = path[index] - rb.position;
            res = mass * (100f * direction - rb.velocity) / 0.5f;//ime.deltaTime
        }
        else
        {
            flag = true;
            index = 0;
            Vector3 direction = path[index] - rb.position;
            res = mass * (100f * direction - rb.velocity) / 0.5f;
        }
        return res;

        //Vector3 forceG = new Vector3();

        //forceG = rb.mass* (denominator / Time.deltaTime);
=======
        Vector3 forceG;
        if (!click)
        {
            forceG = new Vector3(0.0f,0.0f,0.0f);
            return forceG;
        }

        var e = path[0] - transform.position;
        Vector3 denominator = new Vector3();
        denominator.x = Parameters.maxSpeed * e.x - rb.velocity.x;
        denominator.z = Parameters.maxSpeed * e.z - rb.velocity.z;
        denominator.y = 0;

        

        forceG = rb.mass* (denominator / Parameters.T);
>>>>>>> Stashed changes

        
        //return Vector3.zero;
    }

    private Vector3 CalculateAgentForce()
    {
        Vector3 force=new Vector3(0f,0f,0f);
        float R_ij = 2 * radius;
        float Ai = Parameters.A;
        float Bi = Parameters.B;
        float k = Parameters.k;
        float kappa = Parameters.Kappa;
        foreach (var neighbor in perceivedNeighbors) {
            Rigidbody rb_neighbor = neighbor.GetComponent<Rigidbody>();
            float neighbor_x = neighbor.transform.position.x;
            float x = transform.position.x;
            float neighbor_z = neighbor.transform.position.x;
            float z = transform.position.z;
            float D_ij= (float)Math.Sqrt((neighbor_x - x) * (neighbor_x - x) + (neighbor_z - z) * (neighbor_z - z));
            Vector3 N_ij = (neighbor.transform.position - transform.position) / D_ij;
            float g = 0;
            if (R_ij - D_ij > 0)
                g = R_ij - D_ij;
            Vector3 t_ij = new Vector3(-N_ij.z,0,N_ij.x);
            float Delta_v_ji_t=Vector3.Dot(rb.velocity - rb_neighbor.velocity, t_ij);
            Vector3 f= (Ai*(float)Math.Exp((R_ij-D_ij)/Bi)+k*g) * N_ij + kappa * g * Delta_v_ji_t * t_ij;
            force += f;
        }
        return force;
    }

    private Vector3 CalculateWallForce()
    {
        return Vector3.zero;
    }

    public void ApplyForce()
    {
        var force = ComputeForce();
        force.y = 0;

        rb.AddForce(force * 10, ForceMode.Force);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (AgentManager.IsAgent(other.gameObject))
        {
            perceivedNeighbors.Add(other.gameObject);
        }
        else if (WallManager.IsWall(other.gameObject))
        {
            //perceivedWalls.Add(other.gameObejct);
        }
    }
    
    public void OnTriggerExit(Collider other)
    {
        if (AgentManager.IsAgent(other.gameObject))
        {
            perceivedNeighbors.Remove(other.gameObject);
        }
        else if (WallManager.IsWall(other.gameObject))
        {
            //perceivedWalls.Remove(other.gameObejct);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        
    }

    public void OnCollisionExit(Collision collision)
    {
        
    }

    #endregion
}
