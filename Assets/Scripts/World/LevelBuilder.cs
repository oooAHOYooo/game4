using UnityEngine;

/// <summary>
/// Simple level builder - create and place level pieces in the scene
/// Attach to an empty GameObject and use the inspector or call methods to add pieces
/// </summary>
public class LevelBuilder : MonoBehaviour
{
    [SerializeField] private Material defaultMaterial;

    private void Start()
    {
        if (defaultMaterial == null)
        {
            defaultMaterial = new Material(Shader.Find("Standard"));
            defaultMaterial.color = new Color(0.6f, 0.6f, 0.6f);
        }
    }

    /// <summary>Create a quarter-pipe ramp at position</summary>
    public GameObject AddQuarterPipe(Vector3 position, float height = 3f, float length = 4f, float width = 5f)
    {
        var obj = new GameObject("QuarterPipe");
        obj.transform.position = position;
        obj.layer = LayerMask.NameToLayer("Ground") > -1 ? LayerMask.NameToLayer("Ground") : 0;

        var qp = obj.AddComponent<QuarterPipe>();
        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = defaultMaterial;

        return obj;
    }

    /// <summary>Create a grind rail at position</summary>
    public GameObject AddGrindRail(Vector3 position, float length = 8f, float height = 1.5f)
    {
        var obj = new GameObject("GrindRail");
        obj.transform.position = position;
        obj.layer = LayerMask.NameToLayer("Ground") > -1 ? LayerMask.NameToLayer("Ground") : 0;

        var rail = obj.AddComponent<GrindRail>();
        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = defaultMaterial;

        return obj;
    }

    /// <summary>Create a skate box/ledge at position</summary>
    public GameObject AddSkateBox(Vector3 position, float width = 2f, float height = 0.8f, float depth = 3f)
    {
        var obj = new GameObject("SkateBox");
        obj.transform.position = position;
        obj.layer = LayerMask.NameToLayer("Ground") > -1 ? LayerMask.NameToLayer("Ground") : 0;

        var box = obj.AddComponent<SkateBox>();
        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = defaultMaterial;

        return obj;
    }

    /// <summary>Create a half-pipe at position</summary>
    public GameObject AddHalfPipe(Vector3 position, float radius = 3f, float width = 6f)
    {
        var obj = new GameObject("HalfPipe");
        obj.transform.position = position;
        obj.layer = LayerMask.NameToLayer("Ground") > -1 ? LayerMask.NameToLayer("Ground") : 0;

        var hp = obj.AddComponent<HalfPipe>();
        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = defaultMaterial;

        return obj;
    }

    /// <summary>Create stairs at position</summary>
    public GameObject AddStairs(Vector3 position, int steps = 5, float stepHeight = 0.3f, float stepDepth = 0.4f)
    {
        var obj = new GameObject("Stairs");
        obj.transform.position = position;
        obj.layer = LayerMask.NameToLayer("Ground") > -1 ? LayerMask.NameToLayer("Ground") : 0;

        var stairs = obj.AddComponent<Stairs>();
        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = defaultMaterial;

        return obj;
    }

    /// <summary>Create a flat grind surface at position</summary>
    public GameObject AddFlatSurface(Vector3 position, float width = 5f, float length = 10f)
    {
        var obj = new GameObject("FlatSurface");
        obj.transform.position = position;
        obj.layer = LayerMask.NameToLayer("Ground") > -1 ? LayerMask.NameToLayer("Ground") : 0;

        var mf = obj.AddComponent<MeshFilter>();
        mf.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        obj.transform.localScale = new Vector3(width, 0.1f, length);

        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = defaultMaterial;

        var collider = obj.AddComponent<BoxCollider>();
        collider.center = Vector3.zero;

        return obj;
    }
}

/// <summary>
/// Example usage: attach this to an empty GameObject to auto-generate a simple park
/// </summary>
public class AutoGeneratePark : MonoBehaviour
{
    [SerializeField] private bool generateOnStart = true;

    private void Start()
    {
        if (!generateOnStart) return;

        var builder = gameObject.AddComponent<LevelBuilder>();

        // Create a simple skate park layout
        builder.AddQuarterPipe(new(0, 0, 0), height: 2.5f);
        builder.AddQuarterPipe(new(7, 0, 0), height: 2.5f);
        builder.AddGrindRail(new(3.5f, 2f, 2), length: 4f);
        builder.AddSkateBox(new(0, 0, 6), width: 2f, height: 0.8f, depth: 3f);
        builder.AddStairs(new(4, 0, 6), steps: 4);
        builder.AddFlatSurface(new(0, 0, -5), width: 15f, length: 2f);

        Debug.Log("[Park] Auto-generated skate park!");
    }
}
