using System.Collections;
using UnityEngine;

public class BarredDoorAnimation : MonoBehaviour
{
    [SerializeField] private Transform[] bars;
    [SerializeField] private Transform doorHinge;
    [SerializeField] private Transform door;

    [SerializeField] private float barMoveDistance = 2f;
    [SerializeField] private float barSpeed = 2f;

    [SerializeField] private float doorAngle = 90f;
    [SerializeField] private float doorSpeed = 2f;

    public void MoveBar(int index, Vector3 direction)
    {
        StartCoroutine(SlideBar(bars[index], direction));
    }

    public void OpenDoor()
    {
        StartCoroutine(RotateDoor());
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

    IEnumerator RotateDoor()
    {
        float rotated = 0;
        while (rotated < 90f)
        {
            float step = 100f * Time.deltaTime;
            door.RotateAround(doorHinge.position, Vector3.up, step);
            rotated += step;
            yield return null;
        }
    }
}