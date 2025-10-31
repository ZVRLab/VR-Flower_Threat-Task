using UnityEngine;

public class ZoneDetector : MonoBehaviour
{
    public enum ZoneType { Shelter, Mining }
    public ZoneType zoneType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (zoneType == ZoneType.Shelter)
                FindObjectOfType<TrialManager>().SetPlayerInShelter(true);
            else if (zoneType == ZoneType.Mining)
                FindObjectOfType<TrialManager>().SetPlayerInMining(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (zoneType == ZoneType.Shelter)
                FindObjectOfType<TrialManager>().SetPlayerInShelter(false);
            else if (zoneType == ZoneType.Mining)
                FindObjectOfType<TrialManager>().SetPlayerInMining(false);
        }
    }
}
