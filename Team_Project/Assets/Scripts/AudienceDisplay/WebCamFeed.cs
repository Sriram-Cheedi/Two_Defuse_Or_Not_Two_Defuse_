using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Display2WebcamFeed : MonoBehaviour
{
    [SerializeField] private RawImage webcamImage;
    [SerializeField] private string preferredCameraName = "USB Camera";
    [SerializeField] private bool mirrorHorizontally = true;

    private WebCamTexture webcamTexture;
    private bool isStarting;

    private void Start()
    {
        StartCoroutine(StartWebcamRoutine());
    }

    private IEnumerator StartWebcamRoutine()
    {
        if (isStarting) yield break;
        isStarting = true;

        // Give scene reload + camera driver time to settle
        yield return new WaitForSeconds(1.0f);

        if (webcamImage == null)
        {
            Debug.LogError("Webcam RawImage is not assigned.");
            isStarting = false;
            yield break;
        }

        StopWebcamImmediate();

        var devices = WebCamTexture.devices;
        Debug.Log("Webcam device count: " + devices.Length);

        if (devices.Length == 0)
        {
            Debug.LogWarning("No webcam detected.");
            isStarting = false;
            yield break;
        }

        string selectedDevice = devices[0].name;

        foreach (var device in devices)
        {
            Debug.Log("Webcam device: " + device.name);

            if (!string.IsNullOrWhiteSpace(preferredCameraName) &&
                device.name.ToLower().Contains(preferredCameraName.ToLower()))
            {
                selectedDevice = device.name;
            }
        }

        Debug.Log("Starting webcam: " + selectedDevice);

        webcamTexture = new WebCamTexture(selectedDevice);
        webcamImage.texture = webcamTexture;
        webcamImage.rectTransform.localScale = mirrorHorizontally
            ? new Vector3(-1f, 1f, 1f)
            : Vector3.one;

        webcamTexture.Play();

        yield return new WaitForSeconds(1.5f);

        if (webcamTexture != null && webcamTexture.isPlaying && webcamTexture.width > 16)
            Debug.Log("Webcam started successfully.");
        else
            Debug.LogWarning("Webcam failed to start.");

        isStarting = false;
    }

    public void StopWebcamImmediate()
    {
        if (webcamTexture != null)
        {
            if (webcamTexture.isPlaying)
            {
                webcamTexture.Stop();
                Debug.Log("Webcam stopped.");
            }

            if (webcamImage != null)
                webcamImage.texture = null;

            webcamTexture = null;
        }
    }

    private void OnDisable()
    {
        StopWebcamImmediate();
    }

    private void OnDestroy()
    {
        StopWebcamImmediate();
    }

    private void OnApplicationQuit()
    {
        StopWebcamImmediate();
    }
}