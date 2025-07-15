using System.Collections.Generic;
using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    [System.Serializable]
    public class Box
    {
        public string name;
        public string path;
        public float radius;
        public float height;
        public Vector3 offset;
    }

    public List<Box> hurtboxes;
    public PhysicsMaterial hurtboxMat;
    public Color outlineColor = new Color(1f, 0f, 0f, 0.3f); //red
    
    private List<GameObject> colliders = new List<GameObject>();

    void Start()
    {
        //Define boxes
        hurtboxes = new List<Box>
        {
            //Chest & Hip Area:
            new Box { name = "Spine1", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1", radius = 0.175f, height = 0.45f, offset = new Vector3(0f, -0.15f, 0f) },
            new Box { name = "Spine2", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2", radius = 0.17f, height = 0.4f, offset = new Vector3(0f, 0f, 0.05f) }, //Chest?

            //Neck & Head:
            new Box { name = "Neck", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck", radius = 0.07f, height = 0.2f, offset = Vector3.zero },
            new Box { name = "Head", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head", radius = 0.14f, height = 0.3f, offset = new Vector3(0f, 0.055f, 0.01f) },

            //Arms:
            new Box { name = "RightShoulder", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder", radius = 0.1f, height = 0.25f, offset = new Vector3(0f, 0.05f, 0f)  },
            new Box { name = "RightArm", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm", radius = 0.07f, height = 0.4f, offset = new Vector3(0f, 0.15f, 0f) },
            new Box { name = "RightElbow", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm", radius = 0.07f, height = 0f, offset = Vector3.zero },
            new Box { name = "RightForeArm", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm", radius = 0.045f, height = 0.3f, offset = new Vector3(0f, 0.15f, 0f) },
            new Box { name = "RightHand", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand", radius = 0.075f, height = 0.1f, offset = new Vector3(0f, 0.07f, 0.02f)},

            new Box { name = "LeftShoulder", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder", radius = 0.1f, height = 0.25f, offset = new Vector3(0f, 0.05f, 0f)  },
            new Box { name = "LeftArm", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm", radius = 0.07f, height = 0.4f, offset = new Vector3(0f, 0.15f, 0f) },
            new Box { name = "LeftElbow", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm", radius = 0.07f, height = 0f, offset = Vector3.zero },
            new Box { name = "LeftForeArm", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm", radius = 0.045f, height = 0.3f, offset = new Vector3(0f, 0.15f, 0f) },
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
    
        //Init hurtboxes
        foreach (var box in hurtboxes) {
            var bone = transform.Find(box.path);
            if (bone == null) {
                Debug.LogWarning($"{box.path} not found for {box.name}");
                continue;
            }

            //Define the hurtbox GameObjects and Capsule Colliders
            GameObject hurtbox = new GameObject($"Hurtbox_{box.name}");
            hurtbox.transform.SetParent(bone);
            hurtbox.transform.localPosition = Vector3.zero; //box.offset;
            hurtbox.transform.localRotation = Quaternion.identity;

            CapsuleCollider collider = hurtbox.AddComponent<CapsuleCollider>();
            collider.radius = box.radius;
            collider.height = box.height;
            collider.center = box.offset; //Vector3.zero;
            collider.direction = 1; // Y-axis
            collider.isTrigger = true;
            if (hurtboxMat) 
                collider.material = hurtboxMat;

            //Hurtbox drawing in the Scene
            var outline = hurtbox.AddComponent<Outline>();
            outline.radius = box.radius;
            outline.height = box.height;
            outline.outlineColor = outlineColor;

            hurtbox.AddComponent<HurtboxTrigger>().boxName = box.name;
            colliders.Add(hurtbox);
        }
    }
}

public class HurtboxTrigger : MonoBehaviour
{
    public string boxName;
    private Animator animator;
    private AnimationController animationController;
    private SparringAgent agent;
    private SparringEnvController envController;

    private void Start()
    {
        animator = GetComponentInParent<Animator>();
        animationController = GetComponentInParent<AnimationController>();
        agent = GetComponentInParent<SparringAgent>();
        envController = GetComponentInParent<SparringEnvController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        /*
            Function to handle hitbox collisions with detected hurtboxes
            Calls EnvController functions to handle hit & block rewards
        */

        if (other.CompareTag("Hitbox") && !animator.GetCurrentAnimatorStateInfo(0).IsName("Block"))
        {
            Debug.Log($"Hurtbox {boxName} hit by {other.name}");

            string hurtAction = null;

            if (boxName == "Head" || boxName == "Neck")
            {
                hurtAction = "HeadHit";
            }
            else if (boxName == "Spine1" || boxName == "Spine2")
            {
                hurtAction = "BodyHit";
            }
            else if (boxName.Contains("Right"))
            {
                hurtAction = "RightSideHit";
            }
            else if (boxName.Contains("Left"))
            {
                hurtAction = "LeftSideHit";
            }

            if (!string.IsNullOrEmpty(hurtAction))
            {   
                //Assign new action
                var agent = GetComponentInParent<SparringAgent>();
                if (agent != null)
                {
                    agent.inputAction = hurtAction;
                }

                //Accumulate hit rewards
                envController.AttackLandedReward(agent.team, hurtAction);
            }
        }
        else if (other.CompareTag("Hitbox") && animator.GetCurrentAnimatorStateInfo(0).IsName("Block"))
        {
            envController.AttackBlockedReward(agent.team, true);
            Debug.Log($"{boxName} blocked hit by {other.name}");
        }
    }
}

public class Outline : MonoBehaviour
{
    public Color outlineColor = new Color(1f, 0f, 0f, 0.7f); //red
    public float radius = 0.1f;
    public float height = 0.3f;

    private void OnDrawGizmos()
    {
        Gizmos.color = outlineColor;

        //Position and scale gizmo outline
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.matrix = rotationMatrix;

        // Cylinder drawing approximation for capsule
        Gizmos.DrawWireCube(Vector3.zero + Vector3.up * (height / 2f), new Vector3(radius * 2, height, radius * 2));
    }
}