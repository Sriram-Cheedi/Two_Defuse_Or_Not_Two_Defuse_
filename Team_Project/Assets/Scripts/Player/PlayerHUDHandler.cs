using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHUDHandler : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    // public GameObject time_bars;
    
    public void Update()
    {
        timerText.text = GameManager.Instance.bombTimer.ToString("F2");
        
        // if (GameManager.Instance.bombTimer < 20.0 && time_bars.gameObject != null && !time_bars.activeSelf)
        // {
        //     TogglePopup();
        // }
    }
    public void TogglePopup()
    {
        // time_bars.SetActive(true);
    }
}

