using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Level/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Spark")]
    public float doorDelay = 3f;
    
    [Header("Smoke")]
    public float maxRate = 8f;
    public float fillSpeed = 0.1f;
    public float clearSpeed = 5f;
}
