using System.Collections;
using UnityEngine;
public class BarredDoorAnimation : MonoBehaviour
{
    [SerializeField] private Transform[] bars;
    // [SerializeField] private Transform doorHinge;
    // [SerializeField] private Transform door;
    private Vector3[] originalPositions;
    [SerializeField] private float barMoveDistance = 2f;
    [SerializeField] private float barSpeed = 2f;
    [SerializeField] private float doorAngle = 90f;
    [SerializeField] private float doorSpeed = 2f;

    
    void Start()
    {
        originalPositions = new Vector3[bars.Length];

        for (int i = 0; i < bars.Length; i++)
        {
            originalPositions[i] = bars[i].localPosition;
        }
    }
    public void MoveBar(int index, int state)
    {
        StartCoroutine(SlideBar(index, state));
    }

    public void OpenDoor(Transform door, Transform hinge, Vector3 axis, float direction)
    {
        StartCoroutine(RotateDoor(door, hinge, axis, direction));
    }
    IEnumerator SlideBar(int index, int state)
    {
        Transform bar = bars[index];
        Vector3 startPostion = bar.localPosition;
        Vector3 direction;
        if (index == 1)
        {
            direction = Vector3.forward;
        }
        else
        {
            direction = Vector3.back;
        }

        Vector3 targetPosition;
        if (state == 0)
        {
            targetPosition = originalPositions[index] + direction * barMoveDistance;
        }
        else
        {
            targetPosition = originalPositions[index];
        }
        float step = 0;
        while (step < 1)
        {
            step += Time.deltaTime * barSpeed;
            bar.localPosition = Vector3.Lerp(startPostion, targetPosition, step);
            yield return null;
        }
    }

    IEnumerator RotateDoor(Transform door, Transform hinge, Vector3 axis, float direction)
    {
        float rotated = 0;
        while (rotated < 90f)
        {
            float step = 100f * Time.deltaTime * direction;
            door.RotateAround(hinge.position, axis, step);
            rotated += step;
            yield return null;
        }
    }
}