using PersistantSystems.Software;
using PersistantSystems.Software.GameStateMachine;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button playButton;
    [SerializeField] private UnityEngine.UI.Button quitButton;
    [SerializeField] private UnityEngine.UI.Button settingsButton;
    [SerializeField] private UnityEngine.UI.Button backButton;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject loadingPage;

    private void Awake()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    private void OnDestroy()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OnPlayClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(OnSettingsClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
        }
    }

    private void OnPlayClicked()
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        GameManager.Instance.StartGame();
    }

    private void OnQuitClicked()
    {
        Application.Quit();
    }

    private void OnSettingsClicked()
    {
        if (SettingPageManager.Instance == null)
        {
            return;
        }
        SettingPageManager.Instance.OpenSettings();
    }

    private void OnBackClicked()
    {
        if (SettingPageManager.Instance == null)
        { 
            return;
        }
        SettingPageManager.Instance.CloseSettings();
    }
}