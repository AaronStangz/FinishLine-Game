
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthHunger : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Health Regeneration")]
    public float healthRegenRate = 5f;
    public float fullHungerRegenRate = 10f;
    public float regenHungerRequirement = 75f;

    [Header("Regeneration Delay")]
    public float regenDelayAfterDamage = 5f;
    private float regenTimer;

    [Header("Hunger")]
    public float maxHunger = 100f;
    public float currentHunger = 100f;
    public float hungerDrainRate = 0.5f;

    [Header("Starvation")]
    public float starvationDamage = 2f;
    public float starvationDamageInterval = 2f;

    [Header("UI")]
    public Slider healthSlider;
    public Slider hungerSlider;

    public TMP_Text healthText;
    public TMP_Text hungerText;

    private float starvationTimer;

    void Start()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;

        UpdateUI();
    }

    void Update()
    {
        // Slowly decrease hunger
        currentHunger -= hungerDrainRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);

        // Regeneration delay
        if (regenTimer > 0)
        {
            regenTimer -= Time.deltaTime;
        }
        else
        {
            if (currentHunger >= regenHungerRequirement &&
                currentHealth < maxHealth)
            {
                // Full hunger gives faster regeneration
                float regenRate = currentHunger >= maxHunger
                    ? fullHungerRegenRate
                    : healthRegenRate;

                currentHealth += regenRate * Time.deltaTime;
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            }
        }

        // Starvation damage
        if (currentHunger <= 0)
        {
            starvationTimer += Time.deltaTime;

            if (starvationTimer >= starvationDamageInterval)
            {
                TakeDamage(starvationDamage);
                starvationTimer = 0;
            }
        }
        else
        {
            starvationTimer = 0;
        }

        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        regenTimer = regenDelayAfterDamage;

        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateUI();
    }

    public void Eat(float amount)
    {
        currentHunger += amount;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);

        UpdateUI();
    }

    void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (hungerSlider != null)
        {
            hungerSlider.maxValue = maxHunger;
            hungerSlider.value = currentHunger;
        }

        if (healthText != null)
            healthText.text = Mathf.CeilToInt(currentHealth) + " / " + maxHealth;

        if (hungerText != null)
            hungerText.text = Mathf.CeilToInt(currentHunger) + " / " + maxHunger;
    }

    void Die()
    {
        Debug.Log("Player Died!");
    }
}
