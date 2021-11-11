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

    void Start()
    {
        modify = true;
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
        var force = CalculateGoalForce();

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

        
        //return Vector3.zero;
    }

    private Vector3 CalculateAgentForce()
    {
        return Vector3.zero;
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
