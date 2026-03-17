using UnityEngine;

namespace Camera
{
    public class SmartSkaterCamera : MonoBehaviour
    {
        [Header("Target Tracking")]
        [SerializeField] private Transform _target;
        [SerializeField] private Rigidbody _targetRb; // Needed to track the skater's momentum

        [Header("Positioning")]
        [SerializeField] private float DistFromTarget = 4.5f;
        [SerializeField] private float HeightFromTarget = 2.0f;

        [Header("Smoothing (Lerp)")]
        [SerializeField] private float PosFollowSpeed = 5f;
        [SerializeField] private float RotFollowSpeed = 5f;

        [Header("Velocity Prediction")]
        [SerializeField] private float VelocityLookAhead = 1.0f; // Multiplier on velocity to look ahead

        private void Start()
        {
            if (_target != null && _targetRb == null)
            {
                _targetRb = _target.GetComponent<Rigidbody>();
            }

            if (_target == null)
            {
                Transform skaterObj = GameObject.Find("Skater")?.transform;
                if (skaterObj == null)
                    skaterObj = GameObject.FindObjectOfType<SkaterController>()?.transform;

                if (skaterObj != null)
                {
                    _target = skaterObj;
                    _targetRb = _target.GetComponent<Rigidbody>();
                    Debug.Log("SmartSkaterCamera: Found target automatically.");
                }
                else
                {
                    Debug.LogWarning("SmartSkaterCamera: No target found! Please assign _target in the Inspector.");
                }
            }
        }

        private void FixedUpdate()
        {
            if (_target == null) return;

            // 1) Find where the camera SHOULD be
            // Let's place it behind the player's 'forward' rotation by default
            Vector3 targetForward = _target.forward;
            
            // If they are moving fast enough, bias the 'forward' towards their actual momentum line
            if (_targetRb != null && _targetRb.linearVelocity.magnitude > 1f)
            {
                Vector3 velDir = _targetRb.linearVelocity.normalized;
                // Ignore vertical falling for camera positioning
                velDir.y = 0; 
                velDir = velDir.normalized;
                
                // Blend them, favoring actual momentum when fast
                targetForward = Vector3.Lerp(targetForward, velDir, 0.5f).normalized;
            }

            Vector3 idealPos = _target.position 
                               - (targetForward * DistFromTarget) 
                               + (Vector3.up * HeightFromTarget);

            // 2) Move camera smoothly towards ideal position
            transform.position = Vector3.Lerp(transform.position, idealPos, PosFollowSpeed * Time.fixedDeltaTime);


            // 3) Calculate where to look
            Vector3 lookAtTarget = _target.position + (Vector3.up * 1f); // Look slightly above feet
            
            // Add a look-ahead based on velocity to keep skater framed well while speeding
            if (_targetRb != null)
            {
                lookAtTarget += _targetRb.linearVelocity * (VelocityLookAhead * Time.fixedDeltaTime);
            }

            // 4) Rotate camera smoothly towards the target point
            Quaternion targetRot = Quaternion.LookRotation(lookAtTarget - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, RotFollowSpeed * Time.fixedDeltaTime);
        }
    }
}
