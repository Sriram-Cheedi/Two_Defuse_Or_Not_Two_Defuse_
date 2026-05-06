using System;
using System.Collections;
using PersistantSystems.Software;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class OldGameManager : MonoBehaviour
{
    public static OldGameManager Instance { get; private set; }

    public double bombTimer = 25.0;
    public bool isGameOver = false;

    public enum GameState { Menu, CutScene, ReadyUP ,Playing, GameOver, GameWin }
    public GameState currentState = GameState.Menu;
    [Header("Cutscene References")]
    [SerializeField] private VideoPlayer cutscenePlayer;
    [SerializeField] private GameObject cutsceneDisplay;
    [SerializeField] private GameObject menuUI;
    [Header("Warning")] private AudioSource WarningAudio;
    
    public static event Action<GameState> onGameStateChanged;
    
    private bool pause = false;
    private float PreviousTimeScale = 1f;
    
    private bool isMatSceneLoaded = false;
    private bool isHardwareReady = false;
    private bool isSoftwareReady = false;
    
    public static event Action<double> onGameEnd;
    private string curr_username;
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        API_Controller.OnInputChanged += SetHardwareReady;
        Mock_API_Controller.OnInputChanged += SetHardwareReady;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        API_Controller.OnInputChanged -= SetHardwareReady;
        Mock_API_Controller.OnInputChanged -= SetHardwareReady;
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
        // Initial ReadyUp
        onGameStateChanged?.Invoke(currentState);
        if (currentState == GameState.ReadyUP)
        {
            if (isHardwareReady && isSoftwareReady)
            {
                currentState = GameState.Playing;
                onGameStateChanged?.Invoke(currentState);
            }
        }

        if (currentState == GameState.Playing && bombTimer > 0.0f && !pause)
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

    public void SetHardwareReady(string mod, int val)
    {
        if (mod == "PWR" && val == 1)
        {
            isHardwareReady = true;
            Debug.Log("Hardware ready");
        }
    }

    public void StartGame()
    {
        if (currentState == GameState.Playing || currentState == GameState.CutScene ||  currentState == GameState.ReadyUP)
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
            currentState = GameState.ReadyUP;
            isSoftwareReady = true;
            SceneManager.LoadScene("MatScene");
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        cutscenePlayer.loopPointReached -= OnVideoFinished;
        currentState = GameState.ReadyUP;
        SceneManager.LoadScene("MatScene");
        isSoftwareReady = true;
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