 using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

/*
    * Class used for managing navmeshes and agents
*/

public class GlobalNavMeshController : MonoBehaviour
{   
    public GameObject navMeshBuilderObject;
    NavMeshBuilder navMeshBuilder;

    List<GameObject> agents = new List<GameObject>();
    Dictionary<(int, int), DynamicNavMeshController> navMeshSurfaces = new Dictionary<(int, int), DynamicNavMeshController>();
    Queue<DynamicNavMeshController> updateQueue = new Queue<DynamicNavMeshController>();

    private MeshFilter meshFilter; // Mesh filter for visualizing the navmesh
    public Color navMeshColor = new Color(0, 1, 0, 0.5f); // Green with transparency

    float UPDATE_DELAY = .1f;
    
    bool shouldUpdate = false;
    public bool ShouldUpdate { get => shouldUpdate; }
    public void MarkForUpdate() {
        shouldUpdate = true;
    }
    public void FinishUpdate() {
        shouldUpdate = false;
    }

    void Awake() {
        navMeshBuilder = navMeshBuilderObject.GetComponent<NavMeshBuilder>();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = CreateNavMeshMaterial();

    }

    void Start() 
    {
        // Find all agents and save them into the array
        agents = World.GetActiveAgents();
        
        // mark for first update
        MarkForUpdate();

        // start the update queue
        StartCoroutine(ProcessUpdateQueue());
    }

    void Update() {
        if(shouldUpdate) {
            RecalculateNavMeshes();
        }
    }

    IEnumerator ProcessUpdateQueue()
    {
        while (true)
        {
            if (updateQueue.Count > 0)
            {
                var surfaceController = updateQueue.Dequeue();
                if(surfaceController.State == DynamicNavMeshState.Build)
                {
                    surfaceController.State = DynamicNavMeshState.Building;
                    surfaceController.BuildNavMesh();
                    // GenerateNavMeshMesh();
                }
                
                else if(surfaceController.State == DynamicNavMeshState.Destroy)
                {
                    surfaceController.State = DynamicNavMeshState.Destroying;
                    Destroy(surfaceController.gameObject);
                    // GenerateNavMeshMesh();
                }
            }
            yield return new WaitForSeconds(UPDATE_DELAY);
        }
    }

    void RecalculateNavMeshes() {

        // TODO: optimize further by only recalculating the affected navmeshes

        // cluster agents
        var agentClusters = AgentClustering.ClusterAgents();

        // deactivate all agents
        foreach(var agent in agents) {
            var navMeshAgent = agent.GetComponent<NavMeshAgent>();
            navMeshAgent.isStopped = true;
        }

        // mark all surfaces for destruction
        foreach( var (_, surfaceController) in navMeshSurfaces ) {
            surfaceController.State = DynamicNavMeshState.Destroy;
            updateQueue.Enqueue(surfaceController);
        }
        
        // from agent clusters, create navmesh surfaces
        navMeshSurfaces = navMeshBuilder.BuildNavMeshesFromAgentClusters(agentClusters);
        foreach( var (_, surfaceController) in navMeshSurfaces ) {
            // assign the global navmesh controller
            surfaceController.GlobalNavMeshController = this;
            updateQueue.Enqueue(surfaceController);
        }

        FinishUpdate();
    }

    private void GenerateNavMeshMesh() {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();

        // Create a new mesh
        Mesh mesh = new Mesh();

        // Get the vertices in world space and convert them to the local space of the NavMeshSurface
        Vector3[] localVertices = new Vector3[triangulation.vertices.Length];
        for (int i = 0; i < triangulation.vertices.Length; i++)
        {
            localVertices[i] = transform.InverseTransformPoint(triangulation.vertices[i]);
        }

        // Assign the local vertices and triangles to the mesh
        mesh.vertices = localVertices;
        mesh.triangles = triangulation.indices;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
    private Material CreateNavMeshMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = navMeshColor;
        mat.SetFloat("_Mode", 3); // Make it transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        return mat;
    }
}