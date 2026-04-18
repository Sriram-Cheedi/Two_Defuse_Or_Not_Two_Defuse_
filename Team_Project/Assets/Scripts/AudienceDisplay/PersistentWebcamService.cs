using UnityEngine;
using System.Collections;

public class PersistentWebcamService : MonoBehaviour
{
    public static PersistentWebcamService Instance { get; private set; }

    [SerializeField] private string preferredCameraName = "USB Camera";

    public WebCamTexture WebcamTexture { get; private set; }

    public bool IsReady =>
        WebcamTexture != null &&
        WebcamTexture.isPlaying &&
        WebcamTexture.width > 16;

    private bool isStarting;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartWebcamIfNeeded();
    }

    public void StartWebcamIfNeeded()
    {
        if (isStarting) return;
        if (IsReady) return;

        StartCoroutine(StartRoutine());
    }

    private IEnumerator StartRoutine()
    {
        isStarting = true;

        yield return new WaitForSeconds(0.5f);

        WebCamDevice[] devices = WebCamTexture.devices;
        Debug.Log("Persistent webcam device count: " + devices.Length);

        if (devices.Length == 0)
        {
            Debug.LogWarning("No webcam detected.");
            isStarting = false;
            yield break;
        }

        string selectedDevice = devices[0].name;

        foreach (var device in devices)
        {
            Debug.Log("Persistent webcam device: " + device.name);

            if (!string.IsNullOrWhiteSpace(preferredCameraName) &&
                device.name.ToLower().Contains(preferredCameraName.ToLower()))
            {
                selectedDevice = device.name;
            }
        }

        if (WebcamTexture == null)
        {
            WebcamTexture = new WebCamTexture(selectedDevice);
        }

        if (!WebcamTexture.isPlaying)
        {
            Debug.Log("Starting persistent webcam: " + selectedDevice);
            WebcamTexture.Play();
        }

        yield return new WaitForSeconds(1.0f);

        if (IsReady)
            Debug.Log("Persistent webcam started successfully.");
        else
            Debug.LogWarning("Persistent webcam failed to start.");

        isStarting = false;
    }

    private void OnApplicationQuit()
    {
        if (WebcamTexture != null && WebcamTexture.isPlaying)
        {
            WebcamTexture.Stop();
        }
    }
}