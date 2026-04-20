using UnityEngine;

public class DeathEmojiTrigger : MonoBehaviour
{
    [SerializeField] private EmojiReactionManager emojiReactionManager;

    private void OnEnable()
    {
        SpawnManager.RespawnRequested += OnRespawnRequested;
    }

    private void OnDisable()
    {
        SpawnManager.RespawnRequested -= OnRespawnRequested;
    }

    private void OnRespawnRequested(RespawnData data)
    {
        emojiReactionManager.PlayDeathBurst();
        
    }
}