using TMPro;
using UnityEngine;

public class StationStatusPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text teamNameValueText;
    [SerializeField] private TMP_Text statusValueText;

    [Header("Status Colors")]
    [SerializeField] private Color waitingColor = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] private Color playingColor = new Color(1f, 0.78f, 0.2f);
    [SerializeField] private Color defusedColor = new Color(0.2f, 0.85f, 0.35f);
    [SerializeField] private Color explodedColor = new Color(0.9f, 0.2f, 0.2f);

    public void SetStatus(string teamName, string status)
    {
        if (string.IsNullOrWhiteSpace(teamName))
        {
            teamName = "---";
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            status = "WAITING";
        }
        string upperStatus = status.ToUpper();
        teamNameValueText.text = teamName;
        statusValueText.text = upperStatus;
        switch (upperStatus)
        {
            case "PLAYING":
                statusValueText.color = playingColor;
                break;

            case "DEFUSED":
                statusValueText.color = defusedColor;
                break;

            case "EXPLODED":
                statusValueText.color = explodedColor;
                break;

            case "WAITING":
            default:
                statusValueText.color = waitingColor;
                break;
        }
    }
}