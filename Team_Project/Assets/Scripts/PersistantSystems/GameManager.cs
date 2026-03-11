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

    public enum GameState { Menu, Playing, GameOver }
    public GameState currentState = GameState.Menu;
    [Header("Warning")]
    private AudioSource WarningAudio;
    
    
    private bool pause = false;
    private float PreviousTimeScale = 1f;
    
    private bool isMatSceneLoaded = false;
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        isMatSceneLoaded = (scene.name == "MatScene");
    }
    
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);   
        }
        else
        {
            Destroy(gameObject);
        }
        WarningAudio = GetComponent<AudioSource>();
        
    }
    public void Update()
    {

        if (bombTimer > 0.0f &&  currentState == GameState.Playing && !pause)
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
                EndGame();
                bombTimer = 0.0f;
            }

            if (isMatSceneLoaded && API_Provider.Instance != null && API_Provider.Instance.ActiveAPIController != null)
            {
                API_Provider.Instance.ActiveAPIController.TimerSet((int)bombTimer);
            }
        }
    }


    public void StartGame()
    {
        if (currentState == GameState.Playing) return;

        currentState = GameState.Playing;
        SceneManager.LoadScene("MatScene");
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
    
    public void OnHardwareDisconnect()
    {
        if (pause)
        {
            return;
        }
        pause = true;
        PreviousTimeScale = Time.timeScale;
        Time.timeScale = 1f;
        if (WarningAudio != null && WarningAudio.isPlaying)
        {
            WarningAudio.Stop();
        }
    }

    public void OnHardwareReconnect()
    {
        if (!pause)
        {
            return;
        }
        pause = false;
        if (PreviousTimeScale == 0f)
        {
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = PreviousTimeScale;
        }

        if (isMatSceneLoaded)
        {
            try
            {
                API_Provider.Instance.ActiveAPIController.TimerSet((int)bombTimer);
                API_Provider.Instance.ActiveAPIController.TimerOnOff(1);
            }
            catch
            {
                Debug.LogError("Unable to reset Bomb Timer when reconnecting...");
            }
        }
      
    }
}