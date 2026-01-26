using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public double bombTimer = 10;
    public bool isGameOver = false;
    public enum GameState
    {
        Menu,
        Playing,
        GameOver
    }
    public static GameState currentState = GameState.Menu;

    public void Update()
    {
        bombTimer -= Time.deltaTime;
        if (bombTimer <= 0 && currentState != GameState.GameOver)
        {
            EndGame();
        }
    }

    public void StartGame()
    {
        if (currentState == GameState.Playing) return;

        currentState = GameState.Playing;
        SceneManager.LoadScene("SampleScene");
    }

    public void EndGame()
    {
        if (currentState == GameState.GameOver) return;

        currentState = GameState.GameOver;
        SceneManager.LoadScene("GameOver");
    }

}