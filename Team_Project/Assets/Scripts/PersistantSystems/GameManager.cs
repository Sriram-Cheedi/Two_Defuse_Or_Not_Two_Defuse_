using System;
using System.Collections;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public double bombTimer = 25.0;
    public bool isGameOver = false;
    public bool isTimerActive = false;

    public enum GameState { Menu, Playing, GameOver }
    public GameState currentState = GameState.Menu;
    [Header("Warning")]
     private AudioSource WarningAudio;

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
        WarningAudio = GetComponent<AudioSource>();

        if (bombTimer > 0.0f &&  currentState == GameState.Playing)
        {
            bombTimer -= Time.deltaTime;
            if (currentState == GameState.Playing && bombTimer <= 5f && bombTimer > 0f)
            {
                if (!WarningAudio.isPlaying)
                    WarningAudio.Play();
            }
            else
            {
                if (WarningAudio.isPlaying)
                    WarningAudio.Stop();
            }

        
            if (bombTimer <= 0 && currentState != GameState.GameOver)
            {
                isTimerActive = false;
                bombTimer = 0.0f;
                EndGame();
            }

            API_Provider.Instance.ActiveAPIController.TimerSet((int)bombTimer);
        }
    }


    public void StartGame()
    {
        Debug.Log("<color=blue>GameManager: Starting Game...</color>");
        if (currentState == GameState.Playing) return;

        currentState = GameState.Playing;
        SceneManager.LoadScene("StartLevel");

        isTimerActive = true;
    }

    public void EndGame()
    {
        if (currentState == GameState.GameOver) return;

        currentState = GameState.GameOver;
        SceneManager.LoadScene("Explode");
        StartCoroutine(GameoverAnimation());
        
    }

    IEnumerator GameoverAnimation()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("GameOver");
    }
}