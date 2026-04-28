using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GamplayWebCam : MonoBehaviour
{
    [SerializeField] private RawImage webcamImage;
    [SerializeField] private GameObject webcamPanel;
    [SerializeField] private bool mirrorCamera = true;

    private IEnumerator Start()
    {
        if (webcamImage == null)
        {
            yield break;
        }
        
        while (WebCam.Instance == null)
        {
            yield return null;
        }

        WebCam.Instance.StartWebcam();
        float timeout = 5f;
        float timer = 0f;
        while (timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        webcamImage.texture = WebCam.Instance.WebcamTexture;
        webcamImage.rectTransform.localScale = mirrorCamera ? new Vector3(-1f, 1f, 1f) : Vector3.one;

        if (webcamPanel != null)
        {
                webcamPanel.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (webcamImage != null)
        {
            webcamImage.texture = null;
        }

        if (webcamPanel != null)
        {
            webcamPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (webcamImage != null)
        {
            webcamImage.texture = null;
        }

        if (webcamPanel != null)
        {
            webcamPanel.SetActive(false);
        }
    }
}