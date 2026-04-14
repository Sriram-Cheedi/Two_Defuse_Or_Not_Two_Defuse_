using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Level/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Spark")]
    public float doorDelay = 3f;
    
    [Header("Smoke")]
    public float maxRate = 40f;
    public float fillSpeed = 0.01f;
    public float clearSpeed = 3f;
    public float alphaSpeed = 0.05f;
    public float targetAlpha = 1f;

    [Header("Flash")] 
    public float flashDuration = 1f;
    public float offDuration = 1f;
    public float ssmSuccessAlertDuration = 5f;
    public float warningFlashingDuration = 0.5f;
}
