using System;
using UnityEngine;

public class SmokeManager : MonoBehaviour
{
    [SerializeField]private PlayerInputRelay relay;
    private bool smokeReleased;
    private ParticleSystem smoke;
    private float currentRate;
    private float currentAlpha;
    public LevelData levelData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (relay == null) relay = GetComponent<PlayerInputRelay>();
        if (smoke == null) smoke = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        relay.SmokeReleased += HandleSmoke;
        API_Provider.Instance.ActiveAPIController.OnInputChanged += OnSmokeReleased;
    }

    private void OnDisable()
    {
        relay.SmokeReleased -= HandleSmoke;
        API_Provider.Instance.ActiveAPIController.OnInputChanged -= OnSmokeReleased;
    }

    void Start()
    {
        
    }

    private void HandleSmoke(bool smoke)
    {
        smokeReleased = smoke;
    }

    private void OnSmokeReleased(String header, int value)
    {
        Debug.Log("Smoke Released: " + value);
        if (header == "FGM" && value == 2)
        {
            smokeReleased = true;
        }
        else if (header == "FGM" && value == 1)
        {
            smokeReleased = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float targetRate = smokeReleased ? levelData.maxRate : 0f;
        float targetAlpha = smokeReleased ? levelData.targetAlpha : 0f;
        float lerpRate = smokeReleased ? levelData.fillSpeed : levelData.clearSpeed;
        float alphaLerpRate = smokeReleased ? levelData.alphaSpeed : levelData.clearSpeed;
        currentRate = Mathf.MoveTowards(currentRate, targetRate, lerpRate * Time.deltaTime);
        var emission = smoke.emission;
        emission.rateOverTime = currentRate;
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, alphaLerpRate * Time.deltaTime);
        Debug.Log(currentAlpha);
        var main = smoke.main;
        Color c = main.startColor.color;
        c.a = currentAlpha;
        main.startColor = c;
    }
}
