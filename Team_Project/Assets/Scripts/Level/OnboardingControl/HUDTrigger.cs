using System;
using System.Collections;
using UnityEngine;

public class HUDTrigger : MonoBehaviour
{
    [SerializeField] private GameObject[] myHUDs;
    [SerializeField] private MeshRenderer[] targetRenderers;
    [SerializeField] private Material offMaterial;
    [SerializeField] private Material flashMaterial;
    [SerializeField] private LevelData levelData;
    [SerializeField] private GameObject[] movementHuds; 
    GameManager.GameState currentState;

    private void OnEnable()
    {
        GameManager.onGameStateChanged += UpdateGameState;
        ActivationTip.IsBombActive += initializeHUDs;
    }
    
    private void OnDisable()
    {
        GameManager.onGameStateChanged -= UpdateGameState;
        ActivationTip.IsBombActive -= initializeHUDs;
    }

    private IEnumerator FlashLoop()
    {
        while (true)
        {
            // ON
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                targetRenderers[i].material = flashMaterial;
            }
            yield return new WaitForSeconds(levelData.flashDuration); // flash duration

            // OFF
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                targetRenderers[i].material = offMaterial;
            }
            yield return new WaitForSeconds(levelData.offDuration);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (currentState == GameManager.GameState.Playing)
        {
            triggerHUDs();
        }
        if (targetRenderers.Length != 0 && offMaterial != null && flashMaterial  != null)
        {
            StartCoroutine(FlashLoop());
        }
    }

    private void triggerHUDs()
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.ActivateTrigger(myHUDs);
    }

    private void UpdateGameState(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Playing)
        {
            currentState = state;
        }
    }

    private void initializeHUDs()
    {
        if (movementHuds.Length == 0) return;
        HUDManager.Instance.ActivateTrigger(movementHuds);
        ActivationTip.IsBombActive -= initializeHUDs;
    }
}