using System.Collections;
using UnityEngine;
public class BarredDoorAnimation : MonoBehaviour
{
    [SerializeField] private Transform[] bars;
    // [SerializeField] private Transform doorHinge;
    // [SerializeField] private Transform door;

    [SerializeField] private float barMoveDistance = 2f;
    [SerializeField] private float barSpeed = 2f;
    [SerializeField] private float doorAngle = 90f;
    [SerializeField] private float doorSpeed = 2f;

    public void MoveBar(int index, Vector3 direction)
    {
        StartCoroutine(SlideBar(bars[index], direction));
    }

    public void OpenDoor(Transform door, Transform hinge, Vector3 axis, float direction)
    {
        StartCoroutine(RotateDoor(door, hinge, axis, direction));
    }
    IEnumerator SlideBar(Transform bar, Vector3 direction)
    {
        Vector3 startPostion = bar.localPosition;
        Vector3 targetPosition = startPostion + direction * barMoveDistance;
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