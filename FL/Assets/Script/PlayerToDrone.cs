
using UnityEngine;
using UnityEngine.UI;

public class PlayerToDrone : MonoBehaviour
{
    [Header("Objects")]
    public GameObject Player;
    public GameObject Drone;
    public Transform DroneSpawn;

    [Header("Bools")]
    public bool IsPlayer = true;
    public bool IsDrone = false;

    [Header("UI Switching")]
    public GameObject PlayerUI;
    public GameObject DroneUI;

    [Header("Switch Settings")]
    public KeyCode switchKey = KeyCode.G;
    public float holdTime = 2f;

    [Header("Low Drone Time Vignette")]
    public Image lowDroneTimeVignette;
    public float vignetteStartTime = 50f;

    [Range(0f, 1f)]
    public float maxVignetteAlpha = 0.9f;

    public float vignetteFadeSpeed = 3f;
    public Color vignetteColor = Color.red;

    [Header("Drone Time Limit")]
    public float maxDroneTime = 60f;
    private float droneTimer = 0f;

    [Header("Drone Time UI")]
    public Slider droneTimeSlider;


    [Header("UI")]
    public Slider holdSlider;

    private float holdTimer = 0f;
    private bool isHolding = false;

    void Start()
    {
        IsPlayer = true;
        IsDrone = false;

        if (holdSlider != null)
        {
            holdSlider.minValue = 0;
            holdSlider.maxValue = holdTime;
            holdSlider.value = 0;
            holdSlider.gameObject.SetActive(false);
        }

        if (droneTimeSlider != null)
        {
            droneTimeSlider.minValue = 0;
            droneTimeSlider.maxValue = maxDroneTime;
            droneTimeSlider.value = maxDroneTime;
            droneTimeSlider.gameObject.SetActive(false);
        }

        UpdateObjects();
    }

    void Update()
    {
        UpdateVignette();

        // Hold G to switch
        if (Input.GetKey(switchKey))
        {
            holdTimer += Time.deltaTime;
            isHolding = true;

            if (holdSlider != null)
            {
                holdSlider.gameObject.SetActive(true);
                holdSlider.value = holdTimer;
            }

            if (holdTimer >= holdTime)
            {
                if (IsPlayer)
                    EnterDrone();
                else
                    ReturnToPlayer();

                ResetHold();
            }
        }
        else if (isHolding)
        {
            ResetHold();
        }

        // Drone time limit

        if (IsDrone)
        {
            droneTimer += Time.deltaTime;

            float timeLeft = Mathf.Max(0f, maxDroneTime - droneTimer);

            if (droneTimeSlider != null)
            {
                droneTimeSlider.gameObject.SetActive(true);
                droneTimeSlider.maxValue = maxDroneTime;
                droneTimeSlider.value = timeLeft;
            }

            if (timeLeft <= 0f)
            {
                ReturnToPlayer();
            }
        }

    }

    void EnterDrone()
    {
        IsPlayer = false;
        IsDrone = true;

        droneTimer = 0f;

        UpdateObjects();
    }

    void ReturnToPlayer()
    {
        IsPlayer = true;
        IsDrone = false;

        droneTimer = 0f;

        if (droneTimeSlider != null)
        {
            droneTimeSlider.value = maxDroneTime;
            droneTimeSlider.gameObject.SetActive(false);
        }

        UpdateObjects();
    }



    void UpdateVignette()
    {
        if (lowDroneTimeVignette == null) return;

        float targetAlpha = 0f;

        // Only show vignette while controlling the drone
        if (IsDrone)
        {
            float timeLeft = Mathf.Max(0f, maxDroneTime - droneTimer);

            float timePercent = Mathf.InverseLerp(
                vignetteStartTime,
                0f,
                timeLeft
            );

            targetAlpha = timePercent * maxVignetteAlpha;
        }

        Color color = vignetteColor;

        color.a = Mathf.Lerp(
            lowDroneTimeVignette.color.a,
            targetAlpha,
            Time.deltaTime * vignetteFadeSpeed
        );

        lowDroneTimeVignette.color = color;
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

        // Switch UIs
        if (PlayerUI != null)
            PlayerUI.SetActive(IsPlayer);

        if (DroneUI != null)
            DroneUI.SetActive(IsDrone);
    }

    void ResetHold()
    {
        holdTimer = 0f;
        isHolding = false;

        if (holdSlider != null)
        {
            holdSlider.value = 0f;
            holdSlider.gameObject.SetActive(false);
        }
    }
}
