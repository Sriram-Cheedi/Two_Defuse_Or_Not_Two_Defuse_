using System;
using PersistantSystems.Software.GameStateMachine.GameStates;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace PersistantSystems.Software.GameStateMachine
{
    public class GameManager : GameStateMachine
    {
        public static GameManager Instance { get; private set; }
        public double BombTimer;

        public bool IsMatSceneLoaded { get; private set; }

        [SerializeField] private ScoreManager scoreManager;
        
        [Header("Cutscene References")]
        [SerializeField] private VideoPlayer cutscenePlayer;
        [SerializeField] private GameObject  cutsceneDisplay;
        [SerializeField] private GameObject  menuUI;
        [SerializeField] private GameObject  loadingPage;

        [Header("Audio")]
        [SerializeField] private AudioClip warningAudio;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip loseSound;
        [SerializeField] private AudioClip winSound;
        [SerializeField] private AudioClip menuSound;
        [SerializeField] private AudioClip cutSceneSound;
        [SerializeField] private AudioClip scoreBoardSound;
        
        [Header("Audience Display 2 References")]
        [SerializeField] private GameObject audienceMainMenuDisplay;
        [SerializeField] private GameObject audienceCutsceneDisplay;
        
        public static event Action<double> OnGameEnd;

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
                return;
            }

            CacheRefs();
            ShowAudienceMainMenu();
            RegisterStates();
            GetAudioComponent();
            InitializeState(GameState.Menu);
            
        }

        private void ShowAudienceMainMenu()
        {
            Debug.Log("ShowAudienceMainMenu called");

            if (audienceMainMenuDisplay != null)
            {
                audienceMainMenuDisplay.SetActive(true);
                Debug.Log("AudienceMainMenuDisplay ON");
            }
            else
            {
                Debug.LogWarning("audienceMainMenuDisplay is NULL");
            }

            if (audienceCutsceneDisplay != null)
            {
                audienceCutsceneDisplay.SetActive(false);
                Debug.Log("AudienceCutsceneDisplay OFF");
            }
            else
            {
                Debug.LogWarning("audienceCutsceneDisplay is NULL");
            }
        }

        private void ShowAudienceCutscene()
        {
            if (audienceMainMenuDisplay != null)
            {
                audienceMainMenuDisplay.SetActive(false);
            }

            if (audienceCutsceneDisplay != null)
            {
                audienceCutsceneDisplay.SetActive(true);
            }
        }

        private GameObject FindChildByName(Transform parent, string name)
        {
            foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                    return child.gameObject;
            }

            Debug.LogWarning($"{name} not found under {parent.name}");
            return null;
        }

        public void CacheRefs()
        {
            Scene mainMenuScene = SceneManager.GetSceneByName("MainMenu");

            if (!mainMenuScene.IsValid() || !mainMenuScene.isLoaded)
            {
                return;
            }

            GameObject[] rootObjects = mainMenuScene.GetRootGameObjects();

            cutscenePlayer = null;
            cutsceneDisplay = null;
            menuUI = null;
            loadingPage = null;
            audienceMainMenuDisplay = null;
            audienceCutsceneDisplay = null;

            foreach (GameObject root in rootObjects)
            {
                Debug.Log("Checking root: " + root.name);

                if (cutscenePlayer == null)
                    cutscenePlayer = root.GetComponentInChildren<VideoPlayer>(true);

                if (cutsceneDisplay == null)
                    cutsceneDisplay = FindChildByName(root.transform, "CutSceneDisplay");

                if (menuUI == null)
                    menuUI = FindChildByName(root.transform, "Background");

                if (loadingPage == null)
                    loadingPage = FindChildByName(root.transform, "LoadingPage");
                
                if (audienceMainMenuDisplay == null)
                    audienceMainMenuDisplay = FindChildByName(root.transform, "AudienceMainMenuDisplay");

                if (audienceCutsceneDisplay == null)
                    audienceCutsceneDisplay = FindChildByName(root.transform, "AudienceCutsceneDisplay");
            }
            
            scoreManager = FindFirstObjectByType<ScoreManager>(FindObjectsInactive.Include);

        }
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            API_Controller.OnInputChanged += OnHardwareInputChanged;
            Mock_API_Controller.OnInputChanged += OnHardwareInputChanged;
            WireCut.HasCutAnyWire += HandleWireCut;
            GameEvents.OnReplay += ReplayGame;
            GameEvents.OnQuit += QuitGame;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            API_Controller.OnInputChanged -= OnHardwareInputChanged;
            Mock_API_Controller.OnInputChanged -= OnHardwareInputChanged;
            WireCut.HasCutAnyWire -= HandleWireCut;
            GameEvents.OnReplay -= ReplayGame;
            GameEvents.OnQuit -= QuitGame;
            
        }

        private void GetAudioComponent()
        {
            audioSource =  GetComponent<AudioSource>();
        }

        private void RegisterStates()
        {
            States[GameState.Menu] = new MenuState(this, audioSource, menuSound);
            States[GameState.CutScene] = new CutSceneState(this, cutscenePlayer, menuUI, cutsceneDisplay, loadingPage, audioSource, cutSceneSound);
            States[GameState.ReadyUp] = new ReadyUpState(this);
            States[GameState.Playing] = new PlayingState(this, audioSource, warningAudio);
            States[GameState.GameOver] = new GameOverState(this, audioSource, loseSound);
            States[GameState.GameWin] = new GameWinState(this, audioSource, winSound);
            States[GameState.DashBoard] = new ScoreDashBoardState(this, audioSource, scoreBoardSound);
        }

        public void StartGame()
        {
            if (CurrentStateKey == GameState.CutScene ||
                CurrentStateKey == GameState.ReadyUp ||
                CurrentStateKey == GameState.Playing)
            {
                return;
            }
            API_Provider.Instance?.ActiveAPIController?.RequestStatus("PWR");
            API_Provider.Instance?.ActiveAPIController?.SendSet("PWR", API_Provider.Instance.ActiveAPIController.GetDigital("PWR"));
            ShowAudienceCutscene();
            QueueNextState(GameState.CutScene);
        }
        
        public void EndGame()
        {
            if (CurrentStateKey == GameState.GameOver)
                return;

            QueueNextState(GameState.GameOver);
        }

        public void WinGame()
        {
            if (CurrentStateKey == GameState.GameWin)
                return;

            QueueNextState(GameState.GameWin);
        }
        
        public void FireOnGameEnd()
        {
            OnGameEnd?.Invoke(BombTimer);
        }
        
        public void OnCutSceneFinished()
        {
            SceneManager.LoadScene("MatScene");
            QueueNextState(GameState.ReadyUp);
        }
        
        public void SkipCutScene()
        {
            SceneManager.LoadScene("MatScene");
            QueueNextState(GameState.ReadyUp);
        }

        public void OnLoadingScoreBoard()
        {
            // SceneManager.UnloadSceneAsync("MatScene");
            SceneManager.LoadScene("ScoreDashBoard");
            QueueNextState(GameState.DashBoard);
        }
        
        public void ResetSession(double startingBombTimer = 300.0)
        {
            BombTimer = startingBombTimer;
        }
        
        private void OnHardwareInputChanged(string mod, int val)
        {
            if (CurrentState is IHardwareListener listener)
            {
                listener.OnHardwareInputChanged(mod, val);
            }
        }

        public void OnHardwareDisconnect()
        {
            if (CurrentState is IHardwareListener listener)
            {
                listener.OnHardwareDisconnect();
            }
        }

        public void OnHardwareReconnect()
        {
            if (CurrentState is IHardwareListener listener)
            {
                listener.OnHardwareReconnect();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            IsMatSceneLoaded = scene.name == "MatScene";
            if (scene.name == "MainMenu")
            {
                CacheRefs();
                RegisterStates();
                ShowAudienceMainMenu();
            }
        }
        
        private string currentUsername = "---";
        public void SetCurrentUsername(string username)
        {
            currentUsername = string.IsNullOrWhiteSpace(username) ? "---" : username;
        }

        public string GetCurrentUsername()
        {
            return string.IsNullOrWhiteSpace(currentUsername) ? "---" : currentUsername;
        }

        private void HandleWireCut()
        {
            BombTimer = 3.0f;
        }
        
        public void QuitGame()
        {
            Application.Quit();
        }


        public void ReplayGame()
        {
            scoreManager = FindFirstObjectByType<ScoreManager>(FindObjectsInactive.Include);
            scoreManager?.ResetPlayerSession();
            // SetCurrentUsername("---");
            QueueNextState(GameState.Menu);
            VerletInsulatedWire.ResetStaticState();
            SceneManager.LoadScene("MainMenu");
            ResetSession();
        }
    }
}
