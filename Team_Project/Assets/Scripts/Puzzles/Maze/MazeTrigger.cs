using System;
using System.Collections;
using UnityEngine;

public class Maze_Trigger : MonoBehaviour
{
    [SerializeField] Puzzles.Controllers.MazeController maze_controller;
    
    private bool gameActive = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         gameActive = true;
    //         StartCoroutine(startScreen());
    //     }
    // }

    private void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SendSet();
        }
    }

    private void SendSet()
    {
        API_Provider.Instance.ActiveAPIController.SendSet("MAZE", 1);
    }

    IEnumerator startScreen()
    {
        yield return new WaitForSeconds(1.0f);
        while (gameActive)
        {
            // API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] {"\"\"", "\"\""});
            // yield return new WaitForSeconds(0.25f);
            // API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] { "\"--MAZE-SEQ--\"", "\"------------\"" });
            // yield return new WaitForSeconds(1f);
            // for (int i = 0; i < maze_controller.data.correctSequence.Length / 2; i++)
            // {
            //     string v1 = GetDirectionString(maze_controller.data.correctSequence[i * 2]);
            //     string v2 = GetDirectionString(maze_controller.data.correctSequence[i * 2 + 1]);
            //
            //     API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] { $"\"{v1}\"", $"\"{v2}\"" });
            //     yield return new WaitForSeconds(0.5f); 
            //     API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] {"\"\"", "\"\""}); // Blankline
            //     yield return new WaitForSeconds(0.25f); 
            // }
            API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] { "\"--MAZE-SEQ--\"", "\"------------\"" });
            yield return new WaitForSeconds(3f);
            API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] { $"\"U-U-U-L-L-U\"", $"\"U-R-R-R-D-R\"" });
            yield return new WaitForSeconds(5f);
            API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] { $"\"R-U-U-U-L-L\"", $"\"L\"" });
            yield return new WaitForSeconds(5f);
        }
    }
    
    public string GetDirectionString(int dirInt)
    {
        return dirInt switch
        {
            0 => "UP",
            1 => "DOWN",
            2 => "LEFT",
            3 => "RIGHT",
            _ => "WTF IS THIS"
        };
    }
}