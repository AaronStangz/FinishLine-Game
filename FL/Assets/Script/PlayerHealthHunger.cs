
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

    [Header("Low Health Vignette")]
    public Image lowHealthVignette;
    public float vignetteStartHealth = 50f;
    [Range(0f, 1f)]
    public float maxVignetteAlpha = 0.9f;
    public float vignetteFadeSpeed = 3f;
    public Color vignetteColor = Color.red;


    [Header("Low Hunger Vignette")]
    public Image lowHungerVignette;
    public float vignetteStartHunger = 50f;
    [Range(0f, 1f)]
    public float maxHungerVignetteAlpha = 0.8f;
    public float hungerVignetteFadeSpeed = 3f;
    public Color hungerVignetteColor = new Color(1f, 0.45f, 0f);


    [Header("Death / Respawn")]
    public GameObject deathMenu;
    public Transform Spawn;
    public KeyCode respawnKey = KeyCode.Space;

    private float starvationTimer;
    private bool isDead;

    [Header("Respawn Zone")]
    public string respawnZoneTag = "RespawnZone";

    void Start()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;

        if (deathMenu != null)
            deathMenu.SetActive(false);

        if (lowHealthVignette != null)
        {
            Color c = vignetteColor;
            c.a = 0f;
            lowHealthVignette.color = c;
        }

        UpdateUI();


        if (lowHungerVignette != null)
        {
            Color c = hungerVignetteColor;
            c.a = 0f;
            lowHungerVignette.color = c;
        }

    }

    void Update()
    {
        if (isDead)
        {
            UpdateVignette();

            if (Input.GetKeyDown(respawnKey))
                Respawn();

            return;
        }

        // Hunger drain
        currentHunger -= hungerDrainRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);

        // Health regeneration
        if (regenTimer > 0)
        {
            regenTimer -= Time.deltaTime;
        }
        else
        {
            if (currentHunger >= regenHungerRequirement &&
                currentHealth < maxHealth)
            {
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

        UpdateVignette();
        UpdateHungerVignette();
        UpdateUI();
    }

    void UpdateVignette()
    {
        if (lowHealthVignette == null) return;

        // Calculate how low the health is below 50
        float healthPercent = Mathf.InverseLerp(
            vignetteStartHealth,
            0f,
            currentHealth
        );

        float targetAlpha = healthPercent * maxVignetteAlpha;

        Color color = vignetteColor;
        color.a = Mathf.Lerp(
            lowHealthVignette.color.a,
            targetAlpha,
            Time.deltaTime * vignetteFadeSpeed
        );

        lowHealthVignette.color = color;
    }


    void UpdateHungerVignette()
    {
        if (lowHungerVignette == null) return;

        float hungerPercent = Mathf.InverseLerp(
            vignetteStartHunger,
            0f,
            currentHunger
        );

        float targetAlpha = hungerPercent * maxHungerVignetteAlpha;

        Color color = hungerVignetteColor;
        color.a = Mathf.Lerp(
            lowHungerVignette.color.a,
            targetAlpha,
            Time.deltaTime * hungerVignetteFadeSpeed
        );

        lowHungerVignette.color = color;
    }


    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        regenTimer = regenDelayAfterDamage;

        UpdateUI();

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateUI();
    }

    public void Eat(float amount)
    {
        if (isDead) return;

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
        if (isDead) return;

        isDead = true;

        Debug.Log("Player Died!");

        if (deathMenu != null)
            deathMenu.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Respawn()
    {
        if (Spawn == null)
        {
            Debug.LogError("Spawn Transform is not assigned!");
            return;
        }

        transform.position = Spawn.position;
        transform.rotation = Spawn.rotation;

        currentHealth = maxHealth;
        currentHunger = maxHunger;

        regenTimer = 0;
        starvationTimer = 0;

        isDead = false;

        if (deathMenu != null)
            deathMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UpdateVignette();
        UpdateUI();

        Debug.Log("Player Respawned!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(respawnZoneTag))
        {
            Spawn = other.transform;
            Debug.Log("New Respawn Point: " + other.name);
        }
    }
}
