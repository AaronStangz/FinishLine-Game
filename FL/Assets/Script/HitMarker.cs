using UnityEngine;
using TMPro;

public class HitMarker : MonoBehaviour
{
    public enum DisplayMode
    {
        Damage,
        Health
    }

    [Header("Settings")]
    public TextMeshPro text;
    public float floatSpeed = 2f;
    public float lifeTime = 1f;
    public DisplayMode mode;

    [Header("Colors")]
    public Color damageColor = Color.red;
    public Color healthColor = Color.green;

    private float damage;
    private float currentHP;

    void Start()
    {
        // RANDOMLY PICK MODE HERE
        mode = (Random.value > 0.5f) ? DisplayMode.Damage : DisplayMode.Health;

        Destroy(gameObject, lifeTime);
        UpdateText();
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        if (Camera.main != null)
            transform.LookAt(Camera.main.transform);
    }

    public void SetValues(float dmg, float hp)
    {
        damage = dmg;
        currentHP = hp;
        UpdateText();
    }

    void UpdateText()
    {
        if (text == null) return;

        switch (mode)
        {
            case DisplayMode.Damage:
                text.text = $"{damage}";
                text.color = damageColor;
                break;

            case DisplayMode.Health:
                text.text = $"{currentHP}";
                text.color = healthColor;
                break;
        }
    }
}