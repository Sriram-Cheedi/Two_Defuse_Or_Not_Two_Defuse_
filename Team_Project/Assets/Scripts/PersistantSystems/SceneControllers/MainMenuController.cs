// using PersistantSystems.Software;
// using PersistantSystems.Software.GameStateMachine;
// using UnityEngine;
// using UnityEngine.Video;
//
// public class MainMenuController : MonoBehaviour
// {
//     [SerializeField] private UnityEngine.UI.Button playButton;
//     [SerializeField] private UnityEngine.UI.Button quitButton;
//     [SerializeField] private UnityEngine.UI.Button settingsButton;
//     [SerializeField] private UnityEngine.UI.Button backButton;
//     [SerializeField] private VideoPlayer cutscenePlayer;
//     [SerializeField] private GameObject cutsceneDisplay;
//     [SerializeField] private GameObject menuUI;
//     [SerializeField] private GameObject loadingPage;
//     private void Awake()
//     {
//         if (cutsceneDisplay != null)
//             cutsceneDisplay.SetActive(false);
//
//         if (loadingPage != null)
//             loadingPage.SetActive(false);
//
//         if (menuUI != null)
//             menuUI.SetActive(true);
//
//     }
//
//     private void OnEnable()
//     {
//         if (GameManager.Instance != null)
//             GameManager.Instance.RegisterMainMenuController(this);
//
//         if (cutscenePlayer != null)
//         {
//             cutscenePlayer.loopPointReached -= HandleCutsceneFinished;
//             cutscenePlayer.prepareCompleted -= OnVideoPrepared;
//         }
//         
//         if (playButton != null)
//             playButton.onClick.AddListener(OnPlayClicked);
//
//         if (quitButton != null)
//             quitButton.onClick.AddListener(OnQuitClicked);
//
//         if (settingsButton != null)
//             settingsButton.onClick.AddListener(OnSettingsClicked);
//
//         if (backButton != null)
//             // backButton.onClick.AddListener(OnBackClicked);
//     }
//     
//     private void OnPlayClicked()
//     {
//         GameManager.Instance?.StartGame();
//     }
//
//     private void OnQuitClicked()
//     {
//         Application.Quit();
//     }
//
//     private void OnSettingsClicked()
//     {
//         SettingPageManager.Instance?.OpenSettings();
//     }
//     
//     private void OnBackClicked()
//     {
//         SettingPageManager.Instance?.CloseSettings();
//     }
//
//     private void OnDisable()
//     {
//         if (GameManager.Instance != null)
//             GameManager.Instance.UnregisterMainMenuController(this);
//
//         if (cutscenePlayer != null)
//             cutscenePlayer.loopPointReached -= HandleCutsceneFinished;
//     }
//
//     public void OnSkipCutscene()
//     {
//         if (cutscenePlayer != null)
//         {
//             cutscenePlayer.Stop();
//             cutscenePlayer.prepareCompleted -= OnVideoPrepared;
//         }
//
//         if (cutsceneDisplay != null)
//             cutsceneDisplay.SetActive(false);
//
//         GameManager.Instance?.SkipCutScene();
//     }
//
//     public void ShowMenu(bool show)
//     {
//         if (menuUI != null)
//             menuUI.SetActive(show);
//     }
//
//     public void ShowLoading(bool show)
//     {
//         if (loadingPage != null)
//             loadingPage.SetActive(show);
//     }
//
//     public void PlayCutscene()
//     {
//         if (cutsceneDisplay != null)
//             cutsceneDisplay.SetActive(true);
//         
//         if (loadingPage != null)
//             loadingPage.SetActive(false);
//
//         if (cutscenePlayer == null)
//         {
//             OnSkipCutscene();
//             return;
//         }
//
//         cutscenePlayer.Stop();
//         cutscenePlayer.time = 0;
//         cutscenePlayer.Prepare();
//     }
//
//
//     private void OnVideoPrepared(VideoPlayer vp)
//     {
//         if (cutscenePlayer == null)
//             return;
//
//         cutscenePlayer.Play();
//     }
//
//     private void HandleCutsceneFinished(VideoPlayer source)
//     {
//         if (cutsceneDisplay != null)
//             cutsceneDisplay.SetActive(false);
//
//         GameManager.Instance?.OnCutSceneFinished();
//     }
// }
