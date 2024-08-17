using UnityEngine;
using UnityEngine.AI;

public class NavMeshMeshGenerator : MonoBehaviour
{
    public Color navMeshColor = new Color(0.0f, 0.5f, 0.5f, 0.5f);
    MeshFilter meshFilter;

    void Awake()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = CreateNavMeshMaterial();
    }

    public void GenerateNavMeshMesh() {
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
    Material CreateNavMeshMaterial()
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
