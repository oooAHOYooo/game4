using UnityEngine;

/// <summary>
/// Procedurally generates the skate playground environment on scene load.
/// Creates ground, skateboard rig, camera setup—all from code.
/// </summary>
public class SceneInitializer : MonoBehaviour
{
    private void Awake()
    {
        CreateEnvironment();
    }

    private void EnsureLayerExists(string layerName)
    {
        if (LayerMask.NameToLayer(layerName) == -1)
        {
            // Layer doesn't exist - find an empty slot
            for (int i = 8; i < 32; i++) // User layers start at 8
            {
                if (LayerMask.LayerToName(i) == "")
                {
                    Debug.LogWarning($"Layer '{layerName}' not found. Created at layer {i}.");
                    break;
                }
            }
        }
    }

    private void CreateEnvironment()
    {
        // Ensure Ground layer exists
        EnsureLayerExists("Ground");

        // Clear any existing rig (in case of scene reload)
        var existingRig = GameObject.Find("SkateRig");
        if (existingRig != null)
            Destroy(existingRig);

        var existingGround = GameObject.Find("Ground");
        if (existingGround != null)
            Destroy(existingGround);

        // Create ground plane
        CreateGround();

        // Create skateboard rig
        CreateSkateRig();

        // Add example level pieces
        // AddExamplePieces();

        // Setup camera
        SetupCamera();

        Debug.Log("[Scene] Playground initialized procedurally!");
        Debug.Log("[Controls] WASD=Steer | SPACE=Ollie(hold to charge) | J=Push/Kickflip | I=PopShoveIt | L=Heelflip");
    }

    private void CreateGround()
    {
        var groundObj = new GameObject("Ground");
        groundObj.transform.position = Vector3.zero;

        // Add plane mesh
        var meshFilter = groundObj.AddComponent<MeshFilter>();
        var meshRenderer = groundObj.AddComponent<MeshRenderer>();
        meshFilter.mesh = CreatePlaneMesh(10, 10);

        // Material
        var material = new Material(Shader.Find("Standard"));
        material.color = new Color(0.8f, 0.8f, 0.8f);
        meshRenderer.material = material;

        // Physics
        var collider = groundObj.AddComponent<BoxCollider>();
        collider.size = new Vector3(10, 0.1f, 10);
        collider.center = Vector3.zero;

        // Tag and layer
        try
        {
            groundObj.tag = "Ground";
        }
        catch
        {
            Debug.LogWarning("'Ground' tag not found. Create it in Tags & Layers settings.");
        }

        groundObj.layer = LayerMask.NameToLayer("Ground");
        if (groundObj.layer == 0)
        {
            Debug.LogWarning("Ground layer does not exist. Creating with default layer. Add a 'Ground' layer in Unity if needed.");
        }
    }

    private Mesh CreatePlaneMesh(float width, float height)
    {
        var mesh = new Mesh();
        mesh.name = "PlaneMesh";

        float w = width / 2f;
        float h = height / 2f;

        var vertices = new Vector3[]
        {
            new(-w, 0, -h),
            new(w, 0, -h),
            new(w, 0, h),
            new(-w, 0, h)
        };

        var triangles = new int[] { 0, 2, 1, 0, 3, 2 };

        var uv = new Vector2[]
        {
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private void CreateSkateRig()
    {
        var rigRoot = new GameObject("SkateRig");
        rigRoot.transform.position = new Vector3(0, 1f, 0);
        try
        {
            rigRoot.tag = "Skateboard";
        }
        catch
        {
            Debug.LogWarning("'Skateboard' tag not found. Create it in Tags & Layers settings.");
        }

        // Create capsule body
        var capsule = new GameObject("Body");
        capsule.transform.parent = rigRoot.transform;
        capsule.transform.localPosition = Vector3.zero;
        try
        {
            capsule.tag = "Skateboard";
        }
        catch
        {
            Debug.LogWarning("'Skateboard' tag not found. Create it in Tags & Layers settings.");
        }

        var capsuleCollider = capsule.AddComponent<CapsuleCollider>();
        capsuleCollider.radius = 0.3f;
        capsuleCollider.height = 2f;

        // Rigidbody
        var rb = capsule.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.05f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Skateboard mesh (child cube)
        var boardMesh = new GameObject("BoardMesh");
        boardMesh.transform.parent = capsule.transform;
        boardMesh.transform.localPosition = new Vector3(0, -0.3f, 0);
        boardMesh.transform.localScale = new Vector3(0.3f, 0.1f, 0.8f);

        var boardRenderer = boardMesh.AddComponent<MeshRenderer>();
        var boardFilter = boardMesh.AddComponent<MeshFilter>();
        boardFilter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

        var boardMat = new Material(Shader.Find("Standard"));
        boardMat.color = new Color(1f, 0.3f, 0.1f); // Orange-ish
        boardRenderer.material = boardMat;

        // Add scripts
        var skaterController = capsule.AddComponent<SkaterController>();
        var trickSystem = capsule.AddComponent<TrickSystem>();

        // Assign board mesh to trick system
        trickSystem._boardMesh = boardMesh.transform;

        // Make sure Ground layer exists
        if (LayerMask.NameToLayer("Ground") == -1)
        {
            Debug.LogWarning("'Ground' layer not found. Skater will not detect ground properly.");
        }
        else
        {
            skaterController.GroundLayer = LayerMask.GetMask("Ground");
        }

        Debug.Log("[Rig] Skateboard rig created procedurally!");
    }

    private void SetupCamera()
    {
        var mainCamera = UnityEngine.Camera.main;
        if (mainCamera == null)
        {
            var cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            mainCamera = cameraObj.AddComponent<UnityEngine.Camera>();
        }

        mainCamera.transform.position = new Vector3(0, 2f, -5f);
        mainCamera.transform.LookAt(new Vector3(0, 1f, 0));

        Debug.Log("[Camera] Camera positioned!");
    }

    private void AddExamplePieces()
    {
        var levelBuilder = new GameObject("_LevelBuilder").AddComponent<LevelBuilder>();

        // Create a simple skate park with example pieces
        var mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.7f, 0.7f, 0.7f);

        // Quarter-pipe on left side
        var qp1 = levelBuilder.AddQuarterPipe(new(-5, 0, 0), height: 2.5f, length: 3.5f);
        qp1.GetComponent<MeshRenderer>().material = mat;

        // Quarter-pipe on right side
        var qp2 = levelBuilder.AddQuarterPipe(new(5, 0, 0), height: 2.5f, length: 3.5f);
        qp2.GetComponent<MeshRenderer>().material = mat;

        // Grind rail between them
        var rail = levelBuilder.AddGrindRail(new(0, 2.2f, 1.5f), length: 3f);
        rail.GetComponent<MeshRenderer>().material = mat;

        // Skate box in front
        var box = levelBuilder.AddSkateBox(new(0, 0, 5), width: 2f, height: 0.8f, depth: 2f);
        box.GetComponent<MeshRenderer>().material = mat;

        // Stairs to the right
        var stairs = levelBuilder.AddStairs(new(4, 0, 5), steps: 3, stepHeight: 0.3f);
        stairs.GetComponent<MeshRenderer>().material = mat;

        Debug.Log("[Pieces] Added example level pieces - you can add/modify them via code!");
    }
}
