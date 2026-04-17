using System;
using PersistantSystems.Software.GameStateMachine.GameStates;
using Puzzles;
using Puzzles.Minor_Interaction;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private DoorID doorID;
    [SerializeField] private DoorType doorType;
    
    [SerializeField] private Minor_Interactinon_Controller interactinonController;
    [SerializeField] private BarredDoorAnimation doorAnimation;
    
    [SerializeField] private Transform hingedDoorTransform;
    [SerializeField] private Transform hinge;
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip doorOpeningClip;
    
    private bool isOpen;
    private bool hasOpened;
    private bool hasClosed;
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        interactinonController.DoorOpenRequested += HandleDoorOpenRequested;
        interactinonController.DoorCloseRequested += HandleDoorCloseRequested;
        Puzzle.OnSuccess += OpenPuzzleDoor;
        ReadyUpState.TrapDoorOpened += OpenTrapDoor;
    }

    private void OnDisable()
    {
        interactinonController.DoorOpenRequested -= HandleDoorOpenRequested;
        interactinonController.DoorCloseRequested -= HandleDoorCloseRequested;
        Puzzle.OnSuccess -= OpenPuzzleDoor;
        ReadyUpState.TrapDoorOpened -= OpenTrapDoor;
    }

    private void OpenPuzzleDoor(PuzzleGame puzzleGame)
    {
        switch (puzzleGame)
        {
            case PuzzleGame.SSM:
                HandleDoorOpenRequested(DoorID.SSMDoor);
                break;
            case PuzzleGame.NGM:
                HandleDoorOpenRequested(DoorID.NGMDoor);
                break;
        }
    }

    private void OpenTrapDoor()
    {
        HandleDoorOpenRequested(DoorID.TrapDoor);
    }

    private void HandleDoorOpenRequested(DoorID requestedDoorId)
    {
        if (requestedDoorId != doorID || isOpen || hasOpened) return;
        switch (doorType)
        {
            case DoorType.Hinged:
                if (hinge == null || hingedDoorTransform == null) return;
                OpenHingedDoor();
                break;
            case DoorType.Sliding:
                if (leftDoor == null || rightDoor == null) return;
                OpenSlidingDoor();
                break;
        }
        
    }
    
    private void HandleDoorCloseRequested(DoorID requestedDoorId)
    {
        if (requestedDoorId != doorID || !isOpen || hasClosed) return;
        switch (doorType)
        {
            case DoorType.Hinged:
                CloseHingedDoor();
                break;
            case DoorType.Sliding:
                CloseSlidingDoor();
                break;
        }
    }

    private void OpenHingedDoor()
    {
        isOpen = true;
        hasOpened = true;
        switch (doorID)
        {
            case DoorID.TutorialDoor:
                doorAnimation.OpenDoor(hingedDoorTransform.transform, hinge, Vector3.forward, 1f);
                break;
            case DoorID.MazeDoor:
                doorAnimation.OpenDoor(hingedDoorTransform.transform, hinge, Vector3.up, 1f);
                break;
        }
        
        audioSource.PlayOneShot(doorOpeningClip);
    }
    
    private void CloseHingedDoor()
    {
        isOpen = false;
        hasClosed = true;
        switch (doorID)
        {
            case DoorID.TutorialDoor:
                doorAnimation.OpenDoor(hingedDoorTransform.transform, hinge, -Vector3.forward, 1f);
                break;
            case DoorID.MazeDoor:
                doorAnimation.OpenDoor(hingedDoorTransform.transform, hinge, -Vector3.up, 1f);
                break;
        }
        audioSource.PlayOneShot(doorOpeningClip);
    }

    private void OpenSlidingDoor()
    {
        isOpen = true;
        hasOpened = true;
        switch (doorID)
        {
            case DoorID.TrapDoor:
                doorAnimation.OpenSlidingDoor(leftDoor, rightDoor, Directive.OPEN);
                break;
            case DoorID.SSMDoor:
                doorAnimation.OpenSlidingDoor(leftDoor, rightDoor, Directive.OPEN);
                break;
            case DoorID.NGMDoor:
                doorAnimation.OpenSlidingDoor(leftDoor, rightDoor, Directive.OPEN);
                break;
        }
        audioSource.PlayOneShot(doorOpeningClip);
    }
    
    private void CloseSlidingDoor()
    {
        isOpen = false;
        hasClosed = true;
        switch (doorID)
        {
            case DoorID.TrapDoor:
                doorAnimation.OpenSlidingDoor(leftDoor, rightDoor, Directive.CLOSE);
                break;
            case DoorID.SSMDoor:
                doorAnimation.OpenSlidingDoor(leftDoor, rightDoor, Directive.CLOSE);
                break;
            case DoorID.NGMDoor:
                doorAnimation.OpenSlidingDoor(leftDoor, rightDoor, Directive.CLOSE);
                break;
        }
        audioSource.PlayOneShot(doorOpeningClip);
    }
}
