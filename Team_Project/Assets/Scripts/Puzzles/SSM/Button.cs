using System;
using System.Collections;
using Puzzles;
using Puzzles.Controllers;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class Button : MonoBehaviour, Interactible_Interface
{
    [SerializeField] SSM_Controller ssm_controller;
    
    [SerializeField] private int button_number; 
    
    [SerializeField] public Material _materialON;
    [SerializeField] public Material _materialOFF;
    [SerializeField] public Material _incorrectMaterial;
    [SerializeField] public Material _materialSuccess;
    [SerializeField] public GameObject _successAlert;
    public LevelData levelData;

    private Renderer _renderer;
    
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        Puzzle.OnFailure += FlashRed;
        Puzzle.OnSuccess += DisableInteractionSetOn;
    }

    private void OnDisable()
    {
        Puzzle.OnFailure -= FlashRed;
        Puzzle.OnSuccess -= DisableInteractionSetOn;

    }
    
    private void DisableInteractionSetOn(PuzzleGame puzzleGame)
    {
        if (puzzleGame != PuzzleGame.SSM) return;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        StopAllCoroutines();
        _renderer.material = _materialSuccess;
        if (_successAlert == null || levelData ==  null) return;
        StartCoroutine(ShowSuccessAlertForSeconds());
    }
    
    public void Interact(RaycastHit hit)
    {  
        StartCoroutine(HighlightSelected());
        ssm_controller.CheckSSMButtonPress(button_number);
    }
    
    private void FlashRed(PuzzleGame puzzle)
    {
        if (puzzle == PuzzleGame.SSM)
        {
            StopAllCoroutines(); 
            StartCoroutine(FlashColorRoutine( 1.0f));
        }
    }
    
    private IEnumerator FlashColorRoutine(float duration)
    {
        _renderer.material = _incorrectMaterial;
        
        yield return new WaitForSeconds(duration);
        
        _renderer.material = _materialOFF;
    }
    
    IEnumerator HighlightSelected()
    {
        float elapsed = 0;
        float duration = 1.0f;
        
        while (elapsed < duration)
        {   
            _renderer.material = _materialON;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        _renderer.material = _materialOFF;
    }

    IEnumerator ShowSuccessAlertForSeconds()
    {
        _successAlert.SetActive(true);
        yield return new WaitForSeconds(levelData.ssmSuccessAlertDuration);
        _successAlert.SetActive(false);
    }
}
