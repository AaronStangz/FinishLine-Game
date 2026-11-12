
using UnityEngine;

public class PressurePad : MonoBehaviour
{
    public MovingPlatform platform;

    private int objectsOnPad = 0;

    private void OnTriggerEnter(Collider other)
    {
        objectsOnPad++;

        if (objectsOnPad == 1 && platform != null)
        {
            platform.ActivatePlatform();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        objectsOnPad = Mathf.Max(0, objectsOnPad - 1);

        if (objectsOnPad == 0 && platform != null)
        {
            platform.DeactivatePlatform();
        }
    }
}
