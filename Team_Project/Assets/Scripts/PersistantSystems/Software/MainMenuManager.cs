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
            playButton.onClick.AddListener(OnPlayClicked);
        else
            Debug.LogWarning("MainMenuManager: playButton is not assigned.");

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
        else
            Debug.LogWarning("MainMenuManager: quitButton is not assigned.");

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
        else
            Debug.LogWarning("MainMenuManager: settingsButton is not assigned.");

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
        else
            Debug.LogWarning("MainMenuManager: backButton is not assigned.");
    }

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayClicked);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettingsClicked);

        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackClicked);
    }

    private void OnPlayClicked()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("MainMenuManager: GameManager.Instance is null.");
            return;
        }

        Debug.Log("MainMenuManager: Play clicked.");
        GameManager.Instance.StartGame();
    }

    private void OnQuitClicked()
    {
        Debug.Log("MainMenuManager: Quit clicked.");
        Application.Quit();
    }

    private void OnSettingsClicked()
    {
        if (SettingPageManager.Instance == null)
        {
            Debug.LogError("MainMenuManager: SettingPageManager.Instance is null.");
            return;
        }

        Debug.Log("MainMenuManager: Settings clicked.");
        SettingPageManager.Instance.OpenSettings();
    }

    private void OnBackClicked()
    {
        if (SettingPageManager.Instance == null)
        {
            Debug.LogError("MainMenuManager: SettingPageManager.Instance is null.");
            return;
        }

        Debug.Log("MainMenuManager: Back clicked.");
        SettingPageManager.Instance.CloseSettings();
    }
}