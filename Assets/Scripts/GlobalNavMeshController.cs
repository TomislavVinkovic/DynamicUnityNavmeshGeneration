using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GlobalNavMeshController : MonoBehaviour
{
    public GameObject dynamicNavMeshPrefab;
    List<GameObject> agents = new List<GameObject>();
    List<DynamicNavMeshController> navMeshSurfaces = new List<DynamicNavMeshController>();
    Queue<DynamicNavMeshController> updateQueue = new Queue<DynamicNavMeshController>();

    Vector3 AGENT_NAVMESH_BOUNDS_SIZE = new Vector3(10f, 5f, 10f);
    float UPDATE_DELAY = .1f;
    float NAVMESH_SURFACE_MERGE_DISTANCE = 10f;
    string AGENT_TAG = "Agent";

    void Start() 
    {
        // Find all agents and save them into the array
        agents = GetAllAgents();

        // sort agents such that the agents in the bottom left corner are first
        agents.Sort(SortByDistanceToBottomLeftCorner);
        
        List<AgentGroup> agentGroups = GetAgentGroups(agents);
        foreach (var group in agentGroups) {
            GameObject navMeshSurface = InstantiateDynamicNavMeshSurface(group.Center);
            var surfaceController = navMeshSurface.GetComponent<DynamicNavMeshController>();
            surfaceController.SetNavMeshBounds(
                new Bounds(
                    group.Center,
                    Vector3.Scale(AGENT_NAVMESH_BOUNDS_SIZE, new Vector3(group.AgentCount, 1f, group.AgentCount))
                )
            );
            navMeshSurfaces.Add(navMeshSurface.GetComponent<DynamicNavMeshController>());
        }

        StartCoroutine(ProcessUpdateQueue());
    }

    void Update() {
        // group surfaces that are near each other
        GroupNearbyNavMeshSurfaces();

        // push the surfaces that need to be updated into the update queue
        foreach (var navMeshSurface in navMeshSurfaces) {
            if(navMeshSurface.state == DynamicNavMeshState.Create) {
                navMeshSurface.state  = DynamicNavMeshState.Creating;
                updateQueue.Enqueue(navMeshSurface);
            }
            else if(navMeshSurface.state == DynamicNavMeshState.Update) {
                navMeshSurface.state = DynamicNavMeshState.Updating;
                updateQueue.Enqueue(navMeshSurface);
            }
            else if (navMeshSurface.state == DynamicNavMeshState.Destroy) {
                navMeshSurface.state = DynamicNavMeshState.Destroying;
                updateQueue.Enqueue(navMeshSurface);
            }
        }
    }

    // INSTANTIATION
    GameObject InstantiateDynamicNavMeshSurface(Vector3 center) {
        // maybe change later
        center.y = 0;

        return Instantiate(
            dynamicNavMeshPrefab,
            center,
            Quaternion.identity
        );
    }

    IEnumerator ProcessUpdateQueue()
    {
        while (true)
        {
            if (updateQueue.Count > 0)
            {
                var surfaceController = updateQueue.Dequeue();
                if(surfaceController.state == DynamicNavMeshState.Creating)
                {
                    surfaceController.BuildNavMesh();
                }
                    
                else if(surfaceController.state == DynamicNavMeshState.Updating)
                {
                    UpdateNavMesh(surfaceController);
                }
                else if(surfaceController.state == DynamicNavMeshState.Destroying)
                {
                    // deactivate all agents
                    // surfaceController.AgentsInside.ForEach(agent => agent.SetActive(false));
                    navMeshSurfaces.Remove(surfaceController);
                    Destroy(surfaceController.gameObject);
                }
            }
            yield return new WaitForSeconds(UPDATE_DELAY);
        }
    }

    // SORTING METHODS
    int SortByDistanceToBottomLeftCorner(GameObject a, GameObject b) {
        if(a.transform.position.x < b.transform.position.x) {
            return -1;
        }
        if(a.transform.position.x > b.transform.position.x) {
            return 1;
        }
        if(a.transform.position.z < b.transform.position.z) {
            return -1;
        }
        if(a.transform.position.z > b.transform.position.z) {
            return 1;
        }
        return 0;
    }
    int SortByDistanceToBottomLeftCorner(DynamicNavMeshController a, DynamicNavMeshController b) {
        if(a.transform.position.x < b.transform.position.x) {
            return -1;
        }
        if(a.transform.position.x > b.transform.position.x) {
            return 1;
        }
        if(a.transform.position.z < b.transform.position.z) {
            return -1;
        }
        if(a.transform.position.z > b.transform.position.z) {
            return 1;
        }
        return 0;
    }
    
    // GROUPING METHODS
    void GroupNearbyNavMeshSurfaces() {
        var navMeshSurfacesCopy = new List<DynamicNavMeshController>(
            navMeshSurfaces.Where(navMeshSurface => navMeshSurface.state == DynamicNavMeshState.Ready)
        );
        navMeshSurfacesCopy.Sort(SortByDistanceToBottomLeftCorner);

        // group navmesh surfaces that are near each other on the xz plane
        // i am only interested in groups that have more than one element
        var navMeshGroups = GroupNavMeshes(navMeshSurfacesCopy).Where(group => group.Count > 1);
        foreach (var group in navMeshGroups) {
            int agentsInsideCount = group.Sum(navMeshSurface => navMeshSurface.AgentsInside.Count);
            int groupSurfacesCount = group.Count;
            // get the mean of the group
            var groupCenter = LinearAlgebra.GetMeanInSpace(
                group.ConvertAll(navMeshSurface => navMeshSurface.transform.position)
            );

            // instantiate a new navmesh surface in the mean of the group
            GameObject newNavMeshSurface = InstantiateDynamicNavMeshSurface(groupCenter);
            DynamicNavMeshController controller = newNavMeshSurface.GetComponent<DynamicNavMeshController>();

            // set the bounds of the new controller to be the largest bounds
            // times the number of surfaces in the group
            controller.SetNavMeshBounds(
                new Bounds(
                    groupCenter,
                    Vector3.Scale(AGENT_NAVMESH_BOUNDS_SIZE, new Vector3(agentsInsideCount, 1f, agentsInsideCount))
                )
            );

            // add the new controller to the list
            navMeshSurfaces.Add(controller);

            // destroy the group
            foreach (var navMeshSurface in group) {
                navMeshSurface.state = DynamicNavMeshState.Destroy;
            }
        }
    }

    List<List<DynamicNavMeshController>> GroupNavMeshes(List<DynamicNavMeshController> surfaces) {
        List<DynamicNavMeshController> currentGroup = new List<DynamicNavMeshController>();
        List<List<DynamicNavMeshController>> groups = new List<List<DynamicNavMeshController>>();

        foreach (var navMeshSurface in surfaces) {
            // case for the first element
            if(currentGroup.Count == 0) {
                currentGroup.Add(navMeshSurface);
                continue;
            }

            // for each element in the current group, see if the navmeshsurface is close enough
            // compare them by navmeshbounds edge distance
            foreach (var groupSurface in currentGroup) {
                // if their closest points are close enough, add the navmeshsurface to the group
                if(
                    Vector3.Distance(
                        groupSurface.NavMeshBounds.ClosestPoint(navMeshSurface.NavMeshBounds.min),
                        navMeshSurface.NavMeshBounds.ClosestPoint(groupSurface.NavMeshBounds.min)   
                    ) < NAVMESH_SURFACE_MERGE_DISTANCE) {
                    currentGroup.Add(navMeshSurface);
                    break;
                }
            }

            // otherwise, create a new group
            groups.Add(currentGroup);
            // instantiate a new group with the current navmeshsurface
            currentGroup = new List<DynamicNavMeshController>{navMeshSurface};
        }

        return groups;
    }

    /*
        Works simmiliarly as the k-means algorithm
        However, the groups are already sorted so the time complexity is greatly reduced
    */
    List<AgentGroup> GetAgentGroups(List<GameObject> agents) {
        List<AgentGroup> agentGroups = new List<AgentGroup>();
        AgentGroup currentGroup = new AgentGroup();

        // we can do it like this because the agents are sorted
        foreach (var agent in agents) {
            // case for the first element
            if(currentGroup.AgentCount == 0) {
                currentGroup.AddAgent(agent);
                currentGroup.Center = agent.transform.position;
                continue;
            }

            // scale the bounds by number of agents in the group
            var agentsInGroup = currentGroup.AgentCount;
            
            // +1 because i am checking if the agent is inside the bounds of the superficial new group
            var boundsSize = Vector3.Scale(AGENT_NAVMESH_BOUNDS_SIZE, new Vector3(agentsInGroup + 1, 1f, agentsInGroup + 1));
            var smallerBoundsSize = Vector3.Scale(boundsSize, new Vector3(0.7f, 1f, 0.7f));

            var groupCenterBounds = new Bounds(currentGroup.Center, boundsSize);
            var groupCenterSmallerBounds = new Bounds(currentGroup.Center, smallerBoundsSize);


            if(!groupCenterSmallerBounds.Contains(agent.transform.position)) {
                agentGroups.Add(currentGroup);
                currentGroup = new AgentGroup();
            }
            currentGroup.AddAgent(agent);
            currentGroup.Center = LinearAlgebra.GetMeanInSpace(
                currentGroup.Agents.ConvertAll(agent => agent.transform.position)
            );

            
        }
        if( (agentGroups.Count == 0 && currentGroup.AgentCount > 0) || currentGroup.Id != agentGroups[agentGroups.Count - 1].Id ) {
            agentGroups.Add(currentGroup);
        }

        return agentGroups;
    }

    void UpdateNavMesh(DynamicNavMeshController oldMeshSurface) {
        // get all agents inside and sort them by distance to the bottom left corner
        var agentsInside = oldMeshSurface.AgentsInside;
        agentsInside.Sort(SortByDistanceToBottomLeftCorner);

        oldMeshSurface.AgentsInside.ForEach(agent => agent.SetActive(false));

        // get the agent groups
        // if there
        List<AgentGroup> agentGroups = GetAgentGroups(agentsInside);

        // If you can get by without destroying the old mesh, do it
        if(agentGroups.Count == 1) {
            oldMeshSurface.UpdateNavMeshStatic();
        }
        
        // otherwise, destroy the old mesh and create new ones
        else {
            foreach (var group in agentGroups) {
            GameObject navMeshSurface = InstantiateDynamicNavMeshSurface(group.Center);
            var surfaceController = navMeshSurface.GetComponent<DynamicNavMeshController>();
            surfaceController.SetNavMeshBounds(
                new Bounds(
                    group.Center,
                    Vector3.Scale(AGENT_NAVMESH_BOUNDS_SIZE, new Vector3(group.AgentCount, 1f, group.AgentCount))
                )
            );
            navMeshSurfaces.Add(navMeshSurface.GetComponent<DynamicNavMeshController>());
            }
            oldMeshSurface.state = DynamicNavMeshState.Destroy;
        }
        
    }

    private List<GameObject> GetAllAgents() {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];

        // Create a list to store all GameObjects with the specified tag
        List<GameObject> agents = new List<GameObject>();

        // Iterate through allObjects and add those with the specified tag to the agents list
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag(AGENT_TAG))
            {
                agents.Add(obj);
            }
        }

        return agents;
    }
}