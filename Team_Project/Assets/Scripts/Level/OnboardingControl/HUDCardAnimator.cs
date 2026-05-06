using UnityEngine;

public class HUDCardAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void PlayFadeIn()
    {
        gameObject.SetActive(true);

        if (animator != null)
            animator.Play("FadeIn", 0, 0f);
    }

    public void PlayFadeOut()
    {
        if (animator != null)
            animator.Play("FadeOut", 0, 0f);
        else
            gameObject.SetActive(false);
    }

    public void DisableObject()
    {
        gameObject.SetActive(false);
    }
}