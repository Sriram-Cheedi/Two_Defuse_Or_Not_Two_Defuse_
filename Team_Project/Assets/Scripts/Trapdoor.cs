using System;
using UnityEngine;

public class Trapdoor : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.onGameStateChanged += Despawn;
    }
    
    private void Despawn(GameManager.GameState gameState)
    {
        if (gameState == GameManager.GameState.Playing) gameObject.SetActive(false);
    }
}
