using System;
using System.Collections;
using PersistantSystems.Software.GameStateMachine.GameStates;
using UnityEngine;
using UnityEngine.UI;

public class ActivationTip : MonoBehaviour
{
   [SerializeField] private GameObject[] activationTips;
   [SerializeField] private GameObject[] onBoardingTips;
   [SerializeField] private PlayerInputRelay relay;
   private float holdDuration = 1.0f;
   [SerializeField] private float fadeInDuration = 0.3f;
   private bool hasActivated = false;
   
   private void Start()
   {
      ShowActivationTips();
   }

   private void OnEnable()  => ReadyUpState.BombActivated += HideActivationTips;
   private void OnDisable() => ReadyUpState.BombActivated -= HideActivationTips;

   private IEnumerator RunSequence()
   {
      foreach (var tip in onBoardingTips)
         yield return ShowAndHold(tip);
         


      GameEvents.TriggerBoardingDone();
   }

   private IEnumerator ShowAndHold(GameObject hud)
   {
      HUDCardAnimator animator = hud.GetComponent<HUDCardAnimator>();
      var ringTransform = hud.transform.Find("Ring_Card");
      var ring = ringTransform.GetComponent<Image>();
      ring.fillAmount = 0f;

      if (animator != null)
      {
         animator.PlayFadeIn();
         yield return new WaitForSeconds(fadeInDuration);
      }
      else
      {
         hud.SetActive(true);
      }
      

      float held = 0f;
      while (held < holdDuration)
      {
         held = relay.IsEnterHeld ? held + Time.unscaledDeltaTime : 0f;
         ring.fillAmount = held / holdDuration;
         yield return null;
      }
      ring.fillAmount = 0f;

      if (animator != null)
      {
         animator.PlayFadeOut();
      }
      else
      {
         hud.SetActive(false);
      }
   }

   private void ShowActivationTips()
   {
      foreach (var tip in activationTips) tip.SetActive(true);
   }

   private void HideActivationTips()
   {
      if (hasActivated) return;
      foreach (var tip in activationTips) tip.SetActive(false);
      StartCoroutine(RunSequence());
      hasActivated = true;
   }
}
