using UnityEngine;

public class DoorColliderNotifier : MonoBehaviour
{
    [SerializeField] private DoorAnimationController parentController;

    private void OnTriggerEnter(Collider other)
    {
        parentController.OpenDoor(other);
    }

    private void OnTriggerExit(Collider other)
    {
        parentController.CloseDoor(other);
    }
}