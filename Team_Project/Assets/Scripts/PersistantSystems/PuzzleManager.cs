using System;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    public NavKeypad.Keypad SSMKeypad;

    int[] SSMSeq = new int[6];

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
    }

    public int[] GenerateSSMSequence()
    {
        SSMSeq = new int[] { 0, 1, 2, 0, 2, 3 };

        if (SSMKeypad != null)
        {
            SSMKeypad.InitializeKeypadCode(SSMSeq);
        }
        else
        {
            Debug.LogWarning("PuzzleManager: No SSMKeypad assigned! Sequence generated but not sent to hardware.");
        }

        return SSMSeq;
    }

    public void Update()
    {

    }

    public void Start() { }
}