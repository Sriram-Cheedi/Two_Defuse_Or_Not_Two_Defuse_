using UnityEngine;

public class MoveCam : MonoBehaviour
{
    [SerializeField]private Transform cameraPostion;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (cameraPostion == null)
        {
            Debug.LogError($"No cameraPostion found on {gameObject.name}" + "please assign them in the Inspector.", this);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = cameraPostion.rotation;
        transform.position = cameraPostion.position;
    }
}
