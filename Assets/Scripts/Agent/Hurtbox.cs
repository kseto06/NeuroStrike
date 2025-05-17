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
            new Box { name = "Hips", path = "mixamorig:Hips", radius = 0.13f, height = 0.25f, offset = Vector3.zero },
            new Box { name = "Spine", path = "mixamorig:Hips/mixamorig:Spine", radius = 0.085f, height = 0.25f, offset = new Vector3(0f, 0.1f, 0.1f) },
            new Box { name = "Spine1", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1", radius = 0.1f, height = 0.25f, offset = Vector3.zero },
            new Box { name = "Spine2", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2", radius = 0.1f, height = 0.25f, offset = Vector3.zero },
            new Box { name = "Neck", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck", radius = 0.02f, height = 0.05f, offset = Vector3.zero },
            new Box { name = "Head", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head", radius = 0.075f, height = 0.21f, offset = new Vector3(0f, 0f, 0.05f) },

            new Box { name = "LeftShoulder", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder", radius = 0.07f, height = 0.15f, offset = Vector3.zero },
            new Box { name = "LeftArm", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm", radius = 0.05f, height = 0.3f, offset = Vector3.zero },
            new Box { name = "LeftForeArm", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm", radius = 0.04f, height = 0.3f, offset = Vector3.zero },
            new Box { name = "LeftHand", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand", radius = 0.06f, height = 0.13f, offset = Vector3.zero },

            new Box { name = "RightShoulder", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder", radius = 0.07f, height = 0.15f, offset = Vector3.zero },
            new Box { name = "RightArm", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm", radius = 0.05f, height = 0.3f, offset = Vector3.zero },
            new Box { name = "RightForeArm", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm", radius = 0.04f, height = 0.3f, offset = Vector3.zero },
            new Box { name = "RightHand", path = "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand", radius = 0.06f, height = 0.13f, offset = Vector3.zero },

            new Box { name = "LeftUpLeg", path = "mixamorig:Hips/mixamorig:LeftUpLeg", radius = 0.07f, height = 0.4f, offset = Vector3.zero },
            new Box { name = "LeftLeg", path = "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg", radius = 0.09f, height = 0.4f, offset = Vector3.zero },
            new Box { name = "LeftFoot", path = "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg/mixamorig:LeftFoot", radius = 0.08f, height = 0.25f, offset = Vector3.zero },

            new Box { name = "RightUpLeg", path = "mixamorig:Hips/mixamorig:RightUpLeg", radius = 0.07f, height = 0.4f, offset = Vector3.zero },
            new Box { name = "RightLeg", path = "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg", radius = 0.09f, height = 0.4f, offset = Vector3.zero },
            new Box { name = "RightFoot", path = "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg/mixamorig:RightFoot", radius = 0.08f, height = 0.25f, offset = Vector3.zero },
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
            hurtbox.transform.localPosition = box.offset;
            hurtbox.transform.localRotation = Quaternion.identity;

            CapsuleCollider collider = hurtbox.AddComponent<CapsuleCollider>();
            collider.radius = box.radius;
            collider.height = box.height;
            collider.center = Vector3.zero;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hitbox"))
        {
            Debug.Log($"Hurtbox {boxName} hit by {other.name}");
            // On-hit logic/reaction here (for later)
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