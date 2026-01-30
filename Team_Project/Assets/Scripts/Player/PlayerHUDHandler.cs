using TMPro;
using UnityEngine;

public class PlayerHUDHandler : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    public void Update()
    {
        if (GameManager.Instance != null)
        {
            timerText.text = GameManager.Instance.bombTimer.ToString("F2");
        }
    }
}
