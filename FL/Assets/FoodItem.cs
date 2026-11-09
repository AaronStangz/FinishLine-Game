
using UnityEngine;

public class FoodItem : MonoBehaviour
{
    [Header("Food Settings")]
    public float hungerAmount = 25f;
    public float interactionDistance = 3f;
    public KeyCode eatKey = KeyCode.E;

    [Header("References")]
    public Transform player;

    private PlayerHealthHunger playerHunger;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (player != null)
            playerHunger = player.GetComponent<PlayerHealthHunger>();
    }

    void Update()
    {
        if (player == null || playerHunger == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance <= interactionDistance && Input.GetKeyDown(eatKey))
        {
            EatFood();
        }
    }

    void EatFood()
    {
        playerHunger.Eat(hungerAmount);

        Destroy(gameObject);
    }
}
