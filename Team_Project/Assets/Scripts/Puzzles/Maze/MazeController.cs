using UnityEngine;

namespace Puzzles.Controllers
{
    public class MazeController : MonoBehaviour, IPuzzle
    {
        
        public PuzzleData getData => data;
        
        private enum Dir
        {
            UP,
            DOWN,
            LEFT,
            RIGHT
        }

        private int[,] maze = new int[,]
        {
            { 0, 0, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 0, 1 },
            { 1, 0, 0, 1, 1, 1 },
            { 1, 1, 1, 0, 0, 0 },
            { 0, 0, 1, 0, 0, 0 }
        };
        
        public PuzzleData data = new PuzzleData 
        { 
            correctSequence = new int[] { 
                (int)Dir.UP, 
                (int)Dir.UP, 
                (int)Dir.UP,
                (int)Dir.LEFT,
                (int)Dir.LEFT,
                (int)Dir.UP,
                (int)Dir.UP,
                (int)Dir.RIGHT,
                (int)Dir.RIGHT,
                (int)Dir.RIGHT,
                (int)Dir.DOWN,
                (int)Dir.RIGHT,
                (int)Dir.RIGHT,
                (int)Dir.UP,
                (int)Dir.UP,
                (int)Dir.UP,
                (int)Dir.LEFT,
                (int)Dir.LEFT,
                (int)Dir.LEFT
            }
        };
    }
   
}