using System.Collections;
using UnityEngine;

public class IterativeLazerController : MonoBehaviour
{
    public GameObject[] LaneLazers;
    [SerializeField] private float initOffset;
    [SerializeField] private float step;

    IEnumerator Start()
    {
        // Guard against empty arrays to prevent errors
        if (LaneLazers == null || LaneLazers.Length == 0) yield break;

        // Turn all lasers off initially
        for (int i = 0; i < LaneLazers.Length; i++)
        {
            LaneLazers[i].SetActive(false);
        }
        
        // Turn the first laser on
        LaneLazers[0].SetActive(true);
        
        yield return new WaitForSeconds(initOffset);

        for (int i = 0; i < LaneLazers.Length / step; i++)
        {
            StartCoroutine(IterateLazers());
            yield return new WaitForSeconds(step);
        }
    }

    IEnumerator IterateLazers()
    {
        int i = 0;
        
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            
            // Turn OFF the current laser
            LaneLazers[i].SetActive(false);
            
            // Move to the next index, wrapping back to 0 if at the end of the array
            i = (i + 1) % LaneLazers.Length;
            
            // Turn ON the new laser
            LaneLazers[i].SetActive(true);
        }
    }

    void OnDestroy()
    {
        StopCoroutine("IterateLazers");
    }
}