using System;
using UnityEngine;

public class Activtion_Tip : MonoBehaviour
{
   public GameObject[] activationTips;
   [SerializeField] private LightingManager lightingManager;
   private bool bombActive;

   private void Awake()
   {
      if(activationTips == null) return;
      HandleActivateKeyTip(false);
   }

   private void OnEnable()
   {
      lightingManager.isBombActivated += ActivateBomb;
   }

   private void OnDisable()
   {
      lightingManager.isBombActivated -= ActivateBomb;
   }

   private void ActivateBomb()
   {
      if(activationTips == null) return;

      HandleActivateKeyTip(true);
      lightingManager.isBombActivated -= ActivateBomb;
   }
   
   private void HandleActivateKeyTip(bool bombActive)
   {
      if (bombActive)
      {
         foreach (var tip in activationTips)
         {
            tip.SetActive(false);
         }
      }
      else
      {
         foreach (var tip in activationTips)
         {
            tip.SetActive(true);
         }
      }
      
   }
}
