using UnityEngine;

public class MoveCam : MonoBehaviour
{
    [SerializeField]private Transform cameraPostion;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = cameraPostion.rotation;
        transform.position = cameraPostion.position;
    }
}
