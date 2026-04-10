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
        public bool IsHardwareReady  { get; private set; }

        public bool IsMatSceneLoaded { get; private set; }
        
        [Header("Cutscene References")]
        [SerializeField] private VideoPlayer cutscenePlayer;
        [SerializeField] private GameObject  cutsceneDisplay;
        [SerializeField] private GameObject  menuUI;

        [Header("Audio")]
        [SerializeField] private AudioSource warningAudio;
        [SerializeField] private AudioSource winningAudio;
        
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
            RegisterStates();
            GetAudioComponent();
            InitializeState(GameState.Menu);
        }
        
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            API_Controller.OnInputChanged += OnHardwareInputChanged;
            Mock_API_Controller.OnInputChanged += OnHardwareInputChanged;
            // WireCut.HasCutAnyWire += PlayWarningAudio;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            API_Controller.OnInputChanged -= OnHardwareInputChanged;
            Mock_API_Controller.OnInputChanged -= OnHardwareInputChanged;
            // WireCut.HasCutAnyWire -= PlayWarningAudio;
        }

        private void GetAudioComponent()
        {
            warningAudio =  GetComponent<AudioSource>();
        }

        private void RegisterStates()
        {
            States[GameState.Menu] = new MenuState();
            States[GameState.CutScene] = new CutSceneState(this, cutscenePlayer, cutsceneDisplay, menuUI);
            States[GameState.ReadyUp] = new ReadyUpState(this);
            States[GameState.Playing] = new PlayingState(this, warningAudio);
            States[GameState.GameOver] = new GameOverState(this);
            States[GameState.GameWin] = new GameWinState(this);
        }

        public void StartGame()
        {
            if (CurrentStateKey == GameState.CutScene ||
                CurrentStateKey == GameState.ReadyUp ||
                CurrentStateKey == GameState.Playing)
            {
                return;
            }
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
        
        public void ResetSession(double startingBombTimer = 300.0)
        {
            BombTimer = startingBombTimer;
            IsHardwareReady = false;
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
        }

        // public void PlayWarningAudio()
        // {
        //     while (true)
        //     {
        //         if (warningAudio != null && !warningAudio.isPlaying)
        //         {
        //             warningAudio.Play();
        //         }
        //     }
        // }
    }
}