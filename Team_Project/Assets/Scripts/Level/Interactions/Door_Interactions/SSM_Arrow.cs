using Puzzles;
using UnityEngine;

namespace Level.Interactions.Door_Interactions
{
    public class SSM_Arrow : MonoBehaviour
    { 
        public Material materialOn;
        public Material materialOff;
        private MeshRenderer meshRenderer;
    
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
            Puzzle.OnSuccess += SwitchMaterial;
        }

        private void OnDisable()
        {
            Puzzle.OnSuccess -= SwitchMaterial;
        }

        private void SwitchMaterial(PuzzleGame game)
        {
            if (game == PuzzleGame.SSM)
            {
                meshRenderer.material = materialOn;
            }
            
        }
    }
}