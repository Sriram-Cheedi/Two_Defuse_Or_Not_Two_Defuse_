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

    public enum PuzzleGame
    {
        SSM,
        NGM,
        SWT,
        WCM,
    }
    
    public abstract class Puzzle : MonoBehaviour
    {
        PuzzleData getData { get; }
        
        public static event Action<PuzzleGame> OnSuccess;
        public static event Action<PuzzleGame> OnFailure;

        protected static void TriggerSuccess(PuzzleGame puzzleObject)
        {
            OnSuccess?.Invoke(puzzleObject);
        }
        
        protected static void TriggerFailure(PuzzleGame puzzleObject)
        {
            OnFailure?.Invoke(puzzleObject);
        }
        
    }
}