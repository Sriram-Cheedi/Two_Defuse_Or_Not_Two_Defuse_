using System;
using UnityEngine;

public class wireCut : MonoBehaviour
{
    public WireMapping wireMapping;
    public GameManager GameManager;
    public bool hasCut = false;
    
    private void Update()
    {
        if (VerletWire.CutWire == null || hasCut)
        {
            return;
        }
        if (VerletWire.CutWire.wireColor.ToString().ToLower() == wireMapping.cutWire)
        {
            GameManager.EndGame();
        }
        else
        {
            GameManager.WinGame();
        }
        VerletWire.CutWire = null;
        hasCut = false;
    }
}
