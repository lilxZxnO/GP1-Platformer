using UnityEngine;
using UnityEngine.Serialization;

public class HeroEntity : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody2D _rigidbody;

    [Header("Horizontal Movements")]
    [FormerlySerializedAs("_mouvementsettings")]
    [SerializeField] private HeroHorizontalMouvementSettings _groundHorizontalMouvementSettings;
    [SerializeField] private HeroHorizontalMouvementSettings _airHorizontalMouvementSettings;
    [SerializeField] HeroHorizontalMouvementSettings _mouvementsettings;
    [Header("Dash")]
    [SerializeField] DashSettings _dashSettings;
    private float _dashStartTime = 0f;
    private float _horizontalSpeed = 0f;
    private float _moveDirX = 0f;

    [Header("Orientation")]
    [SerializeField] private Transform _orientVisualRoot;
    private float _orientX = 1f;

    [Header("Vertical Mouvements")]
    private float _verticalSpeed = 0f;
    [Header("Fall")]
    [SerializeField] private HeroFallSettings _fallSettings;

    [Header("Ground")]
    [SerializeField] private GrounDetector _groundDetector;
    public bool IsTouchingGround { get; private set; } = false;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;
    
    [Header("Jump")]
    [SerializeField] private HeroJumpSettings _jumpSettings;
    [SerializeField] private HeroFallSettings _jumpFallSettings;
    enum JumpState
    {
        NotJumping,
        JumpImpulsion,
        Falling,
    }
    private JumpState _jumpState = JumpState.NotJumping;
    private float _jumpTimer = 0f;
    private CameraFollowable _cameraFollowable;

    #region Functions Move Dir
    public float GetMoveDirX()
    {
        return _moveDirX;
    }
    public void SetMoveDirX(float dirX)
    {
        _moveDirX = dirX;
    }
    #endregion
    public void JumpStart()
    {
        _jumpState = JumpState.JumpImpulsion;
        _jumpTimer = 0f;
    }
    public bool IsJumping => _jumpState != JumpState.NotJumping;
    private void _UpdateJumpingStateImpulsion()
    {
        _jumpTimer += Time.fixedDeltaTime;
        if (_jumpTimer < _jumpSettings.jumpMaxDuration)
        {
            _verticalSpeed = _jumpSettings.jumpSpeed;
        }
        else
        {
            _jumpState = JumpState.Falling;
        }
    }
    private void _UpdateJumpingStateFalling()
    {
        if (!IsTouchingGround) {
            _ApplyFallGravity(_jumpFallSettings);
        } else {
            _ResetVerticalSpeed();
            _jumpState = JumpState.NotJumping;
        }
    }
    private void _UpdateJump()
    {
        switch (_jumpState) {
            case JumpState.JumpImpulsion:
                _UpdateJumpingStateImpulsion();
                break;
            case JumpState.Falling:
                _UpdateJumpingStateFalling();
                break;
        }
    }
    public void StopJumpImpulsion()
    {
        _jumpState = JumpState.Falling;
    }
    public bool IsJumpingImpulsing => _jumpState == JumpState.JumpImpulsion;
    public bool IsJumpMinDurationReached => _jumpTimer >= _jumpSettings.jumpMinDuration;
    public void Dash()
{
    if (Input.GetKeyDown(KeyCode.E))
    {
        if (IsTouchingGround && _moveDirX != 0f)
        {
            float dashDirection = Mathf.Sign(_moveDirX);
            _horizontalSpeed = _dashSettings.dashSpeed * dashDirection;
            _dashStartTime = Time.time;
            _rigidbody.gravityScale = 0f;
        }
    }
}

    
    private void Awake()
    {
        _cameraFollowable = GetComponent<CameraFollowable>();
        _cameraFollowable.FollowPositionX = _rigidbody.position.x;
        _cameraFollowable.FollowPositionY = _rigidbody.position.y;
    }
    private void FixedUpdate()
{
    _ApplyGrounDetection();
    _UpdateCameraFollowablePosition();
    HeroHorizontalMouvementSettings horizontalMouvementSettings = _GetCurrentHorizontalMouvementSettings();
    
    if (_AreOrientAndMouvementOpposite())
    {
        _TurnBack(horizontalMouvementSettings);
    }
    else
    {
        _UpdateHorizontalSpeed(horizontalMouvementSettings);
        _changeOrientFromHorizontalMouvement();
    }
    
    if (IsJumping)
    {
        _UpdateJump();
    }
    else
    {
        if (!IsTouchingGround)
        {
            _ApplyFallGravity(_fallSettings);
        }
        else
        {
            _ResetVerticalSpeed();
        }
    }
    
    if (_dashStartTime > 0f && Time.time - _dashStartTime >= _dashSettings.dashDuration)
    {
        _rigidbody.gravityScale = 1f; 
        _dashStartTime = 0f; 
    }
    
    _ApplyHorizontalSpeed();
    _ApplyVerticalSpeed();
}

    private void _changeOrientFromHorizontalMouvement()
    {
        if (_moveDirX == 0f) return;
        _orientX = Mathf.Sign(_moveDirX);
    }

    private void _ApplyHorizontalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.x = _horizontalSpeed * _orientX;
        _rigidbody.velocity = velocity;
    }
    
    private void Update()
    {
        _UpdateOrientVisual();
    }

    private void _UpdateOrientVisual()
    {
        Vector3 newScale = _orientVisualRoot.localScale;
        newScale.x = _orientX;
        _orientVisualRoot.localScale = newScale;
    }

    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label($"MoveDirX = {_moveDirX}");
        GUILayout.Label($"OrientX = {_orientX}");
        if (IsTouchingGround)
        {
            GUILayout.Label("OnGround");
        }
        else
        {
            GUILayout.Label("InAir");
        }
        GUILayout.Label($"IsJumping = {_jumpState}");
        GUILayout.Label($"Horizontal Speed = {_horizontalSpeed}");
        GUILayout.Label($"Vertical Speed = {_verticalSpeed}");
        GUILayout.EndVertical();
    }
    private void _Accelerate(HeroHorizontalMouvementSettings settings)
    {
        _horizontalSpeed += settings.acceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed > settings.speedMax)
        {
            _horizontalSpeed = settings.speedMax;
        }
    }
    private void _Decelerate(HeroHorizontalMouvementSettings settings)
    {
        _horizontalSpeed -= settings.deceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed < 0f)
        {
            _horizontalSpeed = 0f;
        }
    }
    private void _UpdateHorizontalSpeed(HeroHorizontalMouvementSettings settings)
    {
        if (_moveDirX != 0f)
        {
            _Accelerate(settings);
        }
        else
        {
            _Decelerate(settings);
    }
    }
    private void _TurnBack(HeroHorizontalMouvementSettings settings)
    {
        _horizontalSpeed -= settings.turnBackFrictions * Time.fixedDeltaTime;
        if (_horizontalSpeed < 0f)
        {
            _horizontalSpeed = 0f;
            _changeOrientFromHorizontalMouvement();
        }
    }
    private bool _AreOrientAndMouvementOpposite()
    {
        return _moveDirX * _orientX < 0f;
    }
    private void _ApplyFallGravity(HeroFallSettings settings)
    {
        _verticalSpeed -= settings.fallGravity * Time.fixedDeltaTime;
        if (_verticalSpeed < -settings.fallSpeedMax)
        {
            _verticalSpeed = -settings.fallSpeedMax;
        }
    }
    private void _ApplyVerticalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.y = _verticalSpeed;
        _rigidbody.velocity = velocity;
    }
    private void _ApplyGrounDetection()
    {
        IsTouchingGround = _groundDetector.DetectGroundNearBy();
    }
    private void _ResetVerticalSpeed()
    {
        _verticalSpeed = 0f;
    }
    private HeroHorizontalMouvementSettings _GetCurrentHorizontalMouvementSettings()
    {
        if (IsTouchingGround)
        {
            return _groundHorizontalMouvementSettings;
        }
        else
        {
            return _airHorizontalMouvementSettings;
        }
    }
    private void _UpdateCameraFollowablePosition()
    {
        _cameraFollowable.FollowPositionX = _rigidbody.position.x;
        if (IsTouchingGround && !IsJumping)
        {
            _cameraFollowable.FollowPositionY = _rigidbody.position.y;
        }
    }
}
