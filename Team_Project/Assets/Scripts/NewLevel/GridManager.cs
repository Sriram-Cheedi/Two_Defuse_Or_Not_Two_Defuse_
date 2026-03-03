using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

public class GridManager : MonoBehaviour
{
    [Header("Grid Size")]
    private int gridSize = 6;
    
    [Header("Grid Start and Target Point")]
    public Vector2Int startPoint;
    public Vector2Int targetPoint;
    
    [Header("Safe/Unsafe ratio (Range = 0 - 1)")]
    public float safeUnsafeProbabilty;
    private CrackFloor[][] grid = new CrackFloor[6][];
    
    public CrackFloor[] floors;
    GameObject floorPrefab;
    public GameObject spawnPoint;
    float offsetToSpawn = 20.0f;
    private Vector3 grizScale = new Vector3(15.0f, 1.0f, 15.0f);
    public Material myMat;
    private PlayerInputRelay relay;

    private void Awake()
    {
        
    }
    
    public void Start()
    {
        // spawnPoint =  GameObject.FindGameObjectWithTag("GridSpawn");
        // safeUnsafeProbabilty = 0.6f;
        GridGeneration();
        BuildPath();
        int count = 0;
        foreach (var floor in floors)
        {
            if (!floor.willBreak)
            {
                Debug.Log(floor.gridX + "," + floor.gridY);
                count++;

            }
        }
        Debug.Log("----------------------------");

        foreach (var floor in floors)
        {
            if (floor.willBreak)
            {
                Random range = new Random();
                double rand2 = range.NextDouble();
                if(rand2 < safeUnsafeProbabilty)
                {
                    floor.willBreak = false;
                    floor.hasBroken = false;
                    Debug.Log(floor.gridX + "," + floor.gridY);
                    count++;
                }
            }
        }
        Debug.Log(count);
        for (int y = 0; y < gridSize; y++)
        {
            var sb = new System.Text.StringBuilder();
            for (int x = 0; x < gridSize; x++)
            {
                if (floors[y * gridSize + x].willBreak == false)
                {
                    sb.Append("1");
                }
                else
                {
                    sb.Append("0");
                }
            }
            Debug.Log(sb.ToString());
        }
    }

    private void OnEnable()
    {
        if (relay == null)
        {
            relay = Object.FindAnyObjectByType<PlayerInputRelay>();
        }
        if (relay != null)
        {
            relay.RespawnRequested += ResetPath;
        }
    }

    private void OnDisable()
    {
        if (relay != null)
        {
            relay.RespawnRequested -= ResetPath;
        }
    }

    private void GridGeneration()
    {

        floors = FindObjectsByType<CrackFloor>(FindObjectsSortMode.None)
            .OrderBy(c => c.gridX)
            .ThenBy(c => c.gridY)
            .ToArray();
       
        Vector3 startingPosition = new Vector3(spawnPoint.transform.position.x + offsetToSpawn, spawnPoint.transform.position.y, spawnPoint.transform.position.z + offsetToSpawn);
        float startPosX = startingPosition.x;
        float startPosY = startingPosition.y;
        float startPosZ = startingPosition.z;

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                floors[y * gridSize + x].transform.localScale = grizScale;
                Vector3 offsetGrid = floors[y * gridSize + x].GetComponent<Collider>().bounds.size;
                offsetGrid.x += 4.0f;
                offsetGrid.z += 4.0f;

                floors[y * gridSize + x].transform.position = new Vector3(startPosX + offsetGrid.x * x,startPosY,startPosZ+ offsetGrid.z * y);
            }
        }

    }

    public void BuildPath()
    {
        startPoint = new Vector2Int(0, 0);
        floors[0].willBreak = false;
        while (startPoint.x < gridSize && startPoint.y < gridSize)
        {
            if (startPoint.x == gridSize - 1 && startPoint.y == gridSize - 1)
            {
                break;
            }

            Random range = new Random();
            double rand = range.NextDouble();
            int Xstep = 1;
            int Ystep = 1;
            if (rand < 0.5)
            {
                Random range2 = new Random();
                double rand2 = range2.NextDouble();

                if (rand2 < 0.5)
                {
                    Xstep -= Xstep;
                }

                if (startPoint.x + Xstep < 0 || startPoint.x + Xstep >= gridSize)
                {
                    continue;
                }

                startPoint.x += Xstep;
                floors[startPoint.y * gridSize + startPoint.x].willBreak = false;
                floors[startPoint.y * gridSize + startPoint.x].hasBroken = false;
                
            }
            else
            {
                Random range3 = new Random();
                double rand3 = range3.NextDouble();
                if (rand3 < 0.5)
                {
                    Ystep -= Ystep;
                }

                if (startPoint.y + Ystep < 0 || startPoint.y + Ystep >= gridSize)
                {
                    continue;
                }

                startPoint.y += Ystep;
                floors[startPoint.y * gridSize + startPoint.x].willBreak = false;
                floors[startPoint.y * gridSize + startPoint.x].hasBroken = false;
            }
        }

        // while (startPoint.x < gridSize)
        // {
        //     if (startPoint.x + 1 < gridSize)
        //     {
        //         startPoint.x += 1;
        //         floors[startPoint.y * gridSize + startPoint.x].willBreak = false;
        //         floors[startPoint.y * gridSize + startPoint.x].hasBroken = false;
        //     }
        //
        //     if (startPoint.x == gridSize - 1)
        //     {
        //         break;
        //     }
        // }
        //
        // while (startPoint.y < gridSize)
        // {
        //     if (startPoint.y + 1 < gridSize)
        //     {
        //         startPoint.y += 1;
        //         floors[startPoint.y * gridSize + startPoint.x].willBreak = false;
        //         floors[startPoint.y * gridSize + startPoint.x].hasBroken = false;
        //     }
        //
        //     if (startPoint.y == gridSize - 1)
        //     {
        //         break;
        //     }
        // }
    }

    public void ResetPath()
    {
        foreach (var floor in floors)
        {
            floor.floorCollider.enabled = true;
            floor.willBreak = true;
            floor.hasBroken = false;
        }
        
        BuildPath();
        int count = 0;
        foreach (var floor in floors)
        {
            if (!floor.willBreak)
            {
                Debug.Log(floor.gridX + "," + floor.gridY);
                count++;

            }
        }
        Debug.Log("----------------------------");

        foreach (var floor in floors)
        {
            if (floor.willBreak)
            {
                Random range = new Random();
                double rand2 = range.NextDouble();
                if(rand2 < safeUnsafeProbabilty)
                {
                    floor.willBreak = false;
                    floor.hasBroken = false;
                    Debug.Log(floor.gridX + "," + floor.gridY);
                    count++;
                }
            }
        }
        Debug.Log(count);
        for (int y = 0; y < gridSize; y++)
        {
            var sb = new System.Text.StringBuilder();
            for (int x = 0; x < gridSize; x++)
            {
                if (floors[y * gridSize + x].willBreak == false)
                {
                    sb.Append("1");
                }
                else
                {
                    sb.Append("0");
                }
            }
            Debug.Log(sb.ToString());
        }
    }
}
