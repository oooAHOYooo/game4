using UnityEngine;
using System.Collections;

public enum TrickInputType
{
    J,
    I,
    L
}

public enum TrickType
{
    None,
    Kickflip,
    Heelflip,
    PopShoveIt,
    VarialKickflip,
    VarialHeelflip,
    Impossible
}

public class TrickSystem : MonoBehaviour
{
    [Header("Trick Rotation")]
    [SerializeField] public Transform _boardMesh; // Child object representing the skateboard
    [SerializeField] private float TrickExecutionTime = 0.35f;
    [SerializeField] private float LandingRotationTolerance = 25f; // degrees

    [Header("Combo")]
    [SerializeField] private float ComboDecayTime = 3f;

    private int _comboCount = 0;
    private float _lastLandTime = -999f;
    private TrickType _currentTrickAttempt = TrickType.None;
    private Coroutine _trickExecutionCoroutine;
    private bool _trickInputThisAir = false;

    private SkaterController _skaterController;
    private Rigidbody _rb;

    private void Start()
    {
        _skaterController = GetComponent<SkaterController>();
        _rb = GetComponent<Rigidbody>();

        if (_boardMesh == null)
            Debug.LogWarning("TrickSystem: BoardMesh not assigned! Tricks will not be visible.");
    }

    public void OnTrickInput(TrickInputType input)
    {
        if (_trickInputThisAir)
            return; // One trick per air session

        _trickInputThisAir = true;
        DetectTrickAndExecute(input);
    }

    private void DetectTrickAndExecute(TrickInputType input)
    {
        _currentTrickAttempt = DetermineTrickType(input);
        if (_currentTrickAttempt != TrickType.None)
        {
            // Stop any ongoing trick execution
            if (_trickExecutionCoroutine != null)
                StopCoroutine(_trickExecutionCoroutine);

            _trickExecutionCoroutine = StartCoroutine(ExecuteTrick(_currentTrickAttempt));
        }
    }

    private TrickType DetermineTrickType(TrickInputType input)
    {
        // This is a simple version tracking only the last input.
        // In a fuller system, you'd track multiple inputs in a buffer.
        // For now, we detect based on the input and what was previously pressed.

        return input switch
        {
            TrickInputType.J => TrickType.Kickflip,
            TrickInputType.L => TrickType.Heelflip,
            TrickInputType.I => TrickType.PopShoveIt,
            _ => TrickType.None
        };
    }

    private IEnumerator ExecuteTrick(TrickType trick)
    {
        if (_boardMesh == null)
            yield break;

        Vector3 startRotation = _boardMesh.localEulerAngles;
        Vector3 endRotation = startRotation;

        switch (trick)
        {
            case TrickType.Kickflip:
                endRotation = startRotation + Vector3.right * 360f;
                break;
            case TrickType.Heelflip:
                endRotation = startRotation + Vector3.right * -360f;
                break;
            case TrickType.PopShoveIt:
                endRotation = startRotation + Vector3.up * 180f;
                break;
            case TrickType.VarialKickflip:
                endRotation = startRotation + Vector3.right * 360f + Vector3.up * 180f;
                break;
            case TrickType.VarialHeelflip:
                endRotation = startRotation + Vector3.right * -360f + Vector3.up * 180f;
                break;
            case TrickType.Impossible:
                endRotation = startRotation + Vector3.right * 360f + Vector3.up * 360f;
                break;
        }

        float elapsed = 0f;
        while (elapsed < TrickExecutionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / TrickExecutionTime;

            // Lerp rotation
            _boardMesh.localEulerAngles = Vector3.Lerp(startRotation, endRotation, t);

            yield return null;
        }

        _boardMesh.localEulerAngles = endRotation;
    }

    public void OnLand()
    {
        float timeSinceLast = Time.time - _lastLandTime;

        // Check if trick was landed successfully
        if (_currentTrickAttempt != TrickType.None && _boardMesh != null)
        {
            float boardRotX = _boardMesh.localEulerAngles.x;
            // Normalize to 0-360, then check distance to 0 (flat)
            if (boardRotX > 180f) boardRotX -= 360f;

            if (Mathf.Abs(boardRotX) <= LandingRotationTolerance)
            {
                // Landed trick successfully
                if (timeSinceLast <= ComboDecayTime)
                {
                    _comboCount++;
                }
                else
                {
                    _comboCount = 1; // Reset combo
                }
                Debug.Log($"[COMBO] Landed {_currentTrickAttempt}! Combo: {_comboCount}");
            }
            else
            {
                // Trick failed — rotation was bad
                Debug.Log($"[SLAM] Failed to land {_currentTrickAttempt}. Combo reset.");
                _comboCount = 0;
            }
        }
        else if (timeSinceLast <= ComboDecayTime && _comboCount > 0)
        {
            // Clean landing while in combo
            _comboCount++; // or just preserve it
            Debug.Log($"[COMBO] Clean landing. Combo: {_comboCount}");
        }

        _lastLandTime = Time.time;
        _currentTrickAttempt = TrickType.None;
        _trickInputThisAir = false;
    }

    public int GetComboCount()
    {
        float timeSinceLast = Time.time - _lastLandTime;
        if (timeSinceLast > ComboDecayTime)
            return 0;
        return _comboCount;
    }
}
