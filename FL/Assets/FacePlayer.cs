
using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    [Header("Settings")]
    public float rotationSpeed = 5f;
    public bool followPlayerRotation = true;

    private Transform player;

    void Start()
    {
        FindPlayer();
    }

    void FindPlayer()
    {
        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

        if (foundPlayer != null)
            player = foundPlayer.transform;
    }

    void LateUpdate()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        // Keep the Canvas in its original position.
        // Only rotate it to follow the player.
        if (followPlayerRotation)
        {
            Quaternion targetRotation = Quaternion.Euler(
                0f,
                player.eulerAngles.y,
                0f
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}