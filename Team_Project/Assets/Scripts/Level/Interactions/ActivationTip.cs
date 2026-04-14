using System;
using PersistantSystems.Software.GameStateMachine.GameStates;
using UnityEngine;

public class ActivationTip : MonoBehaviour
{
   public GameObject[] activationTips;

   private void Awake()
   {
      if(activationTips == null) return;
      foreach (var tip in activationTips)
      {
         tip.SetActive(true);
      }
   }

   private void OnEnable()
   {
      ReadyUpState.BombActivated += hideCards;
   }

   private void OnDisable()
   {
      ReadyUpState.BombActivated -= hideCards;
   }

   private void hideCards()
   {
      if(activationTips == null) return;
      foreach (var tip in activationTips)
      {
         tip.SetActive(false);
      }
   }
}
