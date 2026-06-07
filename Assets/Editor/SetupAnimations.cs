using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public static class SetupAnimations
{
    [MenuItem("Tools/Setup Animations")]
    public static void Execute()
    {
        Debug.Log("SetupAnimations: Searching for FBX humanoid clips...");
        
        string[] fbxFiles = Directory.GetFiles("Assets/Bandit/fbx", "*.fbx", SearchOption.AllDirectories);
        
        AnimationClip idleClip = null;
        AnimationClip walkClip = null;
        AnimationClip runClip = null;

        foreach (string fbx in fbxFiles)
        {
            string p = fbx.Replace("\\", "/");
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(p);
            foreach (var asset in assets)
            {
                if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                {
                    Debug.Log($"Found clip: {clip.name} in {p}");
                    if (p.ToLower().Contains("idle.fbx")) idleClip = clip;
                    else if (p.ToLower().Contains("walk")) walkClip = clip;
                    else if (p.ToLower().Contains("run")) runClip = clip;
                }
            }
        }

        if (idleClip == null || walkClip == null || runClip == null)
        {
            Debug.LogWarning("Missing some animations! Idle: " + (idleClip != null) + ", Walk: " + (walkClip != null) + ", Run: " + (runClip != null));
            return;
        }

        // Create or load AnimatorController
        string controllerPath = "Assets/Bandit/BanditAnimatorController.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // Add Speed parameter
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

        // Create Blend Tree
        BlendTree blendTree;
        AnimatorState state = controller.CreateBlendTreeInController("Locomotion", out blendTree, 0);
        
        blendTree.blendType = BlendTreeType.Simple1D;
        blendTree.blendParameter = "Speed";
        blendTree.useAutomaticThresholds = false;

        // Add motions
        blendTree.AddChild(idleClip, 0f);
        blendTree.AddChild(walkClip, 2.0f);
        blendTree.AddChild(runClip, 5.33f);

        // Apply controller to Player_Bandit in the scene
        ApplyControllerToSceneObjects(controller);

        Debug.Log("Successfully created AnimatorController with locomotion BlendTree.");
    }

    private static void ApplyControllerToSceneObjects(AnimatorController controller)
    {
        GameObject player = GameObject.Find("Player_Bandit");
        if (player != null)
        {
            var anim = player.GetComponent<Animator>();
            if (anim != null)
            {
                anim.runtimeAnimatorController = controller;
                anim.applyRootMotion = false;
                Debug.Log("Assigned AnimatorController to Player_Bandit and disabled Apply Root Motion.");
            }
        }
    }
}
