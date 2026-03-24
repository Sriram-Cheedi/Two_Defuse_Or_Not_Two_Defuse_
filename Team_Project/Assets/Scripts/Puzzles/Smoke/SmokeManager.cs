using System;
using NUnit.Framework.Constraints;
using UnityEngine;

public class SmokeManager : MonoBehaviour
{
    [SerializeField]private PlayerInputRelay relay;
    [SerializeField]private TrackRoom trackPlayerRoom;
    [SerializeField] private GameObject smokeAlert;
    private bool smokeReleased;
    private SmokeEmitter lastRoomActive;
    public bool isActive = false;

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
        API_Controller.OnInputChanged += OnSmokeReleased;
    }

    private void OnDisable()
    {
        relay.SmokeReleased -= HandleSmoke; 
        API_Controller.OnInputChanged -= OnSmokeReleased;
    }
    
    void Start() { }
    
    private void HandleSmoke(bool smoke)
    {
        if (isActive) smokeReleased = smoke;
        else isActive = false;
        
        UpdateRoomSmoke();
    }

    private void OnSmokeReleased(String header, int value)
    {
        if (isActive)
        {
            if (header == "FGM" && value == 2)
            {
                Debug.Log("Smoke Released: " + value);
                smokeAlert.SetActive(true);
                smokeReleased = true;
            } 
            else if (header == "FGM" && value == 1)
            {
                smokeAlert.SetActive(false);
                smokeReleased = false;
            }
            UpdateRoomSmoke();
        }
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
