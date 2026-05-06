using UnityEngine;

public class SegmentDisplay : MonoBehaviour
{
    public Renderer[] segmentRenderers;
    
    public Material matOn;
    public Material matOff;
    
    private int testdigit = 1;

    private readonly int[,] code =
    {
        {1, 1, 1, 1, 1, 1, 0}, //0
        {0, 1, 1, 0, 0, 0, 0}, //1
        {1, 1, 0, 1, 1, 0, 1}, //2
        {1, 1, 1, 1, 0, 0, 1}, //3
        {0, 1, 1, 0, 0, 1, 1}, //4
        {1, 0, 1, 1, 0, 1, 1}, //5
        {1, 0, 1, 1, 1, 1, 1}, //6
        {1, 1, 1, 0, 0, 0, 0}, //7
        {1, 1, 1, 1, 1, 1, 1}, //8
        {1, 1, 1, 1, 0, 1, 1}, //9
    };

    public void SetNumber(int digit)
    {
        if(digit < 0 || digit > 9) return; 
        for (int i = 0; i < segmentRenderers.Length; i++)
        {
            segmentRenderers[i].material = code[digit, i] == 1 ?  matOn : matOff;
        }
    }
}
