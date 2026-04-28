using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text teamNameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Image statusBadgeImage;

    [Header("Badge Sprites")]
    [SerializeField] private Sprite defusedBadge;
    [SerializeField] private Sprite explodedBadge;

    public void SetRow(int rank, string teamName, int score, bool defused)
    {
        rankText.text = rank.ToString();
        teamNameText.text = teamName;
        scoreText.text = score.ToString();

        statusBadgeImage.enabled = true;
        statusBadgeImage.sprite = defused ? defusedBadge : explodedBadge;
    }

    public void SetEmpty(int rank)
    {
        rankText.text = rank.ToString();
        teamNameText.text = "---";
        scoreText.text = "---";
        statusBadgeImage.enabled = false;
    }
}