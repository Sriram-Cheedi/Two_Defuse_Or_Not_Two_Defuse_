using UnityEngine;
using TMPro;

public class ScoreTile : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameTextObject;
    [SerializeField] private TextMeshProUGUI scoreTextComponent;

    public void SetData(string newName, string newScore)
    {
        if (scoreTextComponent != null) scoreTextComponent.text = newScore;
        if (nameTextObject != null) nameTextObject.text = newName;
    }
}