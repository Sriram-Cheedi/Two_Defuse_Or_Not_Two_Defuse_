using System;
using Ionic.BZip2;
using Puzzles;
using Puzzles.Controllers;
using UnityEngine;

public class Indication_Wire : MonoBehaviour
{
    [SerializeField] SSM_Controller ssm_controller;
    [SerializeField] public Material _materialON;
    
    private Renderer _renderer;
    
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        Puzzle.OnSuccess += ShowComplete;
    }

    private void OnDisable()
    {
        Puzzle.OnSuccess -= ShowComplete;
    }

    private void ShowComplete(GameObject obj)
    {
        if (obj != ssm_controller.gameObject) return;
        
        _renderer.material = _materialON;
    }
}
