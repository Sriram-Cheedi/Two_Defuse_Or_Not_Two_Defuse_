using TMPro;
using UnityEngine;
using PersistantSystems.Software;
using PersistantSystems.Software.GameStateMachine;

public class PlayButtonSubmitter : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameManager gameManager;

    public void SubmitAndPlay()
    {
        if (scoreManager == null)
            scoreManager = FindFirstObjectByType<ScoreManager>(FindObjectsInactive.Include);

        if (gameManager == null)
            gameManager = GameManager.Instance;

        string enteredName = usernameInput != null ? usernameInput.text.Trim() : "";

        scoreManager?.OnUsernameEntered(enteredName);
        gameManager?.StartGame();
    }
}