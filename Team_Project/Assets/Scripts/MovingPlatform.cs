using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] float speed = 10f;
    [SerializeField] float delay = 0.5f;
    [SerializeField] private Transform platform;
    private Vector3 targetPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        platform.position = pointA.position;
        targetPosition = pointB.position;
        StartCoroutine(Moveplatform());
    }

    // Update is called once per frame

    IEnumerator Moveplatform()
    {
        while (true)
        {
            while (Vector3.Distance(targetPosition, platform.position) > 0.01f)
            {
                platform.position = Vector3.MoveTowards(platform.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }
            targetPosition = targetPosition == pointA.position ? pointB.position :  pointA.position;
            yield return new WaitForSeconds(delay);
        }
    }
}
