using UnityEngine;

public class ReceptacleTrigger : MonoBehaviour
{
    public ScavengerHuntManager manager;

    private void OnTriggerEnter(Collider other)
    {
        
        if (!other.CompareTag("Pickup")) return;

        manager.OnItemDeposited();

       
        Destroy(other.gameObject);
    }
}