using System;
using UnityEngine;

public class SmokeManager : MonoBehaviour
{
    [SerializeField]private PlayerInputRelay relay;
    private bool smokeReleased;
    private ParticleSystem smoke;
    private float currentRate;
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
    }

    private void OnDisable()
    {
        relay.SmokeReleased -= HandleSmoke;
    }

    void Start()
    {
        
    }

    private void HandleSmoke(bool smoke)
    {
        smokeReleased = smoke;
    }

    // Update is called once per frame
    void Update()
    {
        float targetRate = smokeReleased ? levelData.maxRate : 0f;
        float lerpRate = smokeReleased ? levelData.fillSpeed : levelData.clearSpeed;
        currentRate = Mathf.MoveTowards(currentRate, targetRate, lerpRate * Time.deltaTime);
        var emission = smoke.emission;
        emission.rateOverTime = currentRate;
    }
}
