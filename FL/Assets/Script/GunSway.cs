using UnityEngine;

public class GunSway : MonoBehaviour
{
    [Header("Sway Settings")]
    public float swayAmount = 0.05f;
    public float swaySmooth = 6f;
    public float maxSway = 0.15f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector3 swayOffset = new Vector3(-mouseX, -mouseY, 0f) * swayAmount;

        swayOffset = Vector3.ClampMagnitude(swayOffset, maxSway);

        Vector3 targetPos = startPos + swayOffset;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPos,
            Time.deltaTime * swaySmooth
        );
    }
}