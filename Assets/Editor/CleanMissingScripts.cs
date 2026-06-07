using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class CleanMissingScripts
{
    [MenuItem("Tools/Clean Missing Scripts")]
    public static void Execute()
    {
        Debug.Log("CleanMissingScripts: Scanning scene GameObjects for missing scripts...");
        
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int totalRemoved = 0;
        int objectsCleaned = 0;

        foreach (GameObject go in allObjects)
        {
            int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (missingCount > 0)
            {
                Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");
                int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                totalRemoved += removed;
                objectsCleaned++;
                Debug.Log($"Removed {removed} missing script(s) from '{go.name}' at path '{GetGameObjectPath(go)}'");
                EditorUtility.SetDirty(go);
            }
        }

        // Also check if VirtualCameras is still needed; it was only used for Cinemachine vCams.
        // Let's delete it if all its children components are cleared or if it contains nothing useful.
        GameObject vc = GameObject.Find("VirtualCameras");
        if (vc != null)
        {
            // If it is just an empty container or contains Cinemachine vCams that are now empty, delete it
            Undo.DestroyObjectImmediate(vc);
            Debug.Log("Deleted unused 'VirtualCameras' GameObject.");
        }

        GameObject timelines = GameObject.Find("Timelines");
        if (timelines != null)
        {
            Undo.DestroyObjectImmediate(timelines);
            Debug.Log("Deleted unused 'Timelines' GameObject (Cinemachine/Timeline cinemantics).");
        }

        if (objectsCleaned > 0)
        {
            Debug.Log($"Successfully cleaned {totalRemoved} missing script(s) across {objectsCleaned} GameObject(s).");
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
        else
        {
            Debug.Log("No GameObjects with missing scripts found in the scene.");
        }
    }

    private static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return path;
    }
}
