using System.Collections.Generic;
using UnityEngine;

public class VendingMachine : MonoBehaviour
{
    MainManager IM;
    public GameObject mainManager;

    [Header("Item List")]
    public int cost;
    public List<GameObject> items = new List<GameObject>();

    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public bool spawnOnStart = true;
    public bool randomRotation = false;

    [Header("Spawned Item")]
    public GameObject spawnedItem;

    private void Start()
    {
        mainManager = GameObject.Find("Main Manager");
        IM = mainManager.GetComponent<MainManager>();


        if (spawnOnStart)
        {
            SpawnRandomItem();
        }
    }

    public void SpawnRandomItem()
    {
        if (mainManager != null && IM.Cash > cost)
        {
            IM.Cash -= cost;

            if (items == null || items.Count == 0)
            {
                Debug.LogWarning("No items assigned!");
                return;
            }

            // Pick a random item
            int randomIndex = Random.Range(0, items.Count);

            GameObject selectedItem = items[randomIndex];

            if (selectedItem == null)
                return;

            // Get spawn position
            Vector3 position = spawnPoint != null
                ? spawnPoint.position
                : transform.position;

            // Get spawn rotation
            Quaternion rotation = randomRotation
                ? Random.rotation
                : transform.rotation;

            // Spawn item
            spawnedItem = Instantiate(
                selectedItem,
                position,
                rotation
            );
        }
    }
}