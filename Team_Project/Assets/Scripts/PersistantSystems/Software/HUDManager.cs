using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [SerializeField] private PlayerInputRelay relay;
    [SerializeField] private GameObject showHudPrompt;

    private readonly List<GameObject> currentActiveHUDs = new List<GameObject>();
    private bool hudsHidden;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        if (relay != null)
            relay.HudsPressed += OnHudsPressed;
    }

    private void OnDisable()
    {
        if (relay != null)
            relay.HudsPressed -= OnHudsPressed;
    }

    private void OnHudsPressed()
    {
        if (currentActiveHUDs.Count == 0)
            return;

        hudsHidden = !hudsHidden;
        UpdateHUDVisuals();
    }

    public void ActivateTrigger(GameObject[] huds)
    {
        if (huds == null || huds.Length == 0 || SameHUDs(huds)) return;
        // Clear current HUDs when entering a new trigger
        foreach (var hud in currentActiveHUDs)
        {
            if (hud != null)
                hud.SetActive(false);
        }

        currentActiveHUDs.Clear();


        foreach (var hud in huds)
        {
            if (hud == null) continue;
            currentActiveHUDs.Add(hud);
        }


        // Reset to visible when entering a new trigger
        hudsHidden = false;
        UpdateHUDVisuals();
    }

    private bool SameHUDs(GameObject[] huds)
    {
        if (huds.Length != currentActiveHUDs.Count)
        {
            return false;
        }

        for (int i = 0; i < huds.Length; i++)
        {
            if (huds[i] != currentActiveHUDs[i])
                return false;
        }

        return true;
    }

private void UpdateHUDVisuals()
    {
        bool hasHUDs = currentActiveHUDs.Count > 0;

        if (showHudPrompt != null)
            showHudPrompt.SetActive(hasHUDs && hudsHidden);

        foreach (var hud in currentActiveHUDs)
        {
            if (hud != null)
                hud.SetActive(!hudsHidden);
        }
    }
}