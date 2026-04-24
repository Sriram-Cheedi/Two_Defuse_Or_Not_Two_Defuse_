using System;
using System.Collections;
using UnityEngine;

public class PowerCycle_Trigger : MonoBehaviour
{
    [SerializeField] private bool isHudsTrigger;
    [SerializeField] private GameObject wirecut_card;
    [SerializeField] private LevelData levelData;
    private Coroutine wirecutIntroAlertCoroutine;
    private bool hasTriggered;

    private void Awake()
    {
        hasTriggered = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (other.CompareTag("Player"))
        {
            API_Provider.Instance.ActiveAPIController.SendSet("PWR", 2);
            if (wirecutIntroAlertCoroutine ==  null && wirecut_card != null)
            {
                wirecutIntroAlertCoroutine = StartCoroutine(ShowSuccessAlertForSeconds());
            }
            hasTriggered = true;
        }
    }
    
    private IEnumerator ShowSuccessAlertForSeconds()
    {
        wirecut_card.SetActive(true);

        float duration = levelData.wirecutIntroAlertDuration;
        yield return new WaitForSeconds(duration);

        wirecut_card.SetActive(false);
        wirecutIntroAlertCoroutine = null;
    }
}