using System;
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
    public Rigidbody rb;

    public bool flag;
    private int index;
    public bool modify;


    private HashSet<GameObject> perceivedNeighbors = new HashSet<GameObject>();
    private HashSet<GameObject> perceivedWalls = new HashSet<GameObject>();

    public static bool click;

    void Start()
    {
        modify = true;
        click = false;
        path = new List<Vector3>();
        nma = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        gameObject.transform.localScale = new Vector3(2 * radius, 1, 2 * radius);
        nma.radius = radius;
        rb.mass = mass;

        GetComponent<SphereCollider>().radius = perceptionRadius / 5;
    }

    private void Update()
    {
        if (path.Count > 1 && Vector3.Distance(transform.position, path[0]) < 1.1f)
        {
            path.RemoveAt(0);
        } else if (path.Count == 1 && Vector3.Distance(transform.position, path[0]) < 1.5f)
        {
            path.RemoveAt(0);

            if (path.Count == 0)
            {
                gameObject.SetActive(false);
                AgentManager.RemoveAgent(gameObject);
            }
        }
        
        if (index == path.Capacity-1)
        {
            gameObject.SetActive(false);
            AgentManager.RemoveAgent(gameObject);
        }
        //comment above for flocking
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
        var force = Vector3.zero;
        if (click) {
            //print("goal:"+CalculateGoalForce());
            //print("agent:" + CalculateAgentForce());
            //print("wall:" + CalculateWallForce());
            force = CalculateGoalForce() + CalculateAgentForce()+ 5*CalculateWallForce();
        }
       
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
        Vector3 forceG;
        var e = path[0] - transform.position;
        Vector3 denominator = new Vector3();
        denominator.x = Parameters.maxSpeed * e.x - rb.velocity.x;
        denominator.z = Parameters.maxSpeed * e.z - rb.velocity.z;

        forceG = rb.mass * (denominator / Parameters.T);

        return forceG;
        //return Vector3.zero;
    }

    private Vector3 CalculateAgentForce()
    {

        Vector3 force = new Vector3(0f, 0f, 0f);
        float R_ij = 2 * radius;
        float Ai = Parameters.A;
        float Bi = Parameters.B;
        float k = Parameters.k;
        float kappa = Parameters.Kappa;
        foreach (var neighbor in perceivedNeighbors)
        {
            Rigidbody rb_neighbor = neighbor.GetComponent<Rigidbody>();
            float neighbor_x = neighbor.transform.position.x;
            float x = transform.position.x;
            float neighbor_z = neighbor.transform.position.z;
            float z = transform.position.z;
            float D_ij = (float)Math.Sqrt((neighbor_x - x) * (neighbor_x - x) + (neighbor_z - z) * (neighbor_z - z));
            Vector3 N_ij = (transform.position - neighbor.transform.position) / D_ij;
            float g = 0;
            if (R_ij - D_ij > 0)
                g = R_ij - D_ij;
            Vector3 t_ij = new Vector3(-N_ij.z, 0, N_ij.x);
            float Delta_v_ji_t = Vector3.Dot(rb.velocity - rb_neighbor.velocity, t_ij);
            Vector3 f = (Ai * (float)Math.Exp((R_ij - D_ij) / Bi) + k * g) * N_ij + kappa * g * Delta_v_ji_t * t_ij;
            force += f;
        }
        return force;
        //return Vector3.zero;
    }

    private Vector3 CalculateWallForce()
    {
        Vector3 force = new Vector3(0f, 0f, 0f);
        float R_i = radius + 0.7f;
        float Ai = Parameters.WALL_A;
        float Bi = Parameters.WALL_B;
        float k = Parameters.WALL_k;
        float kappa = Parameters.WALL_Kappa;
        foreach (var wall in perceivedWalls)
        {
            Rigidbody rb_wall = wall.GetComponent<Rigidbody>();
            float x = transform.position.x;
            float z = transform.position.z;
            float wall_x = wall.transform.position.x;
            float wall_z = wall.transform.position.z;
            Vector3 planepos = wall.transform.position;
            if (x >= wall_x + 0.5)
                planepos += 1 / 2 * Vector3.right;
            else if (x <= wall_x - 0.5)
                planepos += 1 / 2 * Vector3.left;
            if (z >= wall_z + 0.5)
                planepos += 1 / 2 * Vector3.forward;
            if (z <= wall_z - 0.5)
                planepos += 1 / 2 * Vector3.back;
            wall_x = planepos.x;
            wall_z = planepos.z;
            float D_iw = (float)Math.Sqrt((wall_x - x) * (wall_x - x) + (wall_z - z) * (wall_z - z));
            Vector3 N_iw;
            N_iw = (transform.position - planepos) / D_iw;
            float g = 0;
            if (R_i - D_iw > 0)
                g = R_i - D_iw;
            Vector3 t_iw = new Vector3(-N_iw.z, 0, N_iw.x);
            Vector3 vi = rb.velocity;
            Vector3 f = (Ai * (float)Math.Exp((R_i - D_iw) / Bi) + k * g) * N_iw - kappa * g * Vector3.Dot(vi, t_iw) * t_iw;
            force += f;
        }
        return force;
        //return Vector3.zero;
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
            perceivedWalls.Add(other.gameObject);
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
            perceivedWalls.Remove(other.gameObject);
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
