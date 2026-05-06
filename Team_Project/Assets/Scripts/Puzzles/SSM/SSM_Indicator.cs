using System;
using UnityEngine;

public class SSM_Indicator : MonoBehaviour
{
    public Material materialOn;
    public Material materialOff;
    [SerializeField] private MeshRenderer[] meshRenderers;
    
    private void Awake()
    {
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material = materialOff;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material = materialOn;
            }
        }
    }
}
