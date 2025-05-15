using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(SkinnedMeshRenderer))]
public class Hurtbox : MonoBehaviour
{
    public Color outlineColor = new Color(1f, 0f, 0f, 0.3f); //red
    private SkinnedMeshRenderer smr;
    private MeshCollider meshCol;
    private Mesh bakedMesh;

    //Init the mesh and hurtbox states
    void Awake()
    {
        smr = GetComponent<SkinnedMeshRenderer>();
        meshCol = GetComponent<MeshCollider>();
        bakedMesh = new Mesh();
        meshCol.convex = true;
        meshCol.isTrigger = true;
    }

    [SerializeField] private int updateInterval = 30; // 30fps update -- NOTE: might use low-poly later for faster times
    private int frameCount;

    //Call once all update functions are called to prep for next update
    void LateUpdate()
    {
        frameCount++;
        if (frameCount % updateInterval != 0) 
            return;

        if (bakedMesh == null) 
            bakedMesh = new Mesh();
        
        smr.BakeMesh(bakedMesh);
        meshCol.sharedMesh = null;
        meshCol.sharedMesh = bakedMesh;
    }

    //Hurtbox functionalities:
    private void OnTriggerEnter(Collider obj)
    {
        if (obj.CompareTag("Hitbox"))
        {
            Debug.Log($"Hit by {obj.name}");
            // Trigger reaction stuff here (for later)
        }
    }

    //color outlining
    private void OnDrawGizmos()
    {
        if (bakedMesh != null)
        {
            Gizmos.color = outlineColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireMesh(bakedMesh);
        }
    }
}