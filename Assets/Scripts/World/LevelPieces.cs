using UnityEngine;

/// <summary>
/// Base class for level geometry that skaters interact with.
/// Attach to GameObject with a collider to make it skate-able.
/// </summary>
public abstract class LevelPiece : MonoBehaviour
{
    [SerializeField] protected Material activeMaterial;
    protected MeshRenderer meshRenderer;

    protected virtual void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Skateboard"))
        {
            OnSkaterEnter();
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Skateboard"))
        {
            OnSkaterExit();
        }
    }

    protected virtual void OnSkaterEnter() { }
    protected virtual void OnSkaterExit() { }
}

/// <summary>
/// Quarter-pipe / ramp - slopes up, great for launching tricks
/// </summary>
public class QuarterPipe : LevelPiece
{
    [SerializeField] private float height = 3f;
    [SerializeField] private float length = 4f;
    [SerializeField] private float width = 5f;

    private void Start()
    {
        GenerateRampMesh();
    }

    private void GenerateRampMesh()
    {
        var mf = GetComponent<MeshFilter>();
        if (mf == null) mf = gameObject.AddComponent<MeshFilter>();

        var mesh = new Mesh();
        var verts = new Vector3[]
        {
            // Base
            new(0, 0, 0),
            new(width, 0, 0),
            new(width, 0, length),
            new(0, 0, length),
            // Top
            new(0, height, 0),
            new(width, height, 0),
            new(width, height, length),
            new(0, height, length)
        };

        var tris = new int[]
        {
            // Base
            0, 2, 1, 0, 3, 2,
            // Top
            4, 5, 6, 4, 6, 7,
            // Front (ramp)
            0, 5, 4, 0, 1, 5,
            // Back
            2, 7, 6, 2, 3, 7,
            // Sides
            0, 4, 7, 0, 7, 3,
            1, 6, 5, 1, 2, 6
        };

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mf.mesh = mesh;

        var collider = GetComponent<BoxCollider>();
        if (collider == null) collider = gameObject.AddComponent<BoxCollider>();
        collider.center = new(width / 2f, height / 2f, length / 2f);
        collider.size = new(width, height, length);
    }
}

/// <summary>
/// Grind rail - long straight surface for grinding tricks
/// </summary>
public class GrindRail : LevelPiece
{
    [SerializeField] private float length = 8f;
    [SerializeField] private float railRadius = 0.15f;
    [SerializeField] private float height = 1.5f;

    private void Start()
    {
        GenerateRailMesh();
    }

    private void GenerateRailMesh()
    {
        var mf = GetComponent<MeshFilter>();
        if (mf == null) mf = gameObject.AddComponent<MeshFilter>();

        var mesh = new Mesh();
        var segments = 12;
        var verts = new Vector3[segments * 2 + 2];

        // Generate cylinder
        for (int i = 0; i <= segments; i++)
        {
            float angle = (i / (float)segments) * 360f * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * railRadius;
            float z = Mathf.Sin(angle) * railRadius;

            verts[i * 2] = new(x, height, 0);
            verts[i * 2 + 1] = new(x, height, length);
        }

        var tris = new System.Collections.Generic.List<int>();
        for (int i = 0; i < segments; i++)
        {
            tris.Add(i * 2);
            tris.Add(i * 2 + 2);
            tris.Add(i * 2 + 1);
            tris.Add(i * 2 + 1);
            tris.Add(i * 2 + 2);
            tris.Add(i * 2 + 3);
        }

        mesh.vertices = verts;
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mf.mesh = mesh;

        var collider = GetComponent<CapsuleCollider>();
        if (collider == null) collider = gameObject.AddComponent<CapsuleCollider>();
        collider.radius = railRadius;
        collider.height = length;
        collider.direction = 2; // Z-axis
        collider.center = new(0, height, length / 2f);
    }
}

/// <summary>
/// Simple box/ledge - good for landing tricks, manuals
/// </summary>
public class SkateBox : LevelPiece
{
    [SerializeField] private float width = 2f;
    [SerializeField] private float height = 0.8f;
    [SerializeField] private float depth = 3f;

    private void Start()
    {
        var mf = GetComponent<MeshFilter>();
        if (mf == null) mf = gameObject.AddComponent<MeshFilter>();

        mf.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

        transform.localScale = new Vector3(width, height, depth);

        var collider = GetComponent<BoxCollider>();
        if (collider == null) collider = gameObject.AddComponent<BoxCollider>();
        collider.center = Vector3.zero;
    }
}

/// <summary>
/// Half-pipe section - curved surface for tricks
/// </summary>
public class HalfPipe : LevelPiece
{
    [SerializeField] private float radius = 3f;
    [SerializeField] private float width = 6f;
    [SerializeField] private int curveSegments = 20;

    private void Start()
    {
        GenerateHalfPipeMesh();
    }

    private void GenerateHalfPipeMesh()
    {
        var mf = GetComponent<MeshFilter>();
        if (mf == null) mf = gameObject.AddComponent<MeshFilter>();

        var mesh = new Mesh();
        var verts = new System.Collections.Generic.List<Vector3>();
        var tris = new System.Collections.Generic.List<int>();

        // Generate half-pipe curve
        for (int i = 0; i <= curveSegments; i++)
        {
            float t = i / (float)curveSegments;
            float angle = t * Mathf.PI; // 0 to 180 degrees
            float y = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius - radius;

            // Left and right edges
            verts.Add(new(-width / 2f, y, z));
            verts.Add(new(width / 2f, y, z));
        }

        // Generate triangles
        for (int i = 0; i < curveSegments; i++)
        {
            int a = i * 2;
            int b = i * 2 + 1;
            int c = (i + 1) * 2;
            int d = (i + 1) * 2 + 1;

            tris.Add(a); tris.Add(c); tris.Add(b);
            tris.Add(b); tris.Add(c); tris.Add(d);
        }

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mf.mesh = mesh;

        var collider = GetComponent<MeshCollider>();
        if (collider == null) collider = gameObject.AddComponent<MeshCollider>();
        collider.convex = false;
    }
}

/// <summary>
/// Stairs - transition piece with discrete steps
/// </summary>
public class Stairs : LevelPiece
{
    [SerializeField] private int stepCount = 5;
    [SerializeField] private float stepHeight = 0.3f;
    [SerializeField] private float stepDepth = 0.4f;
    [SerializeField] private float stairWidth = 3f;

    private void Start()
    {
        GenerateStairsMesh();
    }

    private void GenerateStairsMesh()
    {
        var mf = GetComponent<MeshFilter>();
        if (mf == null) mf = gameObject.AddComponent<MeshFilter>();

        var mesh = new Mesh();
        var verts = new System.Collections.Generic.List<Vector3>();
        var tris = new System.Collections.Generic.List<int>();

        float halfWidth = stairWidth / 2f;

        for (int step = 0; step <= stepCount; step++)
        {
            float z = step * stepDepth;
            float y = step * stepHeight;

            // 4 vertices per step (front-left, front-right, back-left, back-right)
            int baseIndex = verts.Count;
            verts.Add(new(-halfWidth, y, z));
            verts.Add(new(halfWidth, y, z));
            verts.Add(new(-halfWidth, y, z + stepDepth));
            verts.Add(new(halfWidth, y, z + stepDepth));

            if (step > 0)
            {
                int prevBase = baseIndex - 4;
                // Top surface
                tris.Add(prevBase + 2); tris.Add(baseIndex); tris.Add(baseIndex + 2);
                tris.Add(baseIndex); tris.Add(prevBase + 3); tris.Add(baseIndex + 3);
                // Front face
                tris.Add(prevBase); tris.Add(prevBase + 1); tris.Add(baseIndex);
                tris.Add(baseIndex); tris.Add(prevBase + 1); tris.Add(baseIndex + 1);
            }
        }

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mf.mesh = mesh;

        var collider = GetComponent<MeshCollider>();
        if (collider == null) collider = gameObject.AddComponent<MeshCollider>();
        collider.convex = false;
    }
}
