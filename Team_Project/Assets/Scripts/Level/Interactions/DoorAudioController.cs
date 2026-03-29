using System;
using Puzzles.Minor_Interaction;
using UnityEngine;

public class DoorAudioController : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip doorOpeningClip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        Minor_Interactinon_Controller.DoorOpened += PlayDoorOpenAudio;
    }

    private void OnDisable()
    {
        Minor_Interactinon_Controller.DoorOpened -= PlayDoorOpenAudio;
    }

    private void PlayDoorOpenAudio()
    {
        Debug.Log("Door open audio event received");
        audioSource.PlayOneShot(doorOpeningClip);
        Minor_Interactinon_Controller.DoorOpened -= PlayDoorOpenAudio;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
