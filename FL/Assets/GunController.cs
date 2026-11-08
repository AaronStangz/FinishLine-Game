
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

    [Header("Bullet Casings")]
    public GameObject casingPrefab;
    public Transform casingEjectPoint;

    public float casingEjectForce = 3f;
    public float casingUpForce = 1.5f;
    public float casingTorque = 5f;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float spread = 2f;

    private float nextFireTime;
    private bool isReloading = false;

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
        currentAmmo--;

        // Ray from exact center of screen
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        // Apply spread
        float spreadX = Random.Range(-spread, spread);
        float spreadY = Random.Range(-spread, spread);

        ray.direction = Quaternion.Euler(
            spreadY, spreadX, 0f
        ) * ray.direction;

        // Find the point the bullet should travel toward
        Vector3 targetPoint;

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRange))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * maxRange;
        }

        // Calculate direction from gun to screen target
        Vector3 shootDirection =
            (targetPoint - bulletSpawnPoint.position).normalized;

        // Spawn bullet facing target
        GameObject bullet = Instantiate(
            bulletPrefab,
            bulletSpawnPoint.position,
            Quaternion.LookRotation(shootDirection)
        );

        // Set bullet range
        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.maxRange = maxRange;
        }

        Debug.Log("Ammo: " + currentAmmo + "/" + magazineSize);

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
        }
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
