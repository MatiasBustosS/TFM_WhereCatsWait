using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    public enum Direction
    { Up, Down, Left, Right,  Forward, Back }

    [SerializeField] private Direction direction;
    [SerializeField] private float distance;
    [SerializeField] private float speed = 2f;
    
    
    public void Open()
    {
        StartCoroutine(MoveTo(transform.localPosition + DirectionToVector() * distance));
    }

    IEnumerator MoveTo(Vector3 target)
    {
        Vector3 start = transform.localPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            transform.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localPosition = target;
    }

    Vector3 DirectionToVector()
    {
        switch (direction)
        {
            case Direction.Up:    return Vector3.up;
            case Direction.Down:  return Vector3.down;
            case Direction.Left:  return Vector3.left;
            case Direction.Right: return Vector3.right;
            case Direction.Forward: return Vector3.forward;
            case Direction.Back: return Vector3.back;
        }
        return Vector3.zero;
    }
}
