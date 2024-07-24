using UnityEngine;

public class DoorAnimationController : MonoBehaviour
{
    //false = closed, true = open
    private bool doorState = false;
    private Animator doorAnimator;

    private void Start()
    {
        doorAnimator = GetComponent<Animator>();
    }

    public void OpenDoor(Collider other)
    {
        if (other.tag == "Player" && doorState == false)
        {
            doorState = true;
            doorAnimator.SetTrigger("OpenDoors");
        }
    }

    public void CloseDoor(Collider other)
    {
        if (other.tag == "Player" && doorState == true)
        {
            doorState = false;
            doorAnimator.SetTrigger("CloseDoors");
        }
    }
}