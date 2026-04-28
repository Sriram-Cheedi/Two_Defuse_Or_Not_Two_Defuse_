using UnityEngine;
using System.Collections;

public class WebCam : MonoBehaviour
{
    public static WebCam Instance { get; private set; }
    [SerializeField] private string preferredCameraName = "USB Camera";
    public WebCamTexture WebcamTexture { get; private set; }
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
        
        StartWebcam();
    }

    public void StartWebcam()
    {
        if (isStarting) return;
        StartCoroutine(StartCam());
    }

    private IEnumerator StartCam()
    {
        isStarting = true;

        yield return new WaitForSeconds(0.5f);
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            isStarting = false;
            yield break;
        }
        string selectedDevice = devices[0].name;
        foreach (var device in devices)
        {
            if (!string.IsNullOrWhiteSpace(preferredCameraName) && name.ToLower().Contains(preferredCameraName.ToLower()))
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
            WebcamTexture.Play();
        }

        yield return new WaitForSeconds(1.0f);
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