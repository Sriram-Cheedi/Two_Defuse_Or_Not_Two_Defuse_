using System.Collections;
using Puzzles;
using Puzzles.Minor_Interaction;
using UnityEngine;

public class Laser_Arrow : MonoBehaviour
{
        public Material materialOn;
        public Material materialOff;
        private MeshRenderer meshRenderer;
        [SerializeField] private LevelData levelData;
        private Coroutine flashArrowCoroutine;
    
        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material = materialOff;
            }
        }

        private void OnEnable()
        {
            API_Controller.OnInputChanged += HandleButtonPress;
        }

        private void OnDisable()
        {
            API_Controller.OnInputChanged -= HandleButtonPress;
        }

        private void HandleButtonPress(string header, int value)
        {
            if (header == "BTN2")
            {
                if (value == 1)
                {
                    if (flashArrowCoroutine == null)
                    {
                        flashArrowCoroutine = StartCoroutine(FlashArrowForSeconds());
                    }
                }
            }
        }

        IEnumerator FlashArrowForSeconds()
        {
            if (meshRenderer != null)
            {
                meshRenderer.material = materialOn;
                yield return new WaitForSeconds(levelData.doorDelay);
                meshRenderer.material = materialOff;
            }
            flashArrowCoroutine = null;
        }
    
}
