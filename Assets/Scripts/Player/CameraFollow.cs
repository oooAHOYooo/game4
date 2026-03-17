using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;
    public Vector3 Offset = new Vector3(0, 1.5f, -3f);
    public float SmoothSpeed = 10f;

    private void LateUpdate()
    {
        if (Target == null) return;
        
        Vector3 desiredPosition = Target.position + Target.TransformDirection(Offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * SmoothSpeed);
        transform.LookAt(Target.position + Vector3.up * 1f);
    }
}
