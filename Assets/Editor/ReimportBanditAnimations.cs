using UnityEngine;
using UnityEditor;
using System.IO;

public static class ReimportBanditAnimations
{
    [MenuItem("Tools/Reimport Bandit Animations")]
    public static void Execute()
    {
        Debug.Log("Starting Bandit Animations Rig Conversion and Import Settings Update...");

        string[] paths = new[]
        {
            "Assets/Bandit/fbx/Walking.fbx",
            "Assets/Bandit/fbx/Standard Run.fbx",
            "Assets/Bandit/fbx/Idle.fbx"
        };

        foreach (var path in paths)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"File does not exist: {path}");
                continue;
            }

            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError($"Could not load ModelImporter for {path}");
                continue;
            }

            // 1. Force Rig Type to Humanoid
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true;
            
            // Disable keyframe compression to ensure full precision and smooth play
            importer.animationCompression = ModelImporterAnimationCompression.Off;
            importer.resampleCurves = true;

            // 2. Setup Clip Animations (Loop settings)
            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            if (clips == null || clips.Length == 0)
            {
                // Fallback to manually creating clip animation if defaults are null
                var newClip = new ModelImporterClipAnimation();
                newClip.name = Path.GetFileNameWithoutExtension(path);
                newClip.firstFrame = 0;
                newClip.lastFrame = 100;
                clips = new[] { newClip };
            }

            for (int i = 0; i < clips.Length; i++)
            {
                clips[i].loopTime = true;
                clips[i].loopPose = true;
                
                // Bake rotation and height
                clips[i].lockRootRotation = true;
                clips[i].keepOriginalOrientation = false;
                
                clips[i].lockRootHeightY = true;
                clips[i].keepOriginalPositionY = true;
                
                // Bake XZ into pose for in-place movement
                clips[i].lockRootPositionXZ = true;
                clips[i].keepOriginalPositionXZ = false;
            }

            importer.clipAnimations = clips;

            // Save and reimport
            importer.SaveAndReimport();
            Debug.Log($"Successfully converted {path} to Humanoid Rig and configured loop settings.");
        }

        Debug.Log("Bandit Animations Rig Conversion Complete!");
    }
}
