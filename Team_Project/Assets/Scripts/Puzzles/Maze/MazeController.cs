using System;
using System.Linq;
using UnityEngine;

namespace Puzzles.Controllers
{
    public class MazeController : MonoBehaviour, IPuzzle
    {
        
        public PuzzleData getData => data;
        private MazeCell[] cells;
        private Vector2 startPos = new Vector2(2 , -1);
        private int gridHorizontalSize = 6;
        
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
            },
            
        };

        private void InitializeMaze()
        {
            cells = FindObjectsByType<MazeCell>(FindObjectsSortMode.None)
                .OrderBy(c => c.y)
                .ThenBy(c => c.x)
                .ToArray();
            Vector2 pos = startPos;
            for (int i = 0; i < data.correctSequence.Length; i++)
            {
                var dir = data.correctSequence[i];
                switch (dir)
                {
                    case (int)Dir.UP:
                        pos.y += 1;
                        break;
                    case (int)Dir.DOWN:
                        pos.y -= 1;
                        break;
                    case (int)Dir.LEFT:
                        pos.x -= 1;
                        break;
                    case (int)Dir.RIGHT:
                        pos.x += 1;
                        break;
                    default:
                        Debug.Log($"Unknown dir {dir}");
                        break;
                }
                cells[(int)pos.y *  gridHorizontalSize + (int)pos.x].SetSafeZone(true);
            }
        }

        private void Awake()
        {
            InitializeMaze();
        }
    }
   
}