using System.Collections;
using UnityEngine;

public class HUDTrigger : MonoBehaviour
{
    [SerializeField] private GameObject[] myHUDs;
    [SerializeField] private MeshRenderer[] targetRenderers;
    [SerializeField] private Material offMaterial;
    [SerializeField] private Material flashMaterial;
    [SerializeField] private LevelData levelData;

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

        TriggerHUDs();
        if (targetRenderers.Length != 0 && offMaterial != null && flashMaterial  != null)
        {
            StartCoroutine(FlashLoop());
        }
    }

    private void TriggerHUDs()
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.ActivateTrigger(myHUDs);
    }
}