using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HighscoreManager : MonoBehaviour
{
    public TMP_Text[] topScores;
    public DatabaseManager dbManager;
    
    // Start is called before the first frame update
    void Start()
    {
        dbManager = FindObjectOfType<DatabaseManager>();
        canvasText.text = "Hi, it's me!";
    }
}