#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSetupMenu
{
    [MenuItem("Skate/Setup Playground Scene")]
    public static void SetupPlaygroundScene()
    {
        // Create the Ground layer if it doesn't exist
        EnsureLayerExists("Ground");

        // Create a new scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Add SceneInitializer to an empty GameObject
        var initializerObj = new GameObject("_SceneInitializer");
        initializerObj.AddComponent<SceneInitializer>();

        // Save scene
        string scenePath = "Assets/Scenes/Playground.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);

        Debug.Log($"[Editor] Playground scene created at {scenePath}");
        Debug.Log("[Editor] Scene will auto-generate on play!");
    }

    [MenuItem("Skate/Set Playground as Default Scene")]
    public static void SetPlaygroundAsDefault()
    {
        var scenes = EditorBuildSettings.scenes;
        string playgroundPath = "Assets/Scenes/Playground.unity";

        // Check if playground is already in build settings
        bool found = false;
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].path == playgroundPath)
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            // Add to build settings at index 0 (first scene)
            var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            newScenes[0] = new EditorBuildSettingsScene(playgroundPath, true);
            System.Array.Copy(scenes, 0, newScenes, 1, scenes.Length);
            EditorBuildSettings.scenes = newScenes;
            Debug.Log("[Editor] Playground set as default (index 0) in Build Settings");
        }
        else
        {
            Debug.Log("[Editor] Playground already in Build Settings!");
        }
    }

    [MenuItem("Skate/Open Playground")]
    public static void OpenPlayground()
    {
        if (EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "", false))
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Playground.unity", OpenSceneMode.Single);
        }
    }

    private static void EnsureLayerExists(string layerName)
    {
        if (LayerMask.NameToLayer(layerName) == -1)
        {
            // Layer doesn't exist, try to create it
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProp = tagManager.FindProperty("layers");

            for (int i = 8; i < layersProp.arraySize; i++)
            {
                var layer = layersProp.GetArrayElementAtIndex(i);
                if (layer.stringValue == "")
                {
                    layer.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    Debug.Log($"[Editor] Created '{layerName}' layer at index {i}");
                    return;
                }
            }
        }
        else
        {
            Debug.Log($"[Editor] Layer '{layerName}' already exists");
        }
    }
}
#endif
