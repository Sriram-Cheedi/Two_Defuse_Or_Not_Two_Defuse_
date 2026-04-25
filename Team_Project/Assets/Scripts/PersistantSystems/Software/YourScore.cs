using PersistantSystems.Software;
using TMPro;
using UnityEngine;

public class YourScore : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    // [SerializeField] private TMP_Text timeLeftText;

    private void OnEnable()
    {
        scoreText.text = FinalRunResults.yourScore.ToString();
        // timeLeftText.text = $"{Mathf.CeilToInt((float)FinalRunResults.bombTimeRemaining)}s";
    }
}