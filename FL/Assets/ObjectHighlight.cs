
using UnityEngine;
using System.Collections;

public class ObjectHighlight : MonoBehaviour
{
    [Header("Highlight Object")]
    public GameObject highlightObject;

    [Header("Settings")]
    public float highlightDuration = 3f;

    private Coroutine deactivateRoutine;

    void Start()
    {
        if (highlightObject != null)
            highlightObject.SetActive(false);
    }

    public void ActivateHighlight()
    {
        if (highlightObject == null)
            return;

        highlightObject.SetActive(true);

        if (deactivateRoutine != null)
            StopCoroutine(deactivateRoutine);

        deactivateRoutine = StartCoroutine(DeactivateAfterTime());
    }

    IEnumerator DeactivateAfterTime()
    {
        yield return new WaitForSeconds(highlightDuration);

        highlightObject.SetActive(false);
        deactivateRoutine = null;
    }
}
