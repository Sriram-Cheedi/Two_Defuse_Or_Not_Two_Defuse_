using System;
using UnityEngine;

public class MazeClear : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] { "\"\"", "\"\"" });
            API_Provider.Instance.ActiveAPIController.SetMazeStatus("MAZE", 0);
        }
    }
}
