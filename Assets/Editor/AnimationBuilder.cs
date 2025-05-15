using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using System.IO;

public class AnimationBuilder : EditorWindow 
{
    private DefaultAsset animationBuilder;
    private string controllerName = "GeneratedAnimator";

    [MenuItem("Tools/Build Animations from FBX")]
    static void Build() {
        string folderPath = "Assets/Characters/Animations";
        string savePath = folderPath + "/Animator.controller"; //Controller to save all states
        string[] fbxPaths = Directory.GetFiles(folderPath, "*.fbx");

        if (fbxPaths.Length == 0) {
            Debug.LogWarning("No FBX animations found");
            return;
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(savePath);
        AnimatorStateMachine sm = controller.layers[0].stateMachine;

        bool isFirst = true;
        foreach (string path in fbxPaths) {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

            if (clip == null) {
                continue;
            } 

            string name = Path.GetFileNameWithoutExtension(path);
            AnimatorState state = sm.AddState(name);
            state.motion = clip;

            if (isFirst) {
                sm.defaultState = state;
                isFirst = false;
            }

            sm.AddAnyStateTransition(state);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Animator controller generated");
    }
}