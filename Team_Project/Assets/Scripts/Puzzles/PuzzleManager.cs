using System;
using System.Collections.Generic;
using UnityEngine;


public class PuzzleData
{
    public bool completed = false;
    
    public int[] guessSequence;
    public int[] correctSequence;
    
    public int currentSequenceIndex = 0;
    
    public Action OnSuccess;
    public Action OnFailure;
}

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }
    
    private PlayerInputRelay relay;
    
    public Dictionary<string, PuzzleData> puzzleDatas;
    
    private DigitManager NGMdigitManager; 
    
    // Shit which is getting moved
    public Renderer[] ledRenderers;
    
    [SerializeField] public Material matCorrect;
    [SerializeField] public Material matMisplaced;
    [SerializeField] public Material matDefault;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        puzzleDatas = new Dictionary<string, PuzzleData>();
        puzzleDatas["SSM"] = new PuzzleData();
        puzzleDatas["NGM"] = new PuzzleData();
        
        puzzleDatas["SSM"].correctSequence = GenerateSequence(4, 3);
        puzzleDatas["NGM"].correctSequence = GenerateSequence(4, 9);
        
        puzzleDatas["NGM"].guessSequence = new int[4];
    }
    

    private void Start()
    {
        NGMdigitManager = transform.Find("DigitManager").gameObject.GetComponent<DigitManager>();
        
        API_Provider.Instance.ActiveAPIController.SendSetSeq("NGM", puzzleDatas["NGM"].correctSequence);
        API_Provider.Instance.ActiveAPIController.SendSetSeq("SSM", puzzleDatas["SSM"].correctSequence);
        
        API_Provider.Instance.ActiveAPIController.OnNGMChanged += HandleButtonPress;
        API_Provider.Instance.ActiveAPIController.OnNGMSubmit += SetMaterialHARDWARE;

    }
    
    private void OnEnable()
    {
        relay = FindAnyObjectByType<PlayerInputRelay>();
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
    
    
    // =============  SSM FUNCTIONS ===================
    
    public void CheckSSMButtonPress(int pressedValue)
    {
        if (pressedValue ==  puzzleDatas["SSM"].correctSequence[puzzleDatas["SSM"].currentSequenceIndex])
        {
            Debug.Log("Correct!" + pressedValue);
            puzzleDatas["SSM"].currentSequenceIndex++;

            if (puzzleDatas["SSM"].currentSequenceIndex >=  puzzleDatas["SSM"].correctSequence.Length)
            {
                Debug.Log("SSM Complate" + pressedValue);
                PuzzleSolved("SSM");
            }
        }
        else
        {
            Debug.Log("Wrong button! Resetting sequence.");
            puzzleDatas["SSM"].OnFailure?.Invoke();

            ResetPuzzle("SSM");
        }
    }
    
    // =============  NGM FUNCTIONS ===================
    private void HandleButtonPress(int[] array)
    {
        if (puzzleDatas["NGM"].completed) return;
        puzzleDatas["NGM"].guessSequence = array;
        for (int i = 0; i < array.Length; i++)
        {
            Debug.Log("array values :" + array[i]);
            int updatedDigit = array[i];
            puzzleDatas["NGM"].guessSequence[i] = updatedDigit;
            NGMdigitManager.SetDigits(i, updatedDigit);
        }
        
    }
    
    private void HandleDigitInput(int digit)
    {
        if (puzzleDatas["NGM"].completed) return;
        Debug.Log("Handling Digit Input: " + digit);
        if (digit < 1 || digit > 4) return;
        int index = digit - 1;
        int updatedDigit = (++puzzleDatas["NGM"].guessSequence[index]) % 10;
        puzzleDatas["NGM"].guessSequence[index] = updatedDigit;
        NGMdigitManager.SetDigits(index, updatedDigit);
    }
    
    private void SetMaterialHARDWARE(int[] array)
    {
        puzzleDatas["NGM"].guessSequence = array;

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
        for (int i = 0; i < puzzleDatas["NGM"].correctSequence.Length; i++)
        {
            if (puzzleDatas["NGM"].correctSequence[i] == puzzleDatas["NGM"].guessSequence[i])
            {
                solutionSlotUsed[i] = true;
                guessSlotUsed[i] = true;
                SetLedMaterial(i, matCorrect);
            }
        }

        if (solutionSlotUsed[0] && solutionSlotUsed[1] && solutionSlotUsed[2] && solutionSlotUsed[3])
        {
            PuzzleSolved("NGM");
        }
    }

    private void DigitMisplaced(bool[] solutionSlotUsed, bool[] guessSlotUsed)
    {
        for (int currentIndex = 0; currentIndex < puzzleDatas["NGM"].correctSequence.Length; currentIndex++)
        {
            for (int i = 0; i < puzzleDatas["NGM"].correctSequence.Length; i++)
            {
                if (!guessSlotUsed[currentIndex])
                {
                    if (puzzleDatas["NGM"].correctSequence[i] == puzzleDatas["NGM"].guessSequence[currentIndex] && !solutionSlotUsed[i])
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
        for (int i = 0; i < puzzleDatas["NGM"].correctSequence.Length; i++)
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
    
    // ============ General Functions ================
    // When we implement multiple SSM Doors we will need to reset SSM Puzzle state and generate new sequence
    
    public int[] GenerateSequence(int length, int maxValue)
    {
        System.Random rng = new System.Random();
        int[] Seq = new int[length];
        
        for (int i = 0; i < length; i++)
        {
            Seq[i] = rng.Next(0 ,maxValue);
        }
        
        return Seq;
    }
    
    private void PuzzleSolved(string name)
    {
        puzzleDatas[name].completed = true;
        puzzleDatas[name].OnSuccess?.Invoke();
    }
    
    private void ResetPuzzle(string name)
    {
        if (name == "SSM") puzzleDatas[name].currentSequenceIndex = 0;
    }
}