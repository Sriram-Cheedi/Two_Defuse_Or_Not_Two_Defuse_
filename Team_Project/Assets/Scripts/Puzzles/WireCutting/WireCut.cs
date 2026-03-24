using UnityEngine;

public class WireCut : MonoBehaviour
{
    public WireMapping wireMapping;
    public bool hasCut = false;

    private void Update()
    {
        if (VerletWire.CutWireColor == null || hasCut)
            return;

        if (wireMapping == null)
        {
            Debug.LogError("WireMapping is null.");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null.");
            return;
        }

        string cutColor = VerletWire.CutWireColor.Value.ToString().ToLower();

        Debug.Log("Cut wire: " + cutColor);
        Debug.Log("Expected wire: " + wireMapping.cutWire);

        if (cutColor != wireMapping.cutWire)
        {
            Debug.Log("Wrong wire cut -> EndGame");
            GameManager.Instance.EndGame();
        }
        else
        {
            Debug.Log("Correct wire cut -> WinGame");
            GameManager.Instance.WinGame();
        }

        hasCut = true;
        VerletWire.CutWireColor = null;
    }
}