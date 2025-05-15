// Helper class to auto-convert all Generic rigs to Humanoid
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public class RigConverter : EditorWindow {
    [MenuItem("Tools/Convert FBX to Humanoid")]
    static void ConvertSelectedFBX() {
        foreach (Object obj in Selection.objects) {
            string path = AssetDatabase.GetAssetPath(obj);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;

            if (importer != null && importer.animationType != ModelImporterAnimationType.Human) {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.SaveAndReimport();
            } else {
                Debug.Log($"Error in converting {obj.name} to humanoid rig");
            }
        }
    }
}