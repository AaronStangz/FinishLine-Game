
using UnityEngine;

public class TargetHealth : MonoBehaviour
{
    public float health = 100f;

    public void TakeDamage(float damage)
    {
        health -= damage;

        Debug.Log("Health: " + health);

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
