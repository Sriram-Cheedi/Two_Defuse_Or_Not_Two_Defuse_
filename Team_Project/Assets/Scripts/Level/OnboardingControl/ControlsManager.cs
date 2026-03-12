using System.Collections;
using UnityEngine;

public class ControlsManager : MonoBehaviour
{
    [Header("Cards")]
    public GameObject WASDcard;
    public GameObject crouchCard;
    public GameObject mouseCard;
    public GameObject jumpCard;
    public GameObject DifficultyCard;
    public GameObject jumpRestrictionCard;
    public GameObject gravityCard;

    [Header("Timing")]
    public float delay = 1.5f;
    public float duration = 4f;

    private void Start()
    {
        HideCards();
        // StartCoroutine(MovementAndCrouchingCards());
    }

    IEnumerator MovementAndCrouchingCards()
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(ShowForSeconds(new GameObject[]{WASDcard, crouchCard}));
    }

    public void MouseInstructionCard()
    {
        StartCoroutine(ShowForSeconds(new GameObject[]{mouseCard}));
    }

    public void JumpInstructionCard()
    {
        StartCoroutine(ShowForSeconds(new GameObject[]{jumpCard}));
    }

    IEnumerator ShowForSeconds(GameObject[] cards)
    {
        HideCards();
        foreach (GameObject card in cards)
        { 
            card.SetActive(true);
        }
        yield return new WaitForSeconds(duration);
        foreach (GameObject card in cards)
        {
            card.SetActive(false);
        }
    }
    
    void HideCards()
    {
        if (WASDcard)
        {
            WASDcard.SetActive(false);
        }

        if (crouchCard)
        {
            crouchCard.SetActive(false);
        }

        if (mouseCard)
        {
            mouseCard.SetActive(false);
        }

        if (jumpCard)
        {
            jumpCard.SetActive(false);
        }

        if (DifficultyCard)
        {
            DifficultyCard.SetActive(false);
        }

        if (jumpRestrictionCard)
        {
            jumpRestrictionCard.SetActive(false);
        }

        if (gravityCard)
        {
            gravityCard.SetActive(false);
        }
    }
}