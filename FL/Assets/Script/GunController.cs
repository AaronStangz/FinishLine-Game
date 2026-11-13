
using UnityEngine;
using System.Collections;

public class GunController : MonoBehaviour
{
    [Header("Ammo")]
    public int magazineSize = 5;
    public int currentAmmo;
    public float reloadTime = 2f;

    [Header("Shooting")]
    public float fireRate = 5f;
    public float maxRange = 100f;
    public Camera playerCamera;
    public LayerMask hitLayers = ~0;

    [Header("Accuracy")]
    public float spread = 0.3f;
    public float aimSpread = 0.05f;
    public bool useSpread = true;

    [Header("Aiming")]
    public Transform gunTransform;
    public Vector3 aimPosition = new Vector3(0f, -0.1f, 0.3f);
    public Vector3 aimRotation = Vector3.zero;
    public float aimSpeed = 12f;

    public float normalFOV = 60f;
    public float aimFOV = 45f;
    public float fovSpeed = 10f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private float nextFireTime;
    private bool isReloading;
    private bool isAiming;

    [Header("Bullet Casings")]
    public GameObject casingPrefab;
    public Transform casingEjectPoint;
    public float casingEjectForce = 3f;
    public float casingUpForce = 1.5f;
    public float casingTorque = 5f;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;

    [Header("Debug")]
    public bool showAimLine = true;

    void Start()
    {
        currentAmmo = magazineSize;

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (gunTransform != null)
        {
            originalPosition = gunTransform.localPosition;
            originalRotation = gunTransform.localRotation;
        }

        if (playerCamera != null)
            normalFOV = playerCamera.fieldOfView;
    }

    void Update()
    {
        // AIMING
        isAiming = Input.GetMouseButton(1);

        if (gunTransform != null)
        {
            Vector3 targetPosition = isAiming
                ? aimPosition : originalPosition;

            Quaternion targetRotation = isAiming
                ? Quaternion.Euler(aimRotation) : originalRotation;

            gunTransform.localPosition = Vector3.Lerp(
                gunTransform.localPosition,
                targetPosition,
                aimSpeed * Time.deltaTime
            );

            gunTransform.localRotation = Quaternion.Slerp(
                gunTransform.localRotation,
                targetRotation,
                aimSpeed * Time.deltaTime
            );
        }

        // CAMERA FOV
        if (playerCamera != null)
        {
            float targetFOV = isAiming ? aimFOV : normalFOV;

            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                targetFOV,
                fovSpeed * Time.deltaTime
            );
        }

        // RELOADING
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isReloading && currentAmmo < magazineSize)
                StartCoroutine(Reload());
        }

        if (isReloading)
            return;

        // SHOOTING
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Shoot();
                nextFireTime = Time.time + 1f / fireRate;
            }
            else
            {
                StartCoroutine(Reload());
            }
        }
    }

    void Shoot()
    {
        if (playerCamera == null || bulletSpawnPoint == null || bulletPrefab == null)
            return;

        currentAmmo--;

        EjectCasing();

        Ray aimRay = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 targetPoint;

        if (Physics.Raycast(
            aimRay,
            out RaycastHit cameraHit,
            maxRange,
            hitLayers,
            QueryTriggerInteraction.Ignore))
        {
            targetPoint = cameraHit.point;
        }
        else
        {
            targetPoint = aimRay.origin + aimRay.direction * maxRange;
        }

        Vector3 shootDirection =
            (targetPoint - bulletSpawnPoint.position).normalized;

        // DIFFERENT SPREAD WHILE AIMING
        float currentSpread = isAiming ? aimSpread : spread;

        if (useSpread && currentSpread > 0f)
        {
            Vector2 randomSpread =
                Random.insideUnitCircle * currentSpread;

            shootDirection = Quaternion.AngleAxis(
                randomSpread.x,
                playerCamera.transform.up
            ) * shootDirection;

            shootDirection = Quaternion.AngleAxis(
                -randomSpread.y,
                playerCamera.transform.right
            ) * shootDirection;

            shootDirection.Normalize();
        }

        GameObject bullet = Instantiate(
            bulletPrefab,
            bulletSpawnPoint.position,
            Quaternion.LookRotation(shootDirection)
        );

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
            bulletScript.maxRange = maxRange;

        if (showAimLine)
        {
            Debug.DrawRay(
                bulletSpawnPoint.position,
                shootDirection * maxRange,
                Color.red,
                1f
            );
        }

        Debug.Log("Ammo: " + currentAmmo + "/" + magazineSize);

        if (currentAmmo <= 0)
            StartCoroutine(Reload());
    }

    void EjectCasing()
    {
        if (casingPrefab == null || casingEjectPoint == null)
            return;

        GameObject casing = Instantiate(
            casingPrefab,
            casingEjectPoint.position,
            casingEjectPoint.rotation
        );

        Rigidbody rb = casing.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 ejectDirection =
                casingEjectPoint.right * casingEjectForce +
                casingEjectPoint.up * casingUpForce;

            rb.AddForce(ejectDirection, ForceMode.Impulse);

            rb.AddTorque(
                Random.insideUnitSphere * casingTorque,
                ForceMode.Impulse
            );
        }
    }

    IEnumerator Reload()
    {
        if (isReloading || currentAmmo >= magazineSize)
            yield break;

        isReloading = true;

        Debug.Log("Reloading...");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;

        Debug.Log("Reloaded!");
    }
}
