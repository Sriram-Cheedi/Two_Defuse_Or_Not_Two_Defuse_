using System;
using System.Collections;
using Puzzles;
using RigidBodyPlayer.PlayerData;
using UnityEngine;

public class SSM_Gravity_Enable : MonoBehaviour
{
    [SerializeField] private SurfaceIdentity[] floorTiles;
    [SerializeField] private GameObject GravityHUD;
    [SerializeField] private GameObject[] HudsToDisable;
    [SerializeField] private GameObject[] CollidersToDisable;
    [SerializeField] private GameObject[] CollidersToEnable;
    [SerializeField] private LevelData levelData;

    private void Awake()
    {
        // GravityHUD.SetActive(false);
        floorTiles = GetComponentsInChildren<SurfaceIdentity>();
        foreach (var collider in CollidersToDisable)
        {
            collider.SetActive(true);
        }
        
        foreach (var collider in CollidersToEnable)
        {
            collider.SetActive(false);
        }
        foreach (var tile in floorTiles)
        {
            tile.type = SurfaceType.Plastic;
        }
    }

    private void OnEnable()
    {
        Puzzle.OnSuccess += EnableGravitySystem;
    }

    private void OnDisable()
    {
        Puzzle.OnSuccess -= EnableGravitySystem;
    }

    private void EnableGravitySystem(PuzzleGame game)
    {
        if (game != PuzzleGame.SSM) return;
        HandleHuds();
        // StartCoroutine(ShowGravityAlertForSeconds());
        foreach (var tile in floorTiles)
        {
            tile.type = SurfaceType.Plastic;
        }
    }

    private void HandleHuds()
    {
        foreach (var hud in HudsToDisable)
        {
            hud.SetActive(false);
        }

        foreach (var collider in CollidersToDisable)
        {
            collider.SetActive(false);
        }
        
        // foreach (var collider in CollidersToEnable)
        // {
        //     collider.SetActive(true);
        // }
    }
    
    // IEnumerator ShowGravityAlertForSeconds()
    // {
    //     GravityHUD.SetActive(true);
    //     yield return new WaitForSeconds(levelData.surfaceIntroAlertDuration);
    //     GravityHUD.SetActive(false);
    // }
}
