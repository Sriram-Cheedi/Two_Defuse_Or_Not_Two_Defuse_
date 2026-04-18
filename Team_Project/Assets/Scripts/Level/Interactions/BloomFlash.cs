using System;
using System.Collections;
using UnityEngine;

public class SymbolAutoFlash : MonoBehaviour
{
    [SerializeField] private MeshRenderer targetRenderer;
    [SerializeField] private Material offMaterial;
    [SerializeField] private Material flashMaterial;
    [SerializeField] private LevelData levelData;
    
    private Coroutine bloomFlashCoroutine;

    private void OnEnable()
    {
        SWT_Trigger.StopMazeSymbolFlashing += StopMazeSymbolFlashing;
    }

    private void OnDisable()
    {
        SWT_Trigger.StopMazeSymbolFlashing -= StopMazeSymbolFlashing;
    }

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<MeshRenderer>();

        if (bloomFlashCoroutine == null)
        {
            bloomFlashCoroutine = StartCoroutine(FlashLoop());
        }
    }

    private IEnumerator FlashLoop()
    {
        while (true)
        {
            // ON
            targetRenderer.material = flashMaterial;
            yield return new WaitForSeconds(levelData.flashDuration); // flash duration

            // OFF
            targetRenderer.material = offMaterial;
            yield return new WaitForSeconds(levelData.offDuration);
        }
    }

    private void StopMazeSymbolFlashing()
    {
        StopCoroutine(bloomFlashCoroutine);
    }
}