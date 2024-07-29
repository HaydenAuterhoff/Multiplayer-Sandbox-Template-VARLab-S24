using UnityEngine;

public class DoorColliderNotifier : MonoBehaviour
{
    [SerializeField] private DoorAnimationController parentController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            parentController.OpenDoorRpc();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            parentController.CloseDoorRpc();
        }

    }
}