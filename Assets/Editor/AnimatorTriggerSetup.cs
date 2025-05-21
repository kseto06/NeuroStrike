using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Collections.Generic;

public class AnimatorTriggerSetup : EditorWindow
{
    private AnimatorController controller;

    private List<string> hitboxAnimations = new List<string>
    {
        "LeftJab", "LeftCross", "LeftHook",
        "RightJab", "RightCross", "RightHook",
        "LeftUppercut", "RightUppercut", "LeadUppercut", "RearUppercut",
        "RightElbow", "LeftElbow", "RightUpwardsElbow",
        "LeadKnee", "RearKnee",
        "LowKick", "MidRoundhouseKick", "HighRoundhouseKick",
        "SpinningHookKick", "SideKick",
        "LeadTeep", "RearTeep",
        "ComboPunch", "Block", "Idle",
        "StepBackward", "ShortStepForward", "MediumStepForward", "LongStepForward",
        "ShortRightSideStep", "ShortLeftSideStep", "MediumRightSideStep",
        "MediumLeftSideStep", "LongRightSideStep", "LongLeftSideStep",
        "LeftPivot", "RightPivot"
    };


    [MenuItem("Tools/Set Animator Triggers")]
    public static void ShowWindow()
    {
        GetWindow<AnimatorTriggerSetup>("Animator Trigger Setup");
    }

    void OnGUI()
    {
        controller = (AnimatorController)EditorGUILayout.ObjectField("Animator Controller", controller, typeof(AnimatorController), false);

        if (GUILayout.Button("Add Triggers & Transitions"))
        {
            if (controller == null)
            {
                Debug.LogError("No Animator Controller selected");
                return;
            }

            AddTriggersAndTransitions(controller);
        }
    }

    void AddTriggersAndTransitions(AnimatorController animatorController)
    {
        var rootStateMachine = animatorController.layers[0].stateMachine;

        foreach (var animName in hitboxAnimations)
        {
            // Add Trigger parameter to each animation
            if (!HasParameter(animatorController, animName))
            {
                animatorController.AddParameter(animName, AnimatorControllerParameterType.Trigger);
            }

            // Find the animation state in the controller
            AnimatorState targetState = FindStateByName(rootStateMachine, animName);
            if (targetState == null)
            {
                Debug.LogWarning($"State not found for animation: {animName}");
                continue;
            }

            // Add AnyState -> TargetState transitions
            bool hasTransition = false;
            foreach (var transition in rootStateMachine.anyStateTransitions)
            {
                if (transition.destinationState == targetState)
                {
                    hasTransition = true;
                    break;
                }
            }

            if (!hasTransition)
            {
                var transition = rootStateMachine.AddAnyStateTransition(targetState);
                transition.hasExitTime = false;
                transition.hasFixedDuration = true;
                transition.conditions = new AnimatorCondition[] {
                    new AnimatorCondition() {
                        mode = AnimatorConditionMode.If,
                        parameter = animName,
                        threshold = 0
                    }
                };
            }
        }

        EditorUtility.SetDirty(animatorController);
        AssetDatabase.SaveAssets();
        Debug.Log("Animator triggers are setup");
    }

    bool HasParameter(AnimatorController controller, string paramName)
    {
        foreach (var param in controller.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    AnimatorState FindStateByName(AnimatorStateMachine stateMachine, string name)
    {
        foreach (var child in stateMachine.states)
        {
            if (child.state.name == name)
                return child.state;
        }
        return null;
    }
}
