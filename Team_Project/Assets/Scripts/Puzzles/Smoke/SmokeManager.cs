using System;
using UnityEngine;

public class SmokeManager : MonoBehaviour
{
    [SerializeField]private PlayerInputRelay relay;
    [SerializeField]private TrackRoom trackPlayerRoom;
    private bool smokeReleased;
    private SmokeEmitter lastRoomActive;

    private void Awake()
    {
        if (relay == null)
        {
            relay = GetComponent<PlayerInputRelay>();
        }
    }

    private void OnEnable()
    {
        relay.SmokeReleased += HandleSmoke; 
        API_Provider.Instance.ActiveAPIController.OnInputChanged += OnSmokeReleased;
    }

    private void OnDisable()
    {
        relay.SmokeReleased -= HandleSmoke; 
        API_Provider.Instance.ActiveAPIController.OnInputChanged -= OnSmokeReleased;
    }
    
    void Start() { }
    
    private void HandleSmoke(bool smoke)
    {
        smokeReleased = smoke;
        UpdateRoomSmoke();
    }

    private void OnSmokeReleased(String header, int value)
    {
        Debug.Log("Smoke Released: " + value);
        if (header == "FGM" && value == 2)
        {
            smokeReleased = true;
        } 
        else if (header == "FGM" && value == 1)
        {
            smokeReleased = false;
        }
        UpdateRoomSmoke();
    }
    
    private void Update()
    {
        UpdateRoomSmoke();
    }
    
    void UpdateRoomSmoke()
    {
        SmokeEmitter currentRoom  = trackPlayerRoom != null ? trackPlayerRoom.currentSmokeEmitter : null;

        if (lastRoomActive != null && lastRoomActive != currentRoom)
        {
            lastRoomActive.SmokeState(false);
        }

        if (currentRoom != null)
        {
            currentRoom.SmokeState(smokeReleased);
        }

        lastRoomActive = currentRoom;
    }
}
