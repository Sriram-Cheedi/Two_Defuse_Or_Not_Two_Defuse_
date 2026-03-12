using UnityEngine;

namespace Puzzles.Controllers
{
    public class NGM_Controller : MonoBehaviour, IPuzzle 
    {
        [SerializeField] public PlayerInputRelay relay;
        
        public PuzzleData data = new PuzzleData 
        { 
            guessSequence = new int[4], 
            correctSequence = GenerateSequence(4, 9)
        };
        
        public PuzzleData getData => data;
        
        public Renderer[] ledRenderers;
        public SegmentDisplay[] ledDisplays;
    
        [SerializeField] public Material matCorrect;
        [SerializeField] public Material matMisplaced;
        [SerializeField] public Material matDefault;
        
        private void Start()
        {
            API_Provider.Instance.ActiveAPIController.OnNGMChanged += HandleButtonPress;
            API_Provider.Instance.ActiveAPIController.OnNGMSubmit += SetMaterialHARDWARE;
        }
        
        private void OnEnable()
        {
            if (relay != null)
            {
                relay.NumberPressed += HandleDigitInput;
                relay.GuessSubmitted += SetMaterial;
            }
        }
    
        private void OnDisable()
        {
            if (relay != null)
            {
                relay.NumberPressed -= HandleDigitInput;
                relay.GuessSubmitted -= SetMaterial;
            }
        
            API_Provider.Instance.ActiveAPIController.OnNGMChanged -= HandleButtonPress;
            API_Provider.Instance.ActiveAPIController.OnNGMSubmit -= SetMaterialHARDWARE;
        }
        
         private void HandleButtonPress(int[] array)
    {
        if (data.completed) return;
        data.guessSequence = array;
        for (int i = 0; i < array.Length; i++)
        {
            Debug.Log("array values :" + array[i]);
            int updatedDigit = array[i];
            data.guessSequence[i] = updatedDigit;
        }
    }
    
    private void HandleDigitInput(int digit)
    {
        if (data.completed) return;
        // Debug.Log("Handling Digit Input: " + digit);
        if (digit < 1 || digit > 4) return;
        int index = digit - 1;
        int updatedDigit = (++data.guessSequence[index]) % 10;
        data.guessSequence[index] = updatedDigit;
        for (int i = 0; i < data.guessSequence.Length; i++)
        {
            Debug.Log(data.guessSequence[i]);
        }
    }
    
    private void SetMaterialHARDWARE(int[] array)
    {
        data.guessSequence = array;
        Debug.Log("Guess Submitted... Setting Material");
        bool[] solutionSlotUsed = new bool[4];
        bool[] guessSlotUsed = new bool[4];
        DigitCorrect(solutionSlotUsed, guessSlotUsed);
        DigitMisplaced(solutionSlotUsed, guessSlotUsed);
        DigitWrong(guessSlotUsed);
    }
    
    
    private void SetMaterial()
    {
        Debug.Log("Guess Submitted... Setting Material");
        bool[] solutionSlotUsed = new bool[4];
        bool[] guessSlotUsed = new bool[4];
        DigitCorrect(solutionSlotUsed, guessSlotUsed);
        DigitMisplaced(solutionSlotUsed, guessSlotUsed);
        DigitWrong(guessSlotUsed);
    }
    
    private void DigitCorrect(bool[] solutionSlotUsed, bool[] guessSlotUsed)
    {
        for (int i = 0; i < data.correctSequence.Length; i++)
        {
            if (data.correctSequence[i] == data.guessSequence[i])
            {
                solutionSlotUsed[i] = true;
                guessSlotUsed[i] = true;
                SetLedMaterial(i, matCorrect);
            }
        }

        if (solutionSlotUsed[0] && solutionSlotUsed[1] && solutionSlotUsed[2] && solutionSlotUsed[3])
        {
            data.completed = true;
            data.OnSuccess?.Invoke();
        }
    }

    private void DigitMisplaced(bool[] solutionSlotUsed, bool[] guessSlotUsed)
    {
        for (int currentIndex = 0; currentIndex < data.correctSequence.Length; currentIndex++)
        {
            for (int i = 0; i < data.correctSequence.Length; i++)
            {
                if (!guessSlotUsed[currentIndex])
                {
                    if (data.correctSequence[i] == data.guessSequence[currentIndex] && !solutionSlotUsed[i])
                    {
                        SetLedMaterial(currentIndex, matMisplaced);
                        solutionSlotUsed[i] = true;
                        guessSlotUsed[currentIndex] = true;
                        break;
                    }
                }
            }
        }
    }

    private void DigitWrong(bool[] guessSlotUsed)
    {
        for (int i = 0; i < data.correctSequence.Length; i++)
        {
            if (!guessSlotUsed[i])
            {
                SetLedMaterial(i, matDefault);
            }
        }
    }
    
    public void SetLedMaterial(int index, Material mat)
    {
        if (index < 0 || index >= ledRenderers.Length) return;
        ledRenderers[index].material = mat;
    }
    
    void Update()
    {
        for(int i =  0; i < data.guessSequence.Length; i++)
        {
            ledDisplays[i].SetNumber(data.guessSequence[i]);
        }
    }
    
    
    public static int[] GenerateSequence(int length, int maxValue)
    {
        System.Random rng = new System.Random();
        int[] Seq = new int[length];
        
        for (int i = 0; i < length; i++)
        {
            Seq[i] = rng.Next(0 ,maxValue);
        }
        
        return Seq;
    }
    
    }
}