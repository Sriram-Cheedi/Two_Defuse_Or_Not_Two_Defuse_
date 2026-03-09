using System;
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
        ssm_controller.data.OnSuccess += ShowComplete;
    }

    private void OnDisable()
    {
        ssm_controller.data.OnSuccess -= ShowComplete;
    }

    private void ShowComplete()
    {
        _renderer.material = _materialON;
    }
}
