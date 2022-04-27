using System.Collections;
using UnityEngine;

// Handy for disabling VFX when they're done playing
public class TimedDisable : MonoBehaviour
{

    public float lifetime;
    void OnEnable()
    {
        StartCoroutine(nameof(DisableObject));
    }

    private IEnumerator DisableObject()
    {
        yield return new WaitForSeconds(lifetime);
        gameObject.SetActive(false);
    }

}
