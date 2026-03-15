using System;
using UnityEngine;

public class SmokeEmitter : MonoBehaviour
{
    
    public LevelData levelData;
    private AudioSource smokeAudio;
    private ParticleSystem[] smokes;
    private float currentRate;
    private float currentAlpha;
    private bool shouldEmitSmoke;
    private bool wasEmitting;

    
    
    private void Awake()
    {
        if (smokes == null)
        {
            smokes = GetComponentsInChildren<ParticleSystem>(true);
        }
        if (smokeAudio == null)
        {
            smokeAudio = GetComponent<AudioSource>();
        }
    }
    
    public void SmokeState(bool smoke)
    {
        shouldEmitSmoke = smoke;
    }

    private void Update()
    {
        if (smokes == null)
        {
            return;
        }
        float targetRate = shouldEmitSmoke ? levelData.maxRate : 0f;
        float targetAlpha = shouldEmitSmoke ? levelData.targetAlpha : 0f;
        float lerpRate = shouldEmitSmoke? levelData.fillSpeed : levelData.clearSpeed;
        float alphaLerpRate = shouldEmitSmoke ? levelData.alphaSpeed : levelData.clearSpeed;
        currentRate = Mathf.MoveTowards(currentRate, targetRate, lerpRate * Time.deltaTime);
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, alphaLerpRate * Time.deltaTime);
        for (int i = 0; i < smokes.Length; i++)
        {
            ParticleSystem smoke  = smokes[i];
            if (smoke == null) continue;
            var emission = smoke.emission;
            emission.rateOverTime = currentRate;
            Debug.Log(currentAlpha);
            var main = smoke.main;
            Color c = main.startColor.color;
            c.a = currentAlpha;
            main.startColor = new ParticleSystem.MinMaxGradient(c);
        }
        if (smokeAudio != null)
        {
            if (shouldEmitSmoke && !wasEmitting)
            {
                smokeAudio.Play();
            }
            else if (!shouldEmitSmoke && wasEmitting)
            {
                smokeAudio.Stop();
            }
        }
        wasEmitting = shouldEmitSmoke;
    }
}
