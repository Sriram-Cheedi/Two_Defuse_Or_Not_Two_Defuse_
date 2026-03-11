using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PersistantSystems
{
    public class UIManager : MonoBehaviour
    {
        public Volume globalVolume;
        private Vignette deathVignette;
        public CanvasGroup deathFade;
        public MovementData playerData;
        private Coroutine vignetteRoutine;
        private Coroutine fadeRoutine;


        void Start()
        {
            if (globalVolume != null && globalVolume.profile.TryGet(out deathVignette))
            {
                deathVignette.intensity.value = 0f;
            }
            else
            {
                Debug.LogError("Vignette not found in Volume Profile.");
            }
            if (deathFade != null)
                deathFade.alpha = 0f;
        }

        private void OnEnable()
        {
            SpawnManager.OnDeath += TriggerDeathUI;
            SpawnManager.RespawnRequested += ResetUI;
        }

        private void OnDisable()
        {
            SpawnManager.OnDeath -= TriggerDeathUI;
            SpawnManager.RespawnRequested -= ResetUI;
        }

        private void TriggerDeathUI()
        {
            if (vignetteRoutine != null || fadeRoutine != null) return;
            vignetteRoutine = StartCoroutine(FadeVignette());
            fadeRoutine = StartCoroutine(FadeToBlack());
        }

        IEnumerator FadeVignette()
        {
            float target = playerData.vignetteIntensity;
            while (deathVignette != null && deathVignette.intensity.value < target)
            {
                deathVignette.intensity.value = Mathf.MoveTowards(deathVignette.intensity.value, target,
                    Time.deltaTime * playerData.vignetteFadeSpeed);
                yield return null;
            }

            vignetteRoutine = null;
        }
        
        IEnumerator FadeToBlack()
        {
            yield return new WaitForSeconds(0.15f);
            float target = 1f;
            while (deathFade != null && deathFade.alpha < target)
            {
                deathFade.alpha = Mathf.MoveTowards(deathFade.alpha, target,
                    Time.deltaTime * playerData.blackFadeSpeed);
                yield return null;
            }
            
            fadeRoutine = null;
        }

        private void ResetUI(RespawnData data)
        {
            StopDeathCoroutines();
            if (deathVignette != null) deathVignette.intensity.value = 0f;
            if (deathFade != null) deathFade.alpha = 0f;
        }
        
        private void StopDeathCoroutines()
        {
            if (vignetteRoutine != null)
            {
                StopCoroutine(vignetteRoutine);
                vignetteRoutine = null;
            }

            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }
        }
    }
}