
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
    public bool useSpread = true;

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

    private float nextFireTime;
    private bool isReloading;

    void Start()
    {
        currentAmmo = magazineSize;

        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        if (isReloading)
            return;

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

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentAmmo < magazineSize)
                StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        if (playerCamera == null || bulletSpawnPoint == null)
            return;

        currentAmmo--;

        EjectCasing();

        // Get the exact center of the screen.
        Ray aimRay = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 targetPoint;

        // Find what the camera is aiming at.
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

        // Calculate direction from the actual gun barrel.
        Vector3 shootDirection =
            (targetPoint - bulletSpawnPoint.position).normalized;

        // Apply small bullet spread.
        if (useSpread && spread > 0f)
        {
            Vector2 randomSpread = Random.insideUnitCircle * spread;

            shootDirection = Quaternion.AngleAxis(
                randomSpread.x, playerCamera.transform.up
            ) * shootDirection;

            shootDirection = Quaternion.AngleAxis(
                -randomSpread.y, playerCamera.transform.right
            ) * shootDirection;

            shootDirection.Normalize();
        }

        // Spawn bullet facing the target.
        GameObject bullet = Instantiate(
            bulletPrefab,
            bulletSpawnPoint.position,
            Quaternion.LookRotation(shootDirection)
        );

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.maxRange = maxRange;
        }

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
        if (isReloading)
            yield break;

        isReloading = true;

        Debug.Log("Reloading...");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;

        Debug.Log("Reloaded!");
    }
}
