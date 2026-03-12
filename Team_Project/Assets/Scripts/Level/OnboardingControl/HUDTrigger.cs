using System;
using System.Collections.Generic;
using UnityEngine;

public class HUDTrigger : MonoBehaviour
{
    public GameObject[] myHUDs;
    private static List<GameObject> currentActiveHUDs = new List<GameObject>();

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
        }
    }
}
