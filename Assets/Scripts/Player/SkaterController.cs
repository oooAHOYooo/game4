using UnityEngine;

public class SkaterController : MonoBehaviour
{
    public enum State
    {
        Grounded,
        OllieCharge,
        Airborne,
        Landing
    }

    [Header("Physics Tuning")]
    [SerializeField] private float GroundDrag = 3f;
    [SerializeField] private float AirDrag = 0.1f;
    [SerializeField] private float PushForce = 10f;
    [SerializeField] private float MaxSpeed = 16f;
    [SerializeField] private float TurnSpeed = 90f; // deg/s

    [Header("Ollie Tuning")]
    [SerializeField] private float MinOllieForce = 6f;
    [SerializeField] private float MaxOllieForce = 13f;
    [SerializeField] private float MaxChargeTime = 0.7f;

    [Header("Ground Detection")]
    [SerializeField] public LayerMask GroundLayer;
    [SerializeField] private float GroundRaycastDistance = 0.35f;
    [SerializeField] private float GroundRaycastOffset = 0.1f;

    [Header("Landing")]
    [SerializeField] private float LandingStateDuration = 0.2f;

    private Rigidbody _rb;
    private TrickSystem _trickSystem;

    private State _state = State.Grounded;
    public State CurrentState => _state;

    // Input state
    private Vector2 _moveInput;
    private bool _pushPressed;
    private bool _olliePressed;
    private bool _ollieReleased;
    private bool _ollieHeld;

    // Ollie charging
    private float _ollieChargeStart;

    // Landing state
    private float _landingStateStart;

    // Tracking if trick was attempted this air session
    private bool _trickAttemptedThisAir;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _trickSystem = GetComponent<TrickSystem>();

        if (_rb == null)
            Debug.LogError("SkaterController requires a Rigidbody component!");
        if (_trickSystem == null)
            Debug.LogWarning("SkaterController recommends a TrickSystem component!");
    }

    private void Update()
    {
        // Reset frame-trigger booleans
        _pushPressed = false;
        _olliePressed = false;
        _ollieReleased = false;

        // Poll input using legacy Input system
        // Movement
        float moveX = (Input.GetKey(KeyCode.D) ? 1 : 0) + (Input.GetKey(KeyCode.A) ? -1 : 0);
        float moveZ = (Input.GetKey(KeyCode.W) ? 1 : 0) + (Input.GetKey(KeyCode.S) ? -1 : 0);
        _moveInput = new Vector2(moveX, moveZ).normalized;

        // Push (only when grounded)
        if (_state == State.Grounded && Input.GetKeyDown(KeyCode.J))
            _pushPressed = true;

        // Ollie
        if (Input.GetKeyDown(KeyCode.Space))
            _olliePressed = true;
        if (Input.GetKeyUp(KeyCode.Space))
            _ollieReleased = true;
        _ollieHeld = Input.GetKey(KeyCode.Space);

        // Trick inputs (only when airborne)
        if (_state == State.Airborne)
        {
            if (Input.GetKeyDown(KeyCode.J))
                _trickSystem.OnTrickInput(TrickInputType.J);
            if (Input.GetKeyDown(KeyCode.I))
                _trickSystem.OnTrickInput(TrickInputType.I);
            if (Input.GetKeyDown(KeyCode.L))
                _trickSystem.OnTrickInput(TrickInputType.L);
        }
    }

    private void FixedUpdate()
    {
        bool isGrounded = CheckGrounded();

        // State transitions
        switch (_state)
        {
            case State.Grounded:
                HandleGroundedPhysics();

                if (_olliePressed)
                {
                    _state = State.OllieCharge;
                    _ollieChargeStart = Time.time;
                }
                else if (!isGrounded)
                {
                    _state = State.Airborne;
                    _trickAttemptedThisAir = false;
                }
                break;

            case State.OllieCharge:
                HandleGroundedPhysics();

                if (_ollieReleased)
                {
                    PerformOlliePopup();
                    _state = State.Airborne;
                    _trickAttemptedThisAir = false;
                }
                break;

            case State.Airborne:
                HandleAirbornePhysics();

                if (isGrounded)
                {
                    _trickSystem.OnLand();
                    _state = State.Landing;
                    _landingStateStart = Time.time;
                }
                break;

            case State.Landing:
                HandleGroundedPhysics();

                if (Time.time - _landingStateStart >= LandingStateDuration)
                {
                    _state = State.Grounded;
                }
                break;
        }
    }

    private bool CheckGrounded()
    {
        Vector3 rayStart = transform.position + Vector3.up * GroundRaycastOffset;
        if (Physics.Raycast(rayStart, Vector3.down, GroundRaycastDistance, GroundLayer))
        {
            return true;
        }
        return false;
    }

    private void HandleGroundedPhysics()
    {
        _rb.linearDamping = _state == State.OllieCharge ? GroundDrag * 2.5f : GroundDrag;

        // Steering with WASD
        if (_moveInput.sqrMagnitude > 0)
        {
            Vector3 desiredDirection = (transform.right * _moveInput.x + transform.forward * _moveInput.y).normalized;
            float targetAngle = Mathf.Atan2(desiredDirection.x, desiredDirection.z) * Mathf.Rad2Deg;
            float currentAngle = Mathf.Atan2(transform.forward.x, transform.forward.z) * Mathf.Rad2Deg;
            float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);
            float clampedAngle = Mathf.Clamp(angleDiff, -TurnSpeed * Time.fixedDeltaTime, TurnSpeed * Time.fixedDeltaTime);

            transform.Rotate(0, clampedAngle, 0);
        }

        // Push forward
        if (_pushPressed)
        {
            float currentSpeed = _rb.linearVelocity.magnitude;
            if (currentSpeed < MaxSpeed)
            {
                _rb.AddForce(transform.forward * PushForce, ForceMode.Impulse);
            }
        }
    }

    private void HandleAirbornePhysics()
    {
        _rb.linearDamping = AirDrag;

        // Weak mid-air steering influence
        if (_moveInput.sqrMagnitude > 0)
        {
            Vector3 influence = (transform.right * _moveInput.x + transform.forward * _moveInput.y).normalized * 5f;
            _rb.AddForce(influence, ForceMode.Force);
        }
    }

    private void PerformOlliePopup()
    {
        float chargeTime = Time.time - _ollieChargeStart;
        float t = Mathf.Clamp01(chargeTime / MaxChargeTime);
        float force = Mathf.Lerp(MinOllieForce, MaxOllieForce, t);

        float currentSpeed = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z).magnitude;
        Vector3 popForce = Vector3.up * force + transform.forward * (currentSpeed * 0.3f);
        _rb.AddForce(popForce, ForceMode.Impulse);
    }

    public float GetCurrentSpeed()
    {
        return new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z).magnitude;
    }

    public bool IsAirborne()
    {
        return _state == State.Airborne;
    }
}
