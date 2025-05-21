using UnityEngine;
using UnityEditor;
using System.IO;

public class AnimationBakeIntoPose
{
    [MenuItem("Tools/Bake Into Pose for Selected FBX")]
    static void BakeIntoPoseSelected()
    {
        Object[] selectedObjects = Selection.objects;

        foreach (Object obj in selectedObjects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                continue;

            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) continue;

            var clips = importer.defaultClipAnimations;
            bool changed = false;

            for (int i = 0; i < clips.Length; i++)
            {
                clips[i].lockRootRotation = true;
                clips[i].keepOriginalOrientation = true;

                clips[i].lockRootHeightY = false;
                clips[i].keepOriginalPositionY = true;

                clips[i].lockRootPositionXZ = false;
                clips[i].keepOriginalPositionXZ = false;

                changed = true;
            }

            if (changed)
            {
                importer.clipAnimations = clips;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
    }

    [MenuItem("Tools/Bake Into Pose for Selected FBX", true)]
    static bool ValidateBakeIntoPoseSelected()
    {
        foreach (Object obj in Selection.objects)
        {
            if (AssetDatabase.GetAssetPath(obj).EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
