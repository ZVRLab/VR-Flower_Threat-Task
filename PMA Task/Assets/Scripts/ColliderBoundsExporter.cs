using UnityEngine;

public class ColliderBoundsExporter : MonoBehaviour
{
    [Header("Assign your areas")]
    public BoxCollider shelterCollider;  // This is the house
    public BoxCollider miningCollider;

    void Start()
    {
        PrintBounds("Shelter/House", shelterCollider);
        PrintBounds("Mining", miningCollider);
    }

    void PrintBounds(string name, BoxCollider bc)
    {
        if (bc == null)
        {
            Debug.LogWarning($"{name} BoxCollider not assigned!");
            return;
        }

        // World center
        Vector3 worldCenter = bc.transform.position + Vector3.Scale(bc.center, bc.transform.localScale);

        // World size (scale applied)
        Vector3 worldSize = Vector3.Scale(bc.size, bc.transform.localScale);

        // X/Z bounds
        float xmin = worldCenter.x - worldSize.x / 2f;
        float xmax = worldCenter.x + worldSize.x / 2f;
        float zmin = worldCenter.z - worldSize.z / 2f;
        float zmax = worldCenter.z + worldSize.z / 2f;

        Debug.Log($"{name} Bounds:");
        Debug.Log($"Xmin: {xmin}, Xmax: {xmax}, Zmin: {zmin}, Zmax: {zmax}");
        Debug.Log($"World Center: {worldCenter}, World Size: {worldSize}");
    }
}
