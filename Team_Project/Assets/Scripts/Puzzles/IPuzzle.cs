using System;

namespace Puzzles
{
    public class PuzzleData
    {
        public bool completed = false;
    
        public int[] guessSequence;
        public int[] correctSequence;
    
        public int currentSequenceIndex = 0;
    
        public Action OnSuccess;
        public Action OnFailure;
    }
    
    public interface IPuzzle
    {
        PuzzleData getData { get; }
    }
}