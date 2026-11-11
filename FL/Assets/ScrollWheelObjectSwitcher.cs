
using UnityEngine;

public class ScrollWheelObjectSwitcher : MonoBehaviour
{
    [Header("Objects")]
    public GameObject[] objects;

    [Header("Settings")]
    public int currentIndex = 0;
    public bool loop = true;

    void Start()
    {
        UpdateObjects();
    }

    void Update()
    {
        if (objects == null || objects.Length == 0)
            return;

        float scroll = Input.mouseScrollDelta.y;

        if (scroll > 0)
        {
            currentIndex--;

            if (currentIndex < 0)
                currentIndex = loop ? objects.Length - 1 : 0;

            UpdateObjects();
        }
        else if (scroll < 0)
        {
            currentIndex++;

            if (currentIndex >= objects.Length)
                currentIndex = loop ? 0 : objects.Length - 1;

            UpdateObjects();
        }
    }

    void UpdateObjects()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
                objects[i].SetActive(i == currentIndex);
        }
    }
}
