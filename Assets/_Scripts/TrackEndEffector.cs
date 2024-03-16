using System.Collections;
using UnityEngine;

public class TrackEndEffector : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private Transform endEffectorRepresentation;

    public void CenterEndEffector()
    {
        Vector3 camForward = transform.forward;
        Vector3 eePosition = endEffectorRepresentation.position;
        float dist = Vector3.Distance(transform.position, eePosition);
        Vector3 targetPoint = eePosition + -camForward * dist;
        StartCoroutine(MoveCoroutine(targetPoint));
    }
    
    IEnumerator MoveCoroutine(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(transform.position, targetPosition);

        while (distance > 0.001f)
        {
            Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.position = newPosition;
            distance = Vector3.Distance(transform.position, targetPosition);
            yield return null;
        }
    }
}
