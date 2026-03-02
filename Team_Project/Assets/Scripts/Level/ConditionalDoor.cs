using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class ConditionalDoor : MonoBehaviour
{
    private bool _puzzleCompleted = false;
    private bool _isOpen = false;

    private GameObject left_door;
    private GameObject right_door;
    
    private float moveDistance = 2.0f;

    [SerializeField] private string linkedPuzzle = "SSM";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    

    void Start()
    {
        left_door = GameObject.Find("Door_Left");
        right_door = GameObject.Find("Door_Right");

        if (PuzzleManager.Instance != null && PuzzleManager.Instance._puzzleTriggers.ContainsKey("SSM"))
        {
            PuzzleManager.Instance._puzzleTriggers[linkedPuzzle].OnSuccess += OpenDoorWrapper; 
        }
    }

    private void OnDisable()
    {
        PuzzleManager.Instance._puzzleTriggers[linkedPuzzle].OnSuccess -= OpenDoorWrapper;
    }

    // Update is called once per frame
    private void OpenDoorWrapper()
    {
        if (!_isOpen)
        {
            StartCoroutine(OpenDoors());
            _isOpen = true;
        }
    }
    
    IEnumerator OpenDoors()
    {
        float elapsed = 0;
        float duration = 1.0f;
    
        Vector3 leftStart = left_door.transform.position;
        Vector3 rightStart = right_door.transform.position;
    
        Vector3 leftTarget = leftStart + transform.forward * moveDistance;
        Vector3 rightTarget = rightStart - transform.forward * moveDistance;

        while (elapsed < duration)
        {
            left_door.transform.position = Vector3.Lerp(leftStart, leftTarget, elapsed / duration);
            right_door.transform.position = Vector3.Lerp(rightStart, rightTarget, elapsed / duration);
        
            elapsed += Time.deltaTime;
            yield return null;
        }
    
        left_door.transform.position = leftTarget;
        right_door.transform.position = rightTarget;
    }
    
}
