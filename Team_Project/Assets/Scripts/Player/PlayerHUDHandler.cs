using PersistantSystems.Software.GameStateMachine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHUDHandler : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    
    public void Update()
    {
        timerText.text = GameManager.Instance.BombTimer.ToString("F2");
    }
}

