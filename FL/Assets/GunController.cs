
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

        // Get the center of the camera
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0)
        );

        // Apply random bullet spread
        float spreadX = Random.Range(-spread, spread);
        float spreadY = Random.Range(-spread, spread);

        ray.direction = Quaternion.Euler(
            spreadY, spreadX, 0
        ) * ray.direction;

        // Check if bullet hits something within range
        RaycastHit hit;

        GameObject bullet = Instantiate(
    bulletPrefab,
    bulletSpawnPoint.position,
    Quaternion.LookRotation(ray.direction)
);

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.maxRange = maxRange;
        }

        if (Physics.Raycast(ray, out hit, maxRange))
        {
            Debug.Log("Hit: " + hit.collider.name);
        }

        Debug.Log("Ammo: " + currentAmmo + "/" + magazineSize);

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
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
