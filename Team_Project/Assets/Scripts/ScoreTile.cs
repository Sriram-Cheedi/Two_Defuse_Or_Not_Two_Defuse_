using UnityEngine;
using TMPro;

public class ScoreTile : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameTextObject;
    [SerializeField] private TextMeshProUGUI scoreTextComponent;

    public void SetData(string newName, int newScore)
    {
        if (scoreTextComponent != null) scoreTextComponent.text = newScore.ToString();
        if (nameTextObject != null) nameTextObject.text = newName;
    }
}