using System;
using UnityEngine;

namespace Puzzles.Controllers
{
    public class SSM_Controller : MonoBehaviour, IPuzzle
    {
        public PuzzleData data = new PuzzleData 
        { 
            guessSequence = new int[4], 
            correctSequence = new int[4] { 0, 0, 0, 0 }
        };
    
        public PuzzleData getData => data;
        
        // private void Start()
        // {
        //     API_Provider.Instance.ActiveAPIController.SendSetSeq("SSM", data.correctSequence);
        // }
        //
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
                    data.OnSuccess?.Invoke();
                }
            }
            else
            {
                Debug.Log("Wrong button! Resetting sequence.");
                data.OnFailure?.Invoke();
                data.currentSequenceIndex = 0;
            }
        }
        
        public int[] GenerateSequence(int length, int maxValue)
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