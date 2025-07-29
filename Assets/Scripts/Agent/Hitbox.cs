using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [System.Serializable]
    public class Box 
    {
        public string name;
        public string path;
        public float radius;
        public float height;
        public Vector3 offset;
        public GameObject collider;
    }

    public List<Box> hitboxes;
    public PhysicsMaterial hitboxMat;
    public Color outlineColor = Color.cyan;

    private Dictionary<string, List<string>> hitboxMap;

    void Start() 
    {
        //Init hitboxes
        hitboxes = new List<Box>
        {
            //Arms:
            new Box { name = "RightElbow", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm", radius = 0.07f, height = 0f, offset = Vector3.zero },
            new Box { name = "RightHand", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand", radius = 0.075f, height = 0.1f, offset = new Vector3(0f, 0.07f, 0.02f)},

            new Box { name = "LeftElbow", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm", radius = 0.07f, height = 0f, offset = Vector3.zero },
            new Box { name = "LeftHand", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand", radius = 0.075f, height = 0.1f, offset = new Vector3(0f, 0.07f, 0.02f)},

            //Legs:
            new Box { name = "RightThigh", path = "mixamorig:Hips/mixamorig:RightUpLeg", radius = 0.12f, height = 0.5f, offset = new Vector3(0f, 0.2f, 0f)}, //Thigh
            new Box { name = "RightKnee", path = "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg", radius = 0.08f, height = 0f, offset = Vector3.zero }, //Knee
            new Box { name = "RightCalf", path = "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg", radius = 0.07f, height = 0.5f, offset = new Vector3(0f, 0.2f, 0f) }, //Calf
            new Box { name = "RightFoot", path = "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg/mixamorig:RightFoot", radius = 0.05f, height = 0.25f, offset = new Vector3(0f, 0.05f, 0f) },
            new Box { name = "RightToeBase", path = "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg/mixamorig:RightFoot/mixamorig:RightToeBase", radius = 0.05f, height = 0.3f, offset = new Vector3(0f, -0.07f, 0f) },

            new Box { name = "LeftThigh", path = "mixamorig:Hips/mixamorig:LeftUpLeg", radius = 0.12f, height = 0.5f, offset = new Vector3(0f, 0.2f, 0f)}, //Thigh
            new Box { name = "LeftKnee", path = "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg", radius = 0.08f, height = 0f, offset = Vector3.zero }, //Knee
            new Box { name = "LeftCalf", path = "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg", radius = 0.07f, height = 0.5f, offset = new Vector3(0f, 0.2f, 0f) }, //Calf
            new Box { name = "LeftFoot", path = "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg/mixamorig:LeftFoot", radius = 0.05f, height = 0.25f, offset = new Vector3(0f, 0.05f, 0f) },
            new Box { name = "LeftToeBase", path = "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg/mixamorig:LeftFoot/mixamorig:LeftToeBase", radius = 0.05f, height = 0.3f, offset = new Vector3(0f, -0.07f, 0f) },
        };

        //Init hitbox colliders
        foreach (var box in hitboxes) 
        {
            var bone = transform.Find(box.path);
            if (bone == null) 
            {
                Debug.LogWarning($"{box.path} not found for {box.name}");
                continue;
            }

            GameObject hitbox = new GameObject($"Hitbox_{box.name}");
            hitbox.transform.SetParent(bone);
            hitbox.transform.localPosition = Vector3.zero;
            hitbox.transform.localRotation = Quaternion.identity;

            CapsuleCollider collider = hitbox.AddComponent<CapsuleCollider>();
            collider.radius = box.radius;
            collider.height = box.height;
            collider.center = box.offset;
            collider.direction = 1; // Y-axis
            collider.isTrigger = true;
            if (hitboxMat)
                collider.material = hitboxMat;
            

            var outline = hitbox.AddComponent<Outline>();
            outline.radius = box.radius;
            outline.height = box.height;
            outline.outlineColor = outlineColor;

            hitbox.tag = "Hitbox";
            hitbox.SetActive(false); //Disable hitbox on init
            box.collider = hitbox;
        }

        // Attack animation and hitbox mapping
        hitboxMap = new Dictionary<string, List<string>>
        {
            { "LeftJab", new List<string>{ "LeftHand" } },
            { "LeftCross", new List<string>{ "LeftHand" } },
            { "LeftHook", new List<string>{ "LeftHand" } },
            { "RightJab", new List<string>{ "RightHand" } },
            { "RightCross", new List<string>{ "RightHand" } },
            { "RightHook", new List<string>{ "RightHand" } },
            { "LeftUppercut", new List<string>{ "LeftHand" } },
            { "RightUppercut", new List<string>{ "RightHand" } },
            { "LeadUppercut", new List<string>{ "LeftHand" } },
            { "RearUppercut", new List<string>{ "RightHand" } },
            { "RightElbow", new List<string>{ "RightElbow" } },
            { "LeftElbow", new List<string>{ "LeftElbow" } },
            { "RightUpwardsElbow", new List<string>{ "RightElbow" } },
            { "LeadKnee", new List<string>{ "LeftKnee" } },
            { "RearKnee", new List<string>{ "RightKnee" } },
            { "LowKick", new List<string>{ "RightCalf", "RightFoot", "RightToeBase" } },
            { "MidRoundhouseKick", new List<string>{ "RightCalf", "RightFoot", "RightToeBase" } },
            { "HighRoundhouseKick", new List<string>{ "RightCalf", "RightFoot", "RightToeBase" } },
            { "SpinningHookKick", new List<string>{ "RightFoot", "RightToeBase" } },
            { "SideKick", new List<string>{ "LeftFoot", "LeftToeBase" } },
            { "LeadTeep", new List<string>{ "LeftFoot", "LeftToeBase" } },
            { "RearTeep", new List<string>{ "RightFoot", "RightToeBase" } },
            { "ComboPunch", new List<string>{ "LeftHand", "RightHand" } },
            { "Block", new List<string>() },
            { "StepBackward", new List<string>() },
            { "ShortStepForward", new List<string>() },
            { "MediumStepForward", new List<string>() },
            { "LongStepForward", new List<string>() },
            { "ShortRightSideStep", new List<string>() },
            { "ShortLeftSideStep", new List<string>() },
            { "MediumRightSideStep", new List<string>() },
            { "MediumLeftSideStep", new List<string>() },
            { "LongRightSideStep", new List<string>() },
            { "LongLeftSideStep", new List<string>() },
            { "LeftPivot", new List<string>() },
            { "RightPivot", new List<string>() },
            { "Idle", new List<string>() },
        };
    }

    //Function for hitbox activation based on current animation
    public void ActivateHitboxes(string animationName) 
    {
        foreach (var box in hitboxes) 
        {
            box.collider.SetActive(false);
        }

        if (hitboxMap.TryGetValue(animationName, out var activeNames)) 
        {
            foreach (var name in activeNames) 
            {
                var box = hitboxes.Find(b => b.name == name);
                if (box != null) 
                {
                    box.collider.SetActive(true);
                }
            }
        } 
        else 
        {
            Debug.LogWarning($"No hitboxes found for animation: {animationName}");
        }
    }
    
    public void DeactivateHitboxes() 
    {
        foreach (var box in hitboxes) 
        {
            box.collider.SetActive(false);
        }
    }
}