using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class HitboxStateInjector : EditorWindow
{
    private AnimatorController controller;
    private List<string> animationNames = new List<string>();

    [MenuItem("Tools/Inject HitboxActivators")]
    public static void ShowWindow()
    {
        GetWindow<HitboxStateInjector>("Hitbox Injector");
    }

     void OnGUI()
    {
        controller = (AnimatorController)EditorGUILayout.ObjectField("Animator Controller", controller, typeof(AnimatorController), false);

        if (GUILayout.Button("Inject Hitbox Activators"))
        {
            if (controller == null)
            {
                Debug.LogError("Please assign an Animator Controller.");
                return;
            }

            InjectHitboxActivators(controller);
        }
    }

    void InjectHitboxActivators(AnimatorController animatorController)
    {
        //Populate the list of animations that require hitboxes
        animationNames = new List<string>
        {
            "LeftJab", "LeftCross", "LeftHook",
            "RightJab", "RightCross", "RightHook",
            "LeftUppercut", "RightUppercut", "LeadUppercut", "RearUppercut",
            "RightElbow", "LeftElbow", "RightUpwardsElbow",
            "LeadKnee", "RearKnee",
            "LowKick", "MidRoundhouseKick", "HighRoundhouseKick",
            "SpinningHookKick", "SideKick",
            "LeadTeep", "RearTeep",
            "ComboPunch"
        };

        foreach (var layer in animatorController.layers)
        {
            InjectBehaviours(layer.stateMachine);
        }

        EditorUtility.SetDirty(animatorController);
        AssetDatabase.SaveAssets();
        Debug.Log("Hitbox injection complete");
    }

    // Recursive function -- iterating through the Animator's layers and states to inject a HitboxActivator and animationName 
    void InjectBehaviours(AnimatorStateMachine stateMachine)
    {
        foreach (var state in stateMachine.states)
        {
            string stateName = state.state.name;

            if (animationNames.Contains(stateName))
            {
                var alreadyHasStateAttached = false;
                foreach (var behaviour in state.state.behaviours)
                {
                    if (behaviour is HitboxActivator)
                    {
                        alreadyHasStateAttached = true;
                        break;
                    }
                }

                if (!alreadyHasStateAttached)
                {
                    var behaviour = state.state.AddStateMachineBehaviour<HitboxActivator>();
                    behaviour.animationName = stateName;
                }
            }
        }

        // Recurse into substate machines if any
        foreach (var subStateMachine in stateMachine.stateMachines)
        {
            InjectBehaviours(subStateMachine.stateMachine);
        }
    }
}

