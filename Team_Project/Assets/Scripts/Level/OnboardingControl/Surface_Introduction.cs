using System;
using System.Collections;
using UnityEngine;

public class Surface_Introduction : MonoBehaviour
{
    [SerializeField] public GameObject surface_card;
    [SerializeField] private LevelData levelData;
    [SerializeField] private GameObject[] hudsToDisable;
    private Coroutine surfaceIntroAlertCoroutine;
    private bool hasShown;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || hasShown)
            return;

        HideHUDs();
        
        if (surfaceIntroAlertCoroutine ==  null)
        {
            surfaceIntroAlertCoroutine = StartCoroutine(ShowSuccessAlertForSeconds());
        }

    }
    
    private IEnumerator ShowSuccessAlertForSeconds()
    {
        hasShown = true;
        surface_card.SetActive(true);

        float duration = levelData != null ? levelData.surfaceIntroAlertDuration : 0f;
        yield return new WaitForSeconds(duration);

        surface_card.SetActive(false);
        surfaceIntroAlertCoroutine = null;
    }

    private void HideHUDs()
    {
        if (hudsToDisable.Length > 0)
        {
            foreach (GameObject hud in hudsToDisable)
            {
                hud.SetActive(false);
            }
        }
    }
}
