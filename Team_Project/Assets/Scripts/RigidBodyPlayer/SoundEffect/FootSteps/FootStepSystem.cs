using RigidBodyPlayer.Modules;
using RigidBodyPlayer.PlayerData;
using UnityEngine;

public class FootStepSystem : MonoBehaviour, IPlayerModule
{
    private RigidBodyMovement hub;
    public AudioSource audioSource;
    
    public AudioClip[] metalSteps;
    public AudioClip[] plasticSteps;

    private float stepTimer;
    private float timeInAir;

    public void Initialize(RigidBodyMovement Hub)
    {
        hub = Hub;
    }

    public void Tick()
    {
        if (!hub.sensor.IsGrounded)
        {
            timeInAir += Time.deltaTime;
        }
        Vector3 horizontalVelocity = new Vector3(hub.rb.linearVelocity.x, 0, hub.rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > 0.1f && hub.sensor.IsGrounded)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0)
            {
                PlayFootStepSound();
                stepTimer = hub.data.stepInterval;
            }
        }
    }

    public void PlayFootStepSound()
    {
        AudioClip clip = null;
        switch (hub.sensor.CurrentSurface)
        {
            case SurfaceType.Metal:
                clip = metalSteps[Random.Range(0, metalSteps.Length)];
                break;
            case SurfaceType.Plastic:
                clip = plasticSteps[Random.Range(0, plasticSteps.Length)];
                break;
            default:
                break;
        }

        if (clip != null)
        {
            audioSource.PlayOneShot(clip, hub.data.volumnBoost);
        }
    }

    public void PlayLandingSound()
    {
        
        if (timeInAir > hub.data.airTimeBuffer)
        {
            PlayFootStepSound();
            timeInAir = 0;
        }
    }

    public void FixedTick()
    {
        
    }

    public void LateTick()
    {
        
    }
}
