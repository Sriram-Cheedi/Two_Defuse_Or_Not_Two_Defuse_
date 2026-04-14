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
        
        [Header("Cutscene References")]
        [SerializeField] private VideoPlayer cutscenePlayer;
        [SerializeField] private GameObject  cutsceneDisplay;
        [SerializeField] private GameObject  menuUI;
        [SerializeField] private GameObject  loadingPage;

        [Header("Audio")]
        [SerializeField] private AudioSource warningAudio;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip loseSound;
        [SerializeField] private AudioClip winSound;
        [SerializeField] private AudioClip menuSound;
        [SerializeField] private AudioClip cutSceneSound;
        [SerializeField] private AudioClip scoreBoardSound;
        
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
            RegisterStates();
            GetAudioComponent();
            InitializeState(GameState.Menu);
            
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
            var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (cutscenePlayer == null)
                cutscenePlayer = FindFirstObjectByType<VideoPlayer>(FindObjectsInactive.Include);

            if (cutsceneDisplay == null)
                cutsceneDisplay = FindChildByName(canvas.transform, "CutSceneDisplay");

            if (menuUI == null)
                menuUI = FindChildByName(canvas.transform, "Background");

            if (loadingPage == null)
                loadingPage = FindChildByName(canvas.transform, "LoadingPage");
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
            warningAudio =  GetComponent<AudioSource>();
        }

        private void RegisterStates()
        {
            States[GameState.Menu] = new MenuState(this, audioSource, menuSound);
            States[GameState.CutScene] = new CutSceneState(this, cutscenePlayer, menuUI, cutsceneDisplay, loadingPage, audioSource, cutSceneSound);
            States[GameState.ReadyUp] = new ReadyUpState(this);
            States[GameState.Playing] = new PlayingState(this, warningAudio);
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
            }
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
            QueueNextState(GameState.Menu);
            VerletWire.ResetStaticState();
            SceneManager.LoadScene("MainMenu");
            ResetSession();
        }
    }
}