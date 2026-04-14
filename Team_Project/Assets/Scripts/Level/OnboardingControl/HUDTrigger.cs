using System;
using System.Collections;
using PersistantSystems.Software.GameStateMachine;
using PersistantSystems.Software.GameStateMachine.GameStates;
using UnityEngine;

public class HUDTrigger : MonoBehaviour
{
    [SerializeField] private GameObject[] myHUDs;
    [SerializeField] private MeshRenderer[] targetRenderers;
    [SerializeField] private Material offMaterial;
    [SerializeField] private Material flashMaterial;
    [SerializeField] private LevelData levelData;
    [SerializeField] private GameObject[] movementHuds; 
    private Coroutine flashCoroutine;
    private bool isGamePlaying = false;
    // GameManager.GameState currentState;

    private void OnEnable()
    {
        ReadyUpState.BombActivated += initializeHUDs;
        ReadyUpState.TrapDoorOpened += UpdateGameState;
    }
    
    private void OnDisable()
    {
        ReadyUpState.BombActivated -= initializeHUDs;
        ReadyUpState.TrapDoorOpened -= UpdateGameState;
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

        if (isGamePlaying)
        {
            triggerHUDs();
        }
        if (targetRenderers.Length != 0 && offMaterial != null && flashMaterial  != null)
        {
            if (flashCoroutine == null)
            {
                flashCoroutine = StartCoroutine(FlashLoop());
            }
        }
    }

    private void triggerHUDs()
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.ActivateTrigger(myHUDs);
    }

    // private void UpdateGameState(GameManager.GameState state)
    // {
    //     if (state == GameManager.GameState.Playing)
    //     {
    //         currentState = state;
    //     }
    // }

    private void initializeHUDs()
    {
        if (movementHuds.Length == 0) return;
        HUDManager.Instance.ActivateTrigger(movementHuds);
    }
    
    private void UpdateGameState()
    {
        isGamePlaying = true;
    }
}