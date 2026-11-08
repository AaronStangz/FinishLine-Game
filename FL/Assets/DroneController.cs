using UnityEngine;

public class DroneController : MonoBehaviour
{
    public Transform propeller1;
    public Transform propeller2;
    public Transform propeller3;
    public Transform propeller4;

    public float rotationSpeed = 1000f;

    void Update()
    {
        // Spin propellers 1 and 3 clockwise
        propeller1.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        propeller3.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        // Spin propellers 2 and 4 counterclockwise
        propeller2.Rotate(Vector3.up * -rotationSpeed * Time.deltaTime);
        propeller4.Rotate(Vector3.up * -rotationSpeed * Time.deltaTime);
    }
}
