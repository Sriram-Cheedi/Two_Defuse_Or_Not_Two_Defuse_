using UnityEngine;

public class GameplayStationConfig : MonoBehaviour
{
    public static string StationId = "station1";

    [SerializeField] private string stationId = "station1";

    private void Awake()
    {
        StationId = stationId;
        Debug.Log("Gameplay station configured as: " + StationId);
    }
}