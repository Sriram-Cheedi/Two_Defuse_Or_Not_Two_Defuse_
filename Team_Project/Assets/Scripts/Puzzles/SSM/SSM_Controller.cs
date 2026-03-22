using System;
using UnityEngine;

namespace Puzzles.Controllers
{
    public class SSM_Controller : Puzzle
    {
        public PuzzleData data = new PuzzleData 
        { 
            guessSequence = new int[4], 
            correctSequence =new int[4] {0, 0, 0,0},
            // correctSequence = GenerateSequence(6, 4),
        };

        
        public PuzzleData getData => data;
        
        public void CheckSSMButtonPress(int pressedValue)
        {
            if (!data.completed && pressedValue == data.correctSequence[data.currentSequenceIndex])
            {
                Debug.Log("Correct!" + pressedValue);
                data.currentSequenceIndex++;

                if (data.currentSequenceIndex >= data.correctSequence.Length)
                {
                    Debug.Log("SSM Complate" + pressedValue);
                    data.completed = true;
                    TriggerSuccess(this.gameObject);
                }
            }
            else
            {
                Debug.Log("Wrong button! Resetting sequence.");
                TriggerFailure(this.gameObject);
                data.currentSequenceIndex = 0;
            }
        }
        
        public static int[] GenerateSequence(int length, int maxValue)
        {
            System.Random rng = new System.Random();
            int[] Seq = new int[length];
        
            for (int i = 0; i < length; i++)
            {
                Seq[i] = rng.Next(0 ,maxValue);
            }
        
            return Seq;
        }
    }
}