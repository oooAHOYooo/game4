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
    [SerializeField] private float GroundRaycastDistance = 1.15f; // Capsule half-height is 1.0, plus 0.15 for bumps
    [SerializeField] private float GroundRaycastOffset = 0.0f;

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
    private bool _powerslideHeld;

    // Removed Auto-Push variables in favor of constant W force

    // Ollie charging
    private float _ollieChargeStart;

    // Landing state
    private float _landingStateStart;

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
        // === INPUT CONTROLS ===
        // WASD: Steering / Movement
        float moveX = (Input.GetKey(KeyCode.D) ? 1 : 0) + (Input.GetKey(KeyCode.A) ? -1 : 0);
        float moveZ = (Input.GetKey(KeyCode.W) ? 1 : 0) + (Input.GetKey(KeyCode.S) ? -1 : 0);
        _moveInput = new Vector2(moveX, moveZ).normalized;

        // SPACE: Ollie (Jump) - Hold to charge for more power
        if (Input.GetKeyDown(KeyCode.Space))
            _olliePressed = true;
        if (Input.GetKeyUp(KeyCode.Space))
            _ollieReleased = true;
        _ollieHeld = Input.GetKey(KeyCode.Space);

        // Powerslide (D was requested, also adding S and Shift just in case D was meant for 'Drift' and they wanted typical steering)
        _powerslideHeld = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftShift);

        // J: Kickflip (Airborne)
        if (_state == State.Grounded)
        {
            // Removed manual J pushing as requested
        }

        // Trick inputs (only when airborne)
        if (_state == State.Airborne && _trickSystem != null)
        {
            if (Input.GetKeyDown(KeyCode.J))
                _trickSystem.OnTrickInput(TrickInputType.J);  // Kickflip
            if (Input.GetKeyDown(KeyCode.I))
                _trickSystem.OnTrickInput(TrickInputType.I);  // Pop ShoveIt
            if (Input.GetKeyDown(KeyCode.L))
                _trickSystem.OnTrickInput(TrickInputType.L);  // Heelflip
        }

        // Clear triggers after FixedUpdate might have missed them if we strictly rely on bools,
        // but it's better to reset them here if they've been handled.
        // Actually, since FixedUpdate runs on its own timer, let's keep boolean flags until FixedUpdate consumes them.
    }

    private void ConsumeInputs()
    {
        _pushPressed = false;
        _olliePressed = false;
        _ollieReleased = false;
    }

    private void FixedUpdate()
    {
        // Out-of-bounds check
        if (transform.position.y < -30f)
        {
            Debug.Log("Fell off the world! Respawning...");
            transform.position = new Vector3(0, 5, 0);
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _state = State.Airborne;
        }

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
                    _olliePressed = false; // Consume
                }
                else if (!isGrounded)
                {
                    _state = State.Airborne;
                }
                break;

            case State.OllieCharge:
                HandleGroundedPhysics();

                if (_ollieReleased)
                {
                    PerformOlliePopup();
                    _state = State.Airborne;
                    _ollieReleased = false; // Consume
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
        // We let Update reset these appropriately or process them safely
        // Clear triggers
        // _pushPressed = false;
        // _olliePressed = false;
        // _ollieReleased = false;
    }

    private bool CheckGrounded()
    {
        Vector3 rayStart = transform.position + Vector3.up * GroundRaycastOffset;
        // Use a small SphereCast instead of a thin Raycast to be more forgiving with slopes/edges
        if (Physics.SphereCast(rayStart, 0.2f, Vector3.down, out RaycastHit hit, GroundRaycastDistance, GroundLayer))
        {
            return true;
        }
        return false;
    }

    private void HandleGroundedPhysics()
    {
        // Reduce damping drastically when the player is actively trying to accelerate
        float actualDrag = GroundDrag;
        if (_moveInput.y > 0 || _pushPressed)
            actualDrag = 0.5f;

        _rb.linearDamping = _state == State.OllieCharge ? actualDrag * 2.5f : actualDrag;

        float actualTurnSpeed = TurnSpeed > 10f ? TurnSpeed : 150f;

        // Steering with A/D
        if (Mathf.Abs(_moveInput.x) > 0)
        {
            float turnAmount = _moveInput.x * actualTurnSpeed * Time.fixedDeltaTime;
            // Use transform.Rotate to bypass potential Rigidbody FreezeRotation constraints
            transform.Rotate(0, turnAmount, 0, Space.World);

            // Redirect velocity so we follow the new forward instead of sliding sideways
            Vector3 vel = _rb.linearVelocity;
            float horizontalSpeed = new Vector3(vel.x, 0, vel.z).magnitude;
            
            // Preserve vertical momentum (gravity), modify horizontal to match new rotation
            _rb.linearVelocity = new Vector3(transform.forward.x * horizontalSpeed, vel.y, transform.forward.z * horizontalSpeed);
        }

        // Accelerate / Brake with W/S
        if (_moveInput.y != 0)
        {
            float currentSpeed = _rb.linearVelocity.magnitude;
            // Accelerate (Constant force on W)
            if (_moveInput.y > 0 && currentSpeed < MaxSpeed)
            {
                // Multiply force heavily by rb.mass so it moves regardless of weight
                // Removed the impulse feeling in favor of a constant strong push
                _rb.AddForce(transform.forward * PushForce * 15f * _rb.mass, ForceMode.Force);
            }
            // Brake (S key)
            else if (_moveInput.y < 0)
            {
                // Brake effectively resists the current forward velocity
                Vector3 currentForwardVel = Vector3.Project(_rb.linearVelocity, transform.forward);
                if (currentForwardVel.magnitude > 0.1f)
                {
                    _rb.AddForce(-transform.forward * PushForce * 20f * _rb.mass, ForceMode.Force);
                }
                else
                {
                     // Small backward crawl
                    _rb.AddForce(-transform.forward * PushForce * 5f * _rb.mass, ForceMode.Force);
                }
            }
        }

        // Powerslide Logic
        if (_powerslideHeld && _rb.linearVelocity.magnitude > 2f)
        {
            // Drastically increase drag
            _rb.linearDamping = GroundDrag * 3f;
            // Visually turn the board sideways by sliding the velocity direction
            Vector3 vel = _rb.linearVelocity;
            transform.forward = Vector3.Lerp(transform.forward, Vector3.Cross(Vector3.up, vel.normalized), Time.fixedDeltaTime * 10f);
        }

        // Removed original J push mechanics
        
        // Debug
        if (_moveInput.sqrMagnitude > 0 || _pushPressed)
        {
            // Debug.Log($"Moving... Velocity: {_rb.linearVelocity.magnitude:F2} / Damping: {_rb.linearDamping:F2}");
        }
    }

    private void HandleAirbornePhysics()
    {
        _rb.linearDamping = AirDrag;
        
        float actualTurnSpeed = TurnSpeed > 10f ? TurnSpeed : 150f;

        // Mid-air rotation (A/D)
        if (Mathf.Abs(_moveInput.x) > 0)
        {
            float turnAmount = _moveInput.x * actualTurnSpeed * Time.fixedDeltaTime;
            transform.Rotate(0, turnAmount, 0, Space.World);
        }

        // Weak mid-air steering influence (W/A/S/D)
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
