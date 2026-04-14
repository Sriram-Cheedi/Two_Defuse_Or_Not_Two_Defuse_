using System;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Puzzles.Controllers
{
    public class MazeController : Puzzle
    {
        public PuzzleData data;
        public PuzzleData getData => data;
        private MazeCell[] cells;
        private Vector2 startPos = new Vector2(2 , -1);
        private int gridHorizontalSize = 6;
        private static Random rand;  
        public int[][] mazeSequences = new int[][]
        {
            new int[]{
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
            new int[]{
                (int)Dir.UP, 
                (int)Dir.UP, 
                (int)Dir.RIGHT,
                (int)Dir.UP,
                (int)Dir.RIGHT,
                (int)Dir.UP,
                (int)Dir.UP,
                (int)Dir.UP,
                (int)Dir.LEFT,
                (int)Dir.DOWN,
                (int)Dir.LEFT,
                (int)Dir.LEFT,
                (int)Dir.LEFT,
                (int)Dir.UP,
                (int)Dir.UP,
                (int)Dir.RIGHT,
                (int)Dir.RIGHT
            },
            new int[]{
                (int)Dir.UP, 
                (int)Dir.LEFT, 
                (int)Dir.UP,
                (int)Dir.UP,
                (int)Dir.LEFT,
                (int)Dir.UP,
                (int)Dir.UP,
                (int)Dir.RIGHT,
                (int)Dir.RIGHT,
                (int)Dir.DOWN,
                (int)Dir.RIGHT,
                (int)Dir.RIGHT,
                (int)Dir.RIGHT,
                (int)Dir.UP,
                (int)Dir.UP,
                (int)Dir.LEFT,
                (int)Dir.LEFT,
                (int)Dir.UP,
                (int)Dir.LEFT,
            },
            
        };
        
        public enum Dir
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

        private void InitializeMazePath()
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

        private void InitializeMazeData()
        {
            rand = new Random();
            data = new PuzzleData
            {
                correctSequence = mazeSequences[rand.Next(mazeSequences.Length)]
            };
        }

        private void Awake()
        {
            InitializeMazeData();
            InitializeMazePath();
        }
    }
   
}