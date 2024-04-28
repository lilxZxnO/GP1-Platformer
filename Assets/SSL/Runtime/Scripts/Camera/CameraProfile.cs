using UnityEngine;

public enum CameraProfileType
    {
        Static = 0,
        FollowTarget
    }

public class CameraProfile : MonoBehaviour
{
    [Header("type")]
    [SerializeField] private CameraProfileType _profiltype = CameraProfileType.Static;
    [Header("Follow")]
    [SerializeField] private CameraFollowable _targetToFollow = null;
    private Camera _camera;
    public float CameraSize => _camera.orthographicSize;
    public Vector3 Position => _camera.transform.position;
    [Header("Damping")]
    [SerializeField] private bool _useDampingHorizontally = false;
    [SerializeField] private float _horizontalDampingFactor = 5f;
    [SerializeField] private bool _useDampingVertically = false;
    [SerializeField] private float _verticalDampingFactor = 5f;
    [Header("Bounds")]
    [SerializeField] private bool _hasBounds = false;
    [SerializeField] private Rect _boundsRect = new Rect(0f, 0f, 10f, 10f);
    public Rect BoundsRect => _boundsRect;
    public bool HasBounds => _hasBounds;

    
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera != null)
            _camera.enabled = false;
    }
    public CameraProfileType ProfileType => _profiltype;
    public CameraFollowable TargetToFollow => _targetToFollow;
    public CameraFollowable TargetFollowable => _targetToFollow;
    public bool UseDampingHorizontally => _useDampingHorizontally;
    public float HorizontalDampingFactor => _horizontalDampingFactor;
    public bool UseDampingVertically => _useDampingVertically;
    public float VerticalDampingFactor => _verticalDampingFactor;
    
    private void OnDrawGuizmosSelected()
    {
        if (!_hasBounds) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_boundsRect.center, _boundsRect.size);
    }
}

