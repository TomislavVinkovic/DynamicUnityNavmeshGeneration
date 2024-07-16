using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;

public class GlobalNavMeshController : MonoBehaviour
{
    public GameObject dynamicNavMeshPrefab;
    List<GameObject> agents = new List<GameObject>();
    List<DynamicNavMeshController> navMeshSurfaces = new List<DynamicNavMeshController>();
    Queue<DynamicNavMeshController> updateQueue = new Queue<DynamicNavMeshController>();

    Vector3 AGENT_NAVMESH_BOUNDS_SIZE = new Vector3(20f, 5f, 20f);
    float UPDATE_DELAY = .1f;
    float NAVMESH_SURFACE_MERGE_DISTANCE = 10f;

    void Start() 
    {
        // Find all agents and save them into the array
        agents = new List<GameObject>(GameObject.FindGameObjectsWithTag("Agent"));

        // sort agents such that the agents in the bottom left corner are first
        agents.Sort(SortByDistanceToBottomLeftCorner);
        
        List<Vector3> groupCenters = GetAgentGroupCenters();
        foreach (var center in groupCenters) {
            GameObject navMeshSurface = InstantiateDynamicNavMeshSurface(center);
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
                    surfaceController.UpdateNavMesh();
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
            int groupSurfacesCount = group.Count;
            // get the mean of the group
            var groupCenter = LinearAlgebra.GetMeanInSpace(
                group.ConvertAll(navMeshSurface => navMeshSurface.transform.position)
            );
            // find the navmesh in the group with the largest bounds
            var largestBounds = group.OrderByDescending(
                navMeshSurface => navMeshSurface.NavMeshBounds.size.sqrMagnitude
            ).First().NavMeshBounds.size;

            // instantiate a new navmesh surface in the mean of the group
            GameObject newNavMeshSurface = InstantiateDynamicNavMeshSurface(groupCenter);
            DynamicNavMeshController controller = newNavMeshSurface.GetComponent<DynamicNavMeshController>();

            // set the bounds of the new controller to be the largest bounds
            // times the number of surfaces in the group
            controller.SetNavMeshBounds(
                new Bounds(
                    groupCenter,
                    Vector3.Scale(largestBounds, new Vector3(groupSurfacesCount, 1f, groupSurfacesCount))
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

    List<Vector3> GetAgentGroupCenters() {
        List<GameObject> currentGroup = new List<GameObject>();
        List<Vector3> groupCenters = new List<Vector3>();

        Vector3 currentGroupCenter = agents[0].transform.position;

        // we can do it like this because the agents are sorted
        foreach (var agent in agents) {
            var groupCenterBounds = new Bounds(currentGroupCenter, AGENT_NAVMESH_BOUNDS_SIZE);
            if(!groupCenterBounds.Contains(agent.transform.position)) {
                groupCenters.Add(currentGroupCenter);
                currentGroup = new List<GameObject>();
                currentGroupCenter = agent.transform.position;
            }
            currentGroup.Add(agent);
            currentGroupCenter = LinearAlgebra.GetMeanInSpace(
                currentGroup.ConvertAll(agent => agent.transform.position)
            );
        }
        if(currentGroupCenter != groupCenters[groupCenters.Count - 1]) {
            groupCenters.Add(currentGroupCenter);
        }

        return groupCenters;
    }
}