using PersistantSystems.Software;
using PersistantSystems.Software.GameStateMachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button playButton;
    [SerializeField] private UnityEngine.UI.Button quitButton;
    [SerializeField] private UnityEngine.UI.Button settingsButton;
    [SerializeField] private UnityEngine.UI.Button backButton;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject loadingPage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playButton.onClick.AddListener(GameManager.Instance.StartGame);
        quitButton.onClick.AddListener(Application.Quit);
        settingsButton.onClick.AddListener(SettingPageManager.Instance.OpenSettings);
        backButton.onClick.AddListener(SettingPageManager.Instance.CloseSettings);
    }   
}
