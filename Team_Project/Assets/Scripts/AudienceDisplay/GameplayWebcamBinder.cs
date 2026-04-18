using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameplayWebcamBinder : MonoBehaviour
{
    [SerializeField] private RawImage webcamImage;
    [SerializeField] private GameObject webcamPanel;
    [SerializeField] private bool mirrorHorizontally = true;

    private IEnumerator Start()
    {
        if (webcamPanel != null)
            webcamPanel.SetActive(false);

        if (webcamImage == null)
        {
            Debug.LogError("GameplayWebcamBinder: webcamImage not assigned.");
            yield break;
        }

        while (PersistentWebcamService.Instance == null)
            yield return null;

        PersistentWebcamService.Instance.StartWebcamIfNeeded();

        float timeout = 5f;
        float timer = 0f;

        while (!PersistentWebcamService.Instance.IsReady && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (PersistentWebcamService.Instance.IsReady)
        {
            webcamImage.texture = PersistentWebcamService.Instance.WebcamTexture;
            webcamImage.rectTransform.localScale = mirrorHorizontally
                ? new Vector3(-1f, 1f, 1f)
                : Vector3.one;

            if (webcamPanel != null)
                webcamPanel.SetActive(true);

            Debug.Log("GameplayWebcamBinder: webcam texture assigned.");
        }
        else
        {
            Debug.LogWarning("GameplayWebcamBinder: webcam was not ready in time.");
        }
    }

    private void OnDisable()
    {
        if (webcamImage != null)
            webcamImage.texture = null;

        if (webcamPanel != null)
            webcamPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (webcamImage != null)
            webcamImage.texture = null;

        if (webcamPanel != null)
            webcamPanel.SetActive(false);
    }
}