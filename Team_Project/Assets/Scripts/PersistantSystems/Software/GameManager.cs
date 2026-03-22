using System;
using System.Collections;
using PersistantSystems.Software;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public double bombTimer = 25.0;
    public bool isGameOver = false;

    public enum GameState { Menu, CutScene, Playing, GameOver, GameWin }
    public GameState currentState = GameState.Menu;
    [Header("Cutscene References")]
    [SerializeField] private VideoPlayer cutscenePlayer;
    [SerializeField] private GameObject cutsceneDisplay;
    [SerializeField] private GameObject menuUI;
    [Header("Warning")] private AudioSource WarningAudio;
    
    private bool pause = false;
    private float PreviousTimeScale = 1f;
    
    private bool isMatSceneLoaded = false;

    public static event Action<double> onGameEnd;
    private string curr_username;
    
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
        if (currentState == GameState.Playing || currentState == GameState.CutScene)
            return;

        currentState = GameState.CutScene;

        

        if (cutsceneDisplay != null)
            cutsceneDisplay.SetActive(true);

        if (cutscenePlayer != null)
        {
            cutscenePlayer.loopPointReached -= OnVideoFinished;
            cutscenePlayer.loopPointReached += OnVideoFinished;
            cutscenePlayer.Play();
            if (menuUI != null)
                menuUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Cutscene player not assigned. Loading game directly.");
            currentState = GameState.Playing;
            SceneManager.LoadScene("MatScene");
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        cutscenePlayer.loopPointReached -= OnVideoFinished;
        currentState = GameState.Playing;
        SceneManager.LoadScene("MatScene");
    }

    public void EndGame()
    {
        onGameEnd?.Invoke(bombTimer);
        if (currentState == GameState.GameOver) return;

        currentState = GameState.GameOver;
        API_Provider.Instance.ActiveAPIController.SendSet("PWR", 4);
        SceneManager.LoadScene("Explode");
        StartCoroutine(GameoverAnimation());
    }

    public void WinGame()
    {
        onGameEnd?.Invoke(bombTimer);
        if (currentState == GameState.GameWin) return;
        currentState = GameState.GameWin;
        API_Provider.Instance.ActiveAPIController.SendSet("PWR", 4);
        SceneManager.LoadScene("GameWin");
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
        Time.timeScale = 0f;
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