using UnityEngine;

public class AudioSourceFinder : MonoBehaviour
{
    void Update()
    {
        if (Time.frameCount % 60 != 0) return;

        foreach (var src in FindObjectsOfType<AudioSource>())
        {
            if (src.isPlaying)
            {
                Debug.Log($"[{src.gameObject.name}] clip={src.clip?.name} " +
                          $"blend={src.spatialBlend} maxDist={src.maxDistance} " +
                          $"vol={src.volume}", src.gameObject);
            }
        }
    }
}