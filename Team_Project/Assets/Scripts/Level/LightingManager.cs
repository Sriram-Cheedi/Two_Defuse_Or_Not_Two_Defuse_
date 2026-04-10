using System;
using System.Collections;
using PersistantSystems.Software.GameStateMachine.GameStates;
using UnityEngine;

public class LightingManager : MonoBehaviour
{
    public static LightingManager Instance { get; private set; }
    
    private Light[] allLights;
    
    [SerializeField] public PlayerInputRelay relay;
    
    // Variables to store your original ambient settings
    private UnityEngine.Rendering.AmbientMode originalAmbientMode;
    private Color originalAmbientColor;
    private Material originalSkybox;
    private bool isWarningStarted;
    [SerializeField] private LevelData levelData;
    private Coroutine warningCoroutine;
    
    enum MockButtonValues
    {
        PWR_OFF = 7,
        PWR_ON = 8,
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnEnable()
    {
        // For Testing with Keyboard Button Presses
        if (relay != null)
        {
            relay.NumberPressed += HandleMockButtonPress;
        }
        
        API_Controller.OnInputChanged += HandleButtonPress;
        PlayingState.ExplosionWarning +=  HandleWarningLighting;
    }
    
    
    void OnDisable()
    {
        API_Controller.OnInputChanged -= HandleButtonPress;
        PlayingState.ExplosionWarning -=  HandleWarningLighting;
        
        if (relay != null)
        {
            relay.NumberPressed -= HandleMockButtonPress;
        }
    }
    
    
    // Handle with hardware or corresponding software button presses and activate relevent ingame effect.
    private void HandleMockButtonPress(int digit)
    {
        switch (digit)
        {
            case (int)MockButtonValues.PWR_OFF:
                TurnOffLights();
                break;
            case (int)MockButtonValues.PWR_ON:
                TurnOnLights();
                break;
        }
    }

    private void HandleButtonPress(string header, int value)
    {
        switch (header)
        {
            case "PWR":
                if (API_Provider.Instance.ActiveAPIController.getComponentStateManager["PWR"] == 0)
                {
                    TurnOffLights();
                }

                else
                {
                    TurnOnLights();
                }
                break;
           
        }
    }

    private void HandleWarningLighting()
    {
        if (warningCoroutine == null)
        {
            warningCoroutine = StartCoroutine(StartWarningLighting());
        }
    }

    IEnumerator StartWarningLighting()
    {
        while (true)
        {
            TurnOffLights();
            yield return new WaitForSeconds(levelData.warningFlashingDuration);
            TurnOnLights();
            yield return new WaitForSeconds(levelData.warningFlashingDuration);
        }
    }

    private void Start()
    {
        allLights = FindObjectsOfType<Light>(); 
        
        originalAmbientMode = RenderSettings.ambientMode;
        originalAmbientColor = RenderSettings.ambientLight;
        originalSkybox = RenderSettings.skybox;
        TurnOffLights();
    }

    public void TurnOffLights()
    {
        foreach (Light light in allLights)
        {
            light.enabled = false;
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.skybox = null; 
    }

    public void TurnOnLights()
    {
        foreach (Light light in allLights)
        {
            light.enabled = true;
        }

        RenderSettings.ambientMode = originalAmbientMode;
        RenderSettings.ambientLight = originalAmbientColor;
        RenderSettings.skybox = originalSkybox;
    }
}