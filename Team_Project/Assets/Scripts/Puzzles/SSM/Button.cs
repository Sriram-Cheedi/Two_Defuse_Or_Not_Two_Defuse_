using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class Button : MonoBehaviour, Interactible_Interface
{
    [SerializeField] private int button_number; 
    
    [SerializeField] public Material _materialON;
    [SerializeField] public Material _materialOFF;
    [SerializeField] public Material _incorrectMaterial;

    private Material _activeMaterial;
    private Renderer _renderer;
    
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    void Start()
    {
        PuzzleManager.Instance.puzzleDatas["SSM"].OnFailure += FlashRed;
        PuzzleManager.Instance.puzzleDatas["SSM"].OnSuccess += DisableInteraction;
    }

    private void OnDisable()
    {
        PuzzleManager.Instance.puzzleDatas["SSM"].OnFailure -= FlashRed;
        PuzzleManager.Instance.puzzleDatas["SSM"].OnSuccess -= DisableInteraction;
    }
    
    private void DisableInteraction()
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        StopAllCoroutines();
        _renderer.material = _materialON; 
    }
    
    public void Interact(RaycastHit hit)
    {
        StartCoroutine(HighlightSelected());
        PuzzleManager.Instance.CheckSSMButtonPress(button_number);
    }
    
    private void FlashRed()
    {
        StopAllCoroutines(); 
        StartCoroutine(FlashColorRoutine( 1.0f));
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
}
