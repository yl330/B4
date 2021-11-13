using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AgentManager : MonoBehaviour
{
    public int agentCount = 10;
    public float agentSpawnRadius = 20;
    public GameObject agentPrefab;
    public int pusueAgent = 5;
    public int evadeAgent = 5;
    public static Dictionary<GameObject, Agent> agentsObjs = new Dictionary<GameObject, Agent>();

    private static List<Agent> agents = new List<Agent>();

    private static List<Agent> Pagents = new List<Agent>();
    private static List<Agent> Eagents = new List<Agent>();
    private static List<Vector3> list = new List<Vector3>();

    private GameObject agentParent;
    private Vector3 destination;

    public const float UPDATE_RATE = 0.0f;
    private const int PATHFINDING_FRAME_SKIP = 25;

    #region Unity Functions

    void Awake()
    {
        Random.InitState(0);
        Vector3 v1 = new Vector3(-12.8f, 1.1f, 14.3f);
        Vector3 v2 = new Vector3(14.3f, 1.1f, 14.4f);
        Vector3 v3 = new Vector3(14.2f, 1.1f, -13.3f);
        Vector3 v4 = new Vector3(-14.2f, 1.1f, -13.8f);
        list.Add(v1);
        list.Add(v2);
        list.Add(v3);
        list.Add(v4);

        agentParent = GameObject.Find("Agents");
        /*for (int i = 0; i < agentCount; i++)
        {
            var randPos = new Vector3((Random.value - 0.5f) * agentSpawnRadius, 0, (Random.value - 0.5f) * agentSpawnRadius);
            NavMeshHit hit;
            NavMesh.SamplePosition(randPos, out hit, 10, NavMesh.AllAreas);
            randPos = hit.position + Vector3.up;

            GameObject agent = null;
            agent = Instantiate(agentPrefab, randPos, Quaternion.identity);
            agent.name = "Agent " + i;
            agent.transform.parent = agentParent.transform;
            var agentScript = agent.GetComponent<Agent>();
            agentScript.radius = 0.3f;// Random.Range(0.2f, 0.6f);
            agentScript.mass = 1;
            agentScript.perceptionRadius = 3;

            agents.Add(agentScript);
            agentsObjs.Add(agent, agentScript);
        }*/
        for (int i = 0; i < pusueAgent; i++)
        {
            var randPos = new Vector3((Random.value - 0.5f) * agentSpawnRadius, 0, (Random.value - 0.5f) * agentSpawnRadius);
            NavMeshHit hit;
            NavMesh.SamplePosition(randPos, out hit, 10, NavMesh.AllAreas);
            randPos = hit.position + Vector3.up;

            GameObject agent = null;
            agent = Instantiate(agentPrefab, randPos, Quaternion.identity);
            agent.name = "PAgent " + i;
            agent.transform.parent = agentParent.transform;
            var agentScript = agent.GetComponent<Agent>();
            agentScript.radius = 0.3f;// Random.Range(0.2f, 0.6f);
            agentScript.mass = 1;
            agentScript.perceptionRadius = 3;
            agentScript.color = true;

            Pagents.Add(agentScript);
            agentsObjs.Add(agent, agentScript);
        }

        for (int i = 0; i < evadeAgent; i++)
        {
            var randPos = new Vector3((Random.value - 0.5f) * agentSpawnRadius, 0, (Random.value - 0.5f) * agentSpawnRadius);
            NavMeshHit hit;
            NavMesh.SamplePosition(randPos, out hit, 10, NavMesh.AllAreas);
            randPos = hit.position + Vector3.up;

            GameObject agent = null;
            agent = Instantiate(agentPrefab, randPos, Quaternion.identity);
            
            agent.name = "EAgent " + i;
            agent.transform.parent = agentParent.transform;
            var agentScript = agent.GetComponent<Agent>();
            agentScript.radius = 0.3f;// Random.Range(0.2f, 0.6f);
            agentScript.mass = 1;
            agentScript.perceptionRadius = 3;
            agentScript.color = false;

            Eagents.Add(agentScript);
            agentsObjs.Add(agent, agentScript);
        }

        StartCoroutine(Run());
    }
    
    void Update()
    {
        automove();
        #region Visualization
        if (Input.GetMouseButton(1))
        {
            print("Move");
            //Agent.click = true;
            //MouseClickCheck();
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (true)
            {
                var point = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10);
                var dir = point - Camera.main.transform.position;
                RaycastHit rcHit;
                if (Physics.Raycast(point, dir, out rcHit))
                {
                    point = rcHit.point;
                }
            } else
            {
                var randPos = new Vector3((Random.value - 0.5f) * agentSpawnRadius, 0, (Random.value - 0.5f) * agentSpawnRadius);

                NavMeshHit hit;
                NavMesh.SamplePosition(randPos, out hit, 1.0f, NavMesh.AllAreas);
                print(hit.position);
                Debug.DrawLine(hit.position, hit.position + Vector3.up * 10, Color.red, 1000000);
                foreach (var agent in agents)
                {
                    //agent.ComputePath(hit.position);
                }
            }
        }

#if UNITY_EDITOR
        if (Application.isFocused)
        {
            //UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
        }
#endif

        #endregion
    }


    IEnumerator Run()
    {
        yield return null;

        for (int iterations = 0; ; iterations++)
        {
            if (iterations % PATHFINDING_FRAME_SKIP == 0)
            {
                SetAgentDestinations(destination);
                SetPDestinations();
                SetEDestinations();

            }

            foreach (var agent in agents)
            {
                agent.ApplyForce();
            }
            foreach (var agent in Pagents)
            {
                agent.ApplyForce();
            }
            foreach (var agent in Eagents)
            {
                agent.ApplyForce();
            }

            if (UPDATE_RATE == 0)
            {
                yield return null;
            } else
            {
                yield return new WaitForSeconds(UPDATE_RATE);
            }
        }
    }

    #endregion

    #region Public Functions

    public static bool IsAgent(GameObject obj)
    {
        return agentsObjs.ContainsKey(obj);
    }

    public void SetAgentDestinations(Vector3 destination)
    {
        NavMeshHit hit;
        NavMesh.SamplePosition(destination, out hit, 10, NavMesh.AllAreas);
        foreach (var agent in agents)
        {
            agent.ComputePath(hit.position);
            //agent.modify = false;
        }
    }

    public static void RemoveAgent(GameObject obj)
    {
        var agent = obj.GetComponent<Agent>();

        agents.Remove(agent);
        agentsObjs.Remove(obj);
    }

    #endregion

    #region Private Functions

    #endregion

    #region Visualization Functions

    #endregion

    #region Utility Classes

    private class Tuple<K,V>
    {
        public K Item1;
        public V Item2;

        public Tuple(K k, V v) {
            Item1 = k;
            Item2 = v;
        }
    }

    #endregion

    void MouseClickCheck()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (!Physics.Raycast(ray, out hitInfo))
        {
            return;
        }

        //NavMeshHit navHitInfo;
        destination = hitInfo.point;
        SetAgentDestinations(hitInfo.point);
    }
    void automove()
    {
        SetPDestinations();
        SetEDestinations();
    }

    public void SetPDestinations()
    {
        //NavMeshHit hit;
        //Vector3 destination = Eagents[0].getposition();
        //NavMesh.SamplePosition(destination, out hit, 10, NavMesh.AllAreas);
        Vector3 destination = new Vector3(0, 0, 0);
        foreach (var agent in Pagents)
        {
            float distance = Vector3.Distance(agent.getposition(), Eagents[0].getposition());
            foreach (var b in Eagents)
            {
                if(Vector3.Distance(agent.getposition(), b.getposition())<=distance)
                {
                    destination = b.getposition();
                    distance = Vector3.Distance(agent.getposition(), b.getposition());
                }
            }
            agent.ComputePath(destination);
            
        }
    }
    public void SetEDestinations()
    {
        //NavMeshHit hit;
        //NavMesh.SamplePosition(destination, out hit, 10, NavMesh.AllAreas);
        Vector3 destination = new Vector3(0, 0, 0);
        Vector3 res = new Vector3(0, 0, 0);
        foreach (var agent in Eagents)
        {
            float distance = Vector3.Distance(Pagents[0].getposition(), agent.getposition());
            //agent.ComputePath(hit.position);
            foreach (var p in Pagents)
            {

                if (Vector3.Distance(agent.getposition(), p.getposition()) < distance)
                {
                    destination = p.getposition();
                    distance = Vector3.Distance(agent.getposition(), p.getposition());
                }
            }
            float min = Vector3.Distance(destination, list[0]);
            res = list[0];
            foreach (Vector3 v in list)
            {
                if (Vector3.Distance(destination,v) >= min)
                {
                    min = Vector3.Distance(destination, v);
                    res = v;
                }
            }
            agent.ComputePath(res);
        }

    }

}
