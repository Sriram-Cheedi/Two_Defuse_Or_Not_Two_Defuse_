// using UnityEngine;
//
// public class LedManager : MonoBehaviour
// {
//     [SerializeField] private DigitManager digitManager;
//     
//     public Renderer[] ledRenderers;
//     private PlayerInputRelay relay;
//     
//     [SerializeField] private Material matCorrect;
//     [SerializeField] private Material matMisplaced;
//     [SerializeField] private Material matDefault;
//     
//     private void OnEnable()
//     {
//         relay = FindAnyObjectByType<PlayerInputRelay>();
//         if (relay != null)
//         {
//             relay.NumberPressed += HandleDigitInput;
//             relay.GuessSubmitted += SetMaterial;
//         }
//     }
//     
//     private void OnDisable()
//     {
//         if (relay != null)
//         {
//             relay.NumberPressed -= HandleDigitInput;
//             relay.GuessSubmitted -= SetMaterial;
//         }
//     }
//
//     private void HandleDigitInput(int digit)
//     {
//         if (digit < 1 || digit > 4) return;
//         int index = digit - 1;
//         int updatedDigit = (++guess[index]) % 10;
//         guess[index] = updatedDigit;
//         digitManager.SetDigits(index, updatedDigit);
//     }
//
//     private void DigitCorrect(bool[] solutionSlotUsed, bool[] guessSlotUsed)
//     {
//         for (int i = 0; i < solution.Length; i++)
//         {
//             if (solution[i] == guess[i])
//             {
//                 solutionSlotUsed[i] = true;
//                 guessSlotUsed[i] = true;
//                 SetLedMaterial(i, matCorrect);
//             }
//         }
//         isSolved = solutionSlotUsed[0] && solutionSlotUsed[1] && solutionSlotUsed[2] && solutionSlotUsed[3];
//     }
//
//     private void DigitMisplaced(bool[] solutionSlotUsed, bool[] guessSlotUsed)
//     {
//         for (int currentIndex = 0; currentIndex < solution.Length; currentIndex++)
//         {
//             for (int i = 0; i < solution.Length; i++)
//             {
//                 if (!guessSlotUsed[currentIndex])
//                 {
//                     if (solution[i] == guess[currentIndex] && !solutionSlotUsed[i])
//                     {
//                         SetLedMaterial(currentIndex, matMisplaced);
//                         solutionSlotUsed[i] = true;
//                         guessSlotUsed[currentIndex] = true;
//                         break;
//                     }
//                 }
//             }
//         }
//     }
//
//     private void DigitWrong(bool[] guessSlotUsed)
//     {
//         for (int i = 0; i < solution.Length; i++)
//         {
//             if (!guessSlotUsed[i])
//             {
//                 SetLedMaterial(i, matDefault);
//             }
//         }
//     }
//
//     private void SetMaterial()
//     {
//         bool[] solutionSlotUsed = new bool[4];
//         bool[] guessSlotUsed = new bool[4];
//         DigitCorrect(solutionSlotUsed, guessSlotUsed);
//         DigitMisplaced(solutionSlotUsed, guessSlotUsed);
//         DigitWrong(guessSlotUsed);
//     }
//     
//     public void SetLedMaterial(int index, Material mat)
//     {
//         if (index < 0 || index >= ledRenderers.Length) return;
//         ledRenderers[index].material = mat;
//     }
// }
