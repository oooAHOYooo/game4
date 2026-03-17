#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Automatically initializes the Playground scene on first launch.
/// Runs on editor startup if Playground.unity doesn't exist.
/// </summary>
public class AutoInitialize
{
    private static bool _hasInitialized = false;

    [InitializeOnLoadMethod]
    public static void OnEditorLoad()
    {
        if (_hasInitialized)
            return;

        _hasInitialized = true;

        // Check if Playground scene exists
        if (!System.IO.File.Exists("Assets/Scenes/Playground.unity"))
        {
            Debug.Log("[AutoInit] First launch detected. Setting up Playground scene...");
            EditorApplication.delayCall += CreatePlaygroundScene;
        }
        else
        {
            Debug.Log("[AutoInit] Playground.unity exists. Ready to play!");
        }
    }

    private static void CreatePlaygroundScene()
    {
        // Create Ground layer if needed
        EnsureLayerExists("Ground");

        // Create the scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Add SceneInitializer
        var initObj = new GameObject("_SceneInitializer");
        initObj.AddComponent<SceneInitializer>();

        // Save scene
        string scenePath = "Assets/Scenes/Playground.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);

        Debug.Log($"[AutoInit] ✅ Scene created at {scenePath}");

        // Set as default in Build Settings
        var scenes = EditorBuildSettings.scenes;
        if (scenes.Length == 0 || scenes[0].path != scenePath)
        {
            var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            newScenes[0] = new EditorBuildSettingsScene(scenePath, true);
            System.Array.Copy(scenes, 0, newScenes, 1, scenes.Length);
            EditorBuildSettings.scenes = newScenes;
            Debug.Log("[AutoInit] ✅ Set as default scene in Build Settings");
        }

        // Load the scene
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        Debug.Log("[AutoInit] ✅ Scene loaded and ready!");
        Debug.Log("[AutoInit] 🛹 READY TO PLAY — press Play button or Space!");
    }

    private static void EnsureLayerExists(string layerName)
    {
        if (LayerMask.NameToLayer(layerName) != -1)
            return;

        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layersProp = tagManager.FindProperty("layers");

        for (int i = 8; i < layersProp.arraySize; i++)
        {
            var layer = layersProp.GetArrayElementAtIndex(i);
            if (layer.stringValue == "")
            {
                layer.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                Debug.Log($"[AutoInit] Created '{layerName}' layer at index {i}");
                return;
            }
        }
    }

    /// <summary>
    /// Called from command line: -executeMethod AutoInitialize.StartPlaytest
    /// </summary>
    public static void StartPlaytest()
    {
        Debug.Log("[AutoInit] 🛹 Starting playtest from command line...");

        // Ensure scene exists
        if (!System.IO.File.Exists("Assets/Scenes/Playground.unity"))
        {
            CreatePlaygroundScene();
        }

        // Load Playground scene
        EditorSceneManager.OpenScene("Assets/Scenes/Playground.unity", OpenSceneMode.Single);

        // Enter Play mode
        EditorApplication.delayCall += () =>
        {
            EditorApplication.isPlaying = true;
            Debug.Log("[AutoInit] 🎮 PLAY MODE ACTIVE!");
        };
    }
}
#endif
