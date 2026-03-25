using System;
using UnityEngine;

public class ActivationTip : MonoBehaviour
{
   public GameObject[] activationTips;
   private bool bombActive;
   public static Action IsBombActive;

   private void Awake()
   {
      if(activationTips == null) return;
   }

   private void OnEnable()
   {
      // lightingManager.isBombActivated += ActivateBomb;
      GameManager.onGameStateChanged += ActivateBomb;
   }

   private void OnDisable()
   {
      GameManager.onGameStateChanged -= ActivateBomb;
   }

   private void ActivateBomb(GameManager.GameState gameState)
   {
      if(activationTips == null) return;
      if (gameState == GameManager.GameState.ReadyUP)
      {
         HandleActivateKeyTip(false);
      }else if (gameState == GameManager.GameState.Playing)
      {
         HandleActivateKeyTip(true);
         IsBombActive?.Invoke();
         GameManager.onGameStateChanged -= ActivateBomb;
      }
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
