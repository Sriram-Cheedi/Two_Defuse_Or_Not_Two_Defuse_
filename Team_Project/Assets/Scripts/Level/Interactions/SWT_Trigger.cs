using System;
using System.Collections;
using UnityEngine;

public class SWT_Trigger : MonoBehaviour
{
    [SerializeField] private GameObject[] SWT_Symbols;
    private Renderer[] targetRenderers;
    [SerializeField] private Material offMaterial;
    [SerializeField] private Material flashMaterial;
    [SerializeField] private LevelData levelData;
    private Coroutine bloomFlashCoroutine;
    public static event Action StopMazeSymbolFlashing;

    private void Awake()
    {
        targetRenderers = new Renderer[SWT_Symbols.Length];
        for (int i = 0; i < SWT_Symbols.Length; i++)
        {
            targetRenderers[i] = SWT_Symbols[i].GetComponent<Renderer>();
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (bloomFlashCoroutine == null)
            {
                StopMazeSymbolFlashing?.Invoke();
                bloomFlashCoroutine = StartCoroutine(FlashLoop());
            }
        }
    }
    
    private IEnumerator FlashLoop()
    {
        while (true)
        {
            foreach (var targetRenderer in targetRenderers)
            {
                // ON
                targetRenderer.material = flashMaterial;
                yield return new WaitForSeconds(levelData.flashDuration); // flash duration

                // OFF
                targetRenderer.material = offMaterial;
                yield return new WaitForSeconds(levelData.offDuration);
            }
        }
    }
}
