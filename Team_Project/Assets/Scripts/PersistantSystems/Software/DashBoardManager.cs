using PersistantSystems.Software.GameStateMachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class DashBoardManager : MonoBehaviour {
    [SerializeField] private UnityEngine.UI.Button quitButton;
    [SerializeField] private UnityEngine.UI.Button replayButton;

    void Start() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        replayButton.onClick.AddListener(GameManager.Instance.ReplayGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}