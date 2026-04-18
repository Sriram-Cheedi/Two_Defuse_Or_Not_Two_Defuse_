using System;
using NUnit.Framework.Constraints;
using UnityEngine;

public class SmokeManager : MonoBehaviour
{
    [SerializeField]private PlayerInputRelay relay;
    [SerializeField]private TrackRoom trackPlayerRoom;
    [SerializeField] private GameObject smokeAlert;
    // [SerializeField] private EmojiReactionManager emojiReactionManager;
    private bool smokeReleased;
    private SmokeEmitter lastRoomActive;
    public bool isActive = false;
    private int ssmStatus;
    private int mazeStatus;

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
        // emojiReactionManager.PlaySmokeBurst();
        if (isActive)
        {
            mazeStatus = API_Provider.Instance.ActiveAPIController.GetMazeStatus();
            if (header == "FGM" && value == 2)
            {
                API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] { "\"Module Activated\"", "\"------VENT------\"" });
                smokeAlert.SetActive(true);
                smokeReleased = true;
            }
            
            else if (header == "FGM" && value == 1)
            {
                ssmStatus = API_Provider.Instance.ActiveAPIController.GetSSMStatus();
                if (ssmStatus != 2)
                {
                    API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] { "\"Module Activated\"", "\"------SSM------\"" });
                }
                else
                {
                    API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] { "\"\"", "\"\"" });

                }
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
