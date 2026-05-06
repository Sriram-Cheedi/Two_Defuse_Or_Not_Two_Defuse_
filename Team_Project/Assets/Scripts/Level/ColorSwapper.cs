using System;
using PersistantSystems.Software.GameStateMachine.GameStates;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorSwapper : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Image background;

    private void OnEnable()
    {
        PlayingState.ExplosionWarning += SwapColors;
    }

    private void OnDisable()
    {
        PlayingState.ExplosionWarning -= SwapColors;
    }

    public void SwapColors()
    {
        Color temp = text.color;
        text.color = background.color;
        background.color = temp;
    }
}