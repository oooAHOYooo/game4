#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public class PrefabGenerator
{
    private static Material _defaultMat;
    private static readonly string FolderDir = "Assets/Prefabs/Ramps";
    private static readonly string MeshFolderDir = "Assets/Prefabs/Ramps/Meshes";

    static PrefabGenerator()
    {
        EditorApplication.delayCall += () =>
        {
            if (!Directory.Exists(FolderDir) || Directory.GetFiles(FolderDir, "*.prefab").Length == 0)
            {
                GeneratePrefabs();
            }
        };
    }

    [MenuItem("Skate/Generate Level Prefabs")]
    public static void GeneratePrefabs()
    {
        if (!Directory.Exists(FolderDir))
        {
            Directory.CreateDirectory(FolderDir);
            AssetDatabase.Refresh();
        }
        if (!Directory.Exists(MeshFolderDir))
        {
            Directory.CreateDirectory(MeshFolderDir);
            AssetDatabase.Refresh();
        }

        // We use the Standard shader or find an existing one
        _defaultMat = new Material(Shader.Find("Standard"));
        _defaultMat.color = new Color(0.6f, 0.6f, 0.6f);

        // 0. Ground Plane
        var ground = new GameObject("Ground");
        var groundMf = ground.AddComponent<MeshFilter>();
        groundMf.mesh = CreatePlaneMesh(150, 150);
        var groundCol = ground.AddComponent<BoxCollider>();
        groundCol.size = new Vector3(150, 0.1f, 150);
        groundCol.center = Vector3.zero;
        SetupRenderer(ground);
        // Force the Ground tag so the controller works flawlessly
        try { ground.tag = "Ground"; } catch { }
        SaveAsPrefab(ground, $"{FolderDir}/Ground.prefab");

        // 1. Skate Box
        var box = new GameObject("SkateBox");
        box.AddComponent<SkateBox>();
        SetupRenderer(box);
        SaveAsPrefab(box, $"{FolderDir}/SkateBox.prefab");

        // 2. Quarter Pipe
        var qp = new GameObject("QuarterPipe");
        qp.AddComponent<QuarterPipe>();
        SetupRenderer(qp);
        SaveAsPrefab(qp, $"{FolderDir}/QuarterPipe.prefab");

        // 3. Half Pipe
        var hp = new GameObject("HalfPipe");
        hp.AddComponent<HalfPipe>();
        SetupRenderer(hp);
        SaveAsPrefab(hp, $"{FolderDir}/HalfPipe.prefab");

        // 4. Grind Rail
        var rail = new GameObject("GrindRail");
        rail.AddComponent<GrindRail>();
        SetupRenderer(rail);
        SaveAsPrefab(rail, $"{FolderDir}/GrindRail.prefab");

        // 5. Stairs
        var stairs = new GameObject("Stairs");
        stairs.AddComponent<Stairs>();
        SetupRenderer(stairs);
        SaveAsPrefab(stairs, $"{FolderDir}/Stairs.prefab");

        // Optional flat surface
        var flatObj = new GameObject("FlatSurface");
        var mf = flatObj.AddComponent<MeshFilter>();
        mf.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        flatObj.transform.localScale = new Vector3(5, 0.1f, 10);
        SetupRenderer(flatObj);
        flatObj.AddComponent<BoxCollider>().center = Vector3.zero;
        SaveAsPrefab(flatObj, $"{FolderDir}/FlatSurface.prefab");

        Debug.Log($"[PrefabGenerator] Generated draggable prefabs in {FolderDir}!");
    }

    private static void SetupRenderer(GameObject obj)
    {
        obj.layer = LayerMask.NameToLayer("Ground") > -1 ? LayerMask.NameToLayer("Ground") : 0;
        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = _defaultMat;
    }

    private static void SaveAsPrefab(GameObject obj, string localPath)
    {
        // 1. Force the custom procedural scripts to generate their meshes NOW
        // by calling their Awake/Start methods or manually extracting the generated mesh.
        // Actually, since AddComponent runs Awake immediately, the meshes should be on the MeshFilters.

        // 2. Save all generated meshes as .asset files first, otherwise Unity loses them
        MeshFilter[] filters = obj.GetComponentsInChildren<MeshFilter>(true);
        foreach (var mf in filters)
        {
            if (mf.sharedMesh != null && !AssetDatabase.Contains(mf.sharedMesh))
            {
                // Create an asset for this mesh
                string meshPath = $"{MeshFolderDir}/{obj.name}_{mf.gameObject.name}_Mesh.asset";
                meshPath = AssetDatabase.GenerateUniqueAssetPath(meshPath);
                
                // Clone the mesh so we don't accidentally modify a primitive
                Mesh newMesh = Object.Instantiate(mf.sharedMesh);
                newMesh.name = $"{obj.name}_Mesh";
                AssetDatabase.CreateAsset(newMesh, meshPath);
                
                // Re-assign the saved mesh back to the filter
                mf.sharedMesh = newMesh;
                
                // If there's a MeshCollider, update it too
                var mc = mf.GetComponent<MeshCollider>();
                if (mc != null) mc.sharedMesh = newMesh;
            }
        }

        // Now save the Prefab
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        PrefabUtility.SaveAsPrefabAsset(obj, localPath, out bool success);
        GameObject.DestroyImmediate(obj);
    }

    private static Mesh CreatePlaneMesh(float width, float height)
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
}
#endif
