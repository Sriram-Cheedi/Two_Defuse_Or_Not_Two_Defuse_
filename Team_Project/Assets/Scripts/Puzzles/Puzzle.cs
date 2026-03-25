using System;
using UnityEngine;

namespace Puzzles
{
    public class PuzzleData
    {
        public bool completed = false;
    
        public int[] guessSequence;
        public int[] correctSequence;
    
        public int currentSequenceIndex = 0;
    }
    
    public abstract class Puzzle : MonoBehaviour
    {
        PuzzleData getData { get; }
        
        public static event Action<GameObject> OnSuccess;
        public static event Action<GameObject> OnFailure;

        protected static void TriggerSuccess(GameObject puzzleObject)
        {
            OnSuccess?.Invoke(puzzleObject);
        }
        
        protected static void TriggerFailure(GameObject puzzleObject)
        {
            OnFailure?.Invoke(puzzleObject);
        }
        
    }
}