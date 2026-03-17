using System;
using System.Collections.Generic;
using UnityEngine;

public class HUDTrigger : MonoBehaviour
{
    public GameObject[] myHUDs;
    private static List<GameObject> currentActiveHUDs = new List<GameObject>();
    [SerializeField] private GameObject showHud;
    [SerializeField] private PlayerInputRelay relay;
    private static bool hideHuds;
    private static GameObject globalShowHud;

    private void Awake()
    {
        if (globalShowHud == null && showHud != null)
        {
            globalShowHud = showHud;
        }
    }

    private void OnEnable()
    {
        if (relay == null) return;
        relay.HudsPressed += OnHudsPressed;
    }

    private void OnDisable()
    {
        if (relay == null) return;
        relay.HudsPressed -= OnHudsPressed;
    }

    private void OnHudsPressed()
    {
        hideHuds = !hideHuds;
        UpdateHUDVisuals();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentActiveHUDs != null && currentActiveHUDs.Count > 0)
            {
                foreach (var currentActiveHUD in currentActiveHUDs)
                {
                    currentActiveHUD.SetActive(false);
                }
                currentActiveHUDs.Clear();
            }

            if (myHUDs.Length > 0)
            {
                foreach (var myHud in myHUDs)
                {
                    myHud.SetActive(true);
                    currentActiveHUDs.Add(myHud);
                }
            }
            hideHuds = false;
            UpdateHUDVisuals();
        }
    }

    private void UpdateHUDVisuals()
    {
        if (relay == null) return;
        if (showHud == null) return;
        if (currentActiveHUDs.Count <= 0) return;
        Debug.Log("Switching HUDs" + hideHuds);
        showHud.SetActive(hideHuds);
        foreach (var currentActiveHUD in currentActiveHUDs)
        {
            currentActiveHUD.SetActive(!hideHuds);
        }
    }
}
