using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public double bombTimer = 25.0;
    public bool isGameOver = false;

    public enum GameState { Menu, Playing, GameOver }
    public GameState currentState = GameState.Menu;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

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
        SceneManager.LoadScene("StartLevel");
    }

    public void EndGame()
    {
        if (currentState == GameState.GameOver) return;

        currentState = GameState.GameOver;
        SceneManager.LoadScene("GameOver");
    }

}