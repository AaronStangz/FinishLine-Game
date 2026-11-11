using UnityEngine;

public class PlayerToDrone : MonoBehaviour
{
    [Header("Objects")]
    public GameObject Player;
    public GameObject Drone;
    public Transform DroneSpawn;

    [Header("Bools")]
    public bool IsPlayer = true;
    public bool IsDrone = false;

    [Header("Settings")]
    public KeyCode switchKey = KeyCode.G;

    void Start()
    {
        // Make sure the player starts active
        IsPlayer = true;
        IsDrone = false;

        UpdateObjects();
    }

    void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            if (IsPlayer)
            {
                IsPlayer = false;
                IsDrone = true;
            }
            else
            {
                IsPlayer = true;
                IsDrone = false;
            }

            UpdateObjects();
        }
    }

    void UpdateObjects()
    {
        if (Player != null)
            Player.SetActive(IsPlayer);

        if (Drone != null)
        {
            if (IsDrone && DroneSpawn != null)
            {
                Drone.transform.position = DroneSpawn.position;
                Drone.transform.rotation = DroneSpawn.rotation;
            }

            Drone.SetActive(IsDrone);
        }
    }
}