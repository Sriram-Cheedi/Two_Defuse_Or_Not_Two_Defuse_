using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance
    {
        get;
        private set;
    }

    public double timer = 10;
    public bool isGameOver = false;

    public bool isLoading = false;

    public enum GameState { Menu, Playing, GameOver }
    public GameState currentState = GameState.Menu;


    private void Awake()
    {
        // Singleton Logic: If an instance already exists, destroy this one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0 && currentState != GameState.GameOver)
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