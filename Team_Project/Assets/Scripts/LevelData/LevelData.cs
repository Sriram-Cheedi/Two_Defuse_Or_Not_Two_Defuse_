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
}
