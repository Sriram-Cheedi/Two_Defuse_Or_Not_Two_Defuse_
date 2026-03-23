using UnityEngine;

public class WireMapping : MonoBehaviour
{
    public Puzzles.Controllers.SSM_Controller ssm_controller;
    private int finalSSMColour;
    public string cutWire;

    private void Start()
    {
        if (ssm_controller == null)
        {
            Debug.LogError("SSM Controller is not assigned.");
            return;
        }

        finalSSMColour = ssm_controller.data.correctSequence[
            ssm_controller.data.correctSequence.Length - 1
        ];

        if (finalSSMColour == 0)//yellow
            cutWire = "red";
        else if (finalSSMColour == 1) //green
            cutWire = "blue";
        else if (finalSSMColour == 2) //blue
            cutWire = "grey";
        else if (finalSSMColour == 3) //red
            cutWire = "brown";

        Debug.Log("Expected cut wire is: " + cutWire);
    }
}