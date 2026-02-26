using UnityEngine;

public class DigitManager : MonoBehaviour
{
    public SegmentDisplay[] displays;
    public int[] guess;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        guess = new int[4] {0,0,0,0};
    }

    public int[] GetDigits()
    {
        return guess;
    }

    public void SetDigits(int index, int value)
    {
        guess[index] = value;
    }

    // Update is called once per frame
    void Update()
    {
        for(int i =  0; i < guess.Length; i++)
        {
            displays[i].SetNumber(guess[i]);
        }
    }
}
