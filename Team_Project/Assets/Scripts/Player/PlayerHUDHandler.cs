using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHUDHandler : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    
    public void Update()
    {
        timerText.text = GameManager.Instance.bombTimer.ToString("F2");
    }
}

