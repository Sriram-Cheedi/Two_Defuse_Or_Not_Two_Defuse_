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
        SSMSeq = new int[] { 5, 2, 6, 1, 2, 6 };
        SSMKeypad.InitializeKeypadCode(SSMSeq);
        return SSMSeq;
    }

    public void Update()
    {

    }

    public void Start() { }
}