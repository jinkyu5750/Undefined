using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Rigidbody + CapsuleCollider 기반 1인칭 이동(걷기/점프/앉기). FPS 샘플 PlayerCharacterController/PlayerInputHandler에 의존하지 않음.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class RigidbodyFpsMotor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform cameraPivot;
    [SerializeField] Camera playerCamera;

    [Header("Look")]
    [SerializeField] float mouseSensitivityHorizontal = 0.12f;
    [SerializeField] float mouseSensitivityVertical = 0.12f;
    [SerializeField] float pitchClamp = 88f;

    [Header("Move")]
    [Tooltip("체크 시 카메라가 바라보는 방향 기준 WASD(default FPS). 해제하면 이 오브젝트 transform.forward 기준.")]
    [SerializeField] bool moveRelativeToCamera = true;

    [Tooltip("입력 X/Y 의미 바꿔야 앞으로 갈 때 (액션맵 레이아웃이 다를 때).")]
    [SerializeField] bool swapMoveAxes;

    [Tooltip("Player/Move가 세로값을 거의 안 줄 때 WASD 폴백으로 보강. 키보드 앞이 안 먹을 때 켜둠.")]
    [SerializeField] bool augmentMoveWithKeyboard = true;

    [SerializeField]
    [Tooltip("보통 거리/sec. 높게 (8~12) 픽업형, 낮게 (3~5) 탐험·조심 플레이.")]
    float walkSpeed = 6f;

    [SerializeField] float sprintSpeed = 10f;

    [Tooltip("지면 좌표가속 단위/sec (대략 80~140 = 탄탄한 FPS 반응, 35~ 이하면 헤비).")]
    [SerializeField] float groundAccel = 90f;

    [Tooltip("공중에서 방향 바꿀 때 가속.")]
    [SerializeField] float airAccel = 30f;

    [Tooltip("지면 입력 뗐을 때 감속(높을수록 바로 서는 느낌).")]
    [SerializeField] float groundFriction = 65f;

    [Header("Jump")]
    [SerializeField] float jumpVelocity = 6.5f;
    [SerializeField] float coyoteTime = 0.12f;
    [SerializeField] float jumpBufferTime = 0.12f;

    [Header("Crouch")]
    [SerializeField] float crouchHeight = 1f;
    [SerializeField] float crouchCameraDrop = 0.55f;
    [SerializeField] float stanceLerpSpeed = 12f;
    [SerializeField] float crouchMoveSpeedMultiplier = 0.55f;
    [Tooltip("켜두면 Sprint 중에는 앉은 채 속도 적용 안 함 등과 같이 무시 가능")]
    [SerializeField] bool allowSprintWhileCrouched;

    [Header("Ground")]
    [SerializeField] LayerMask groundLayers = ~0;
    [SerializeField] float groundProbeDistance = 0.35f;
    [SerializeField] float groundSlopeDotMin = 0.55f;

    const string MoveId = "Player/Move";
    const string LookId = "Player/Look";
    const string JumpId = "Player/Jump";
    const string SprintId = "Player/Sprint";
    const string CrouchId = "Player/Crouch";

    Rigidbody _rb;
    CapsuleCollider _capsule;

    InputAction _moveAction;
    InputAction _lookAction;
    InputAction _jumpAction;
    InputAction _sprintAction;
    InputAction _crouchAction;

    float _standingHeight;
    Vector3 _standingCenter;
    float _standingCamLocalY;
    bool _crouchingToggle;
    float _targetCapsuleHeight;
    Vector3 _targetCapsuleCenter;
    float _targetCamLocalY;

    float _yawDegrees;
    float _pitchDegrees;

    Vector2 _moveInput;

    float _jumpPressedTime = -999f;

    bool _wasGroundedLastFixed;
    float _leftGroundFixedTime = -1000f;

    Vector2 ReadFallbackMove()
    {
        var kb = Keyboard.current;
        if (kb == null)
            return Vector2.zero;

        float x = 0f;
        float y = 0f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;

        var v = new Vector2(x, y);
        return v.sqrMagnitude > 1f ? v.normalized : v;
    }

    Vector2 ReadFallbackLook()
    {
        var mouse = Mouse.current;
        return mouse != null ? mouse.delta.ReadValue() : Vector2.zero;
    }

    bool ReadFallbackJumpDown()
    {
        var kb = Keyboard.current;
        return kb != null && kb.spaceKey.wasPressedThisFrame;
    }

    bool ReadFallbackSprintHeld()
    {
        var kb = Keyboard.current;
        return kb != null && kb.leftShiftKey.isPressed;
    }

    bool ReadFallbackCrouchToggle()
    {
        var kb = Keyboard.current;
        return kb != null && kb.ctrlKey.wasPressedThisFrame;
    }

    bool CanProcessGameplayInput()
    {
        return Cursor.lockState == CursorLockMode.Locked;
    }

    void CacheInputActions()
    {
        if (InputSystem.actions == null)
            return;

        _moveAction = InputSystem.actions.FindAction(MoveId);
        _lookAction = InputSystem.actions.FindAction(LookId);
        _jumpAction = InputSystem.actions.FindAction(JumpId);
        _sprintAction = InputSystem.actions.FindAction(SprintId);
        _crouchAction = InputSystem.actions.FindAction(CrouchId);

        EnableIfValid(_moveAction);
        EnableIfValid(_lookAction);
        EnableIfValid(_jumpAction);
        EnableIfValid(_sprintAction);
        EnableIfValid(_crouchAction);
    }

    static void EnableIfValid(InputAction action)
    {
        if (action != null && !action.enabled)
            action.Enable();
    }

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _capsule = GetComponent<CapsuleCollider>();

        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _rb.useGravity = true;

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (cameraPivot == null && playerCamera != null)
            cameraPivot = playerCamera.transform.parent != null ? playerCamera.transform.parent : playerCamera.transform;

        _standingHeight = Mathf.Max(_capsule.height, 0.1f);
        _standingCenter = _capsule.center;
        _targetCapsuleHeight = _standingHeight;
        _targetCapsuleCenter = _standingCenter;

        if (cameraPivot != null)
        {
            _standingCamLocalY = cameraPivot.localPosition.y;
            _targetCamLocalY = _standingCamLocalY;
        }

        CacheInputActions();

        Transform orientation = cameraPivot != null ? cameraPivot : playerCamera != null ? playerCamera.transform : null;
        if (orientation != null)
            _yawDegrees = orientation.eulerAngles.y;
    }

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        PollMoveInput();

        bool jumpPressed = false;
        if (_jumpAction != null)
            jumpPressed |= _jumpAction.WasPressedThisFrame();
        if (Application.isFocused && Keyboard.current != null)
            jumpPressed |= ReadFallbackJumpDown();

        if (jumpPressed)
            _jumpPressedTime = Time.time;

        PollLookInput(Time.deltaTime);
        PollCrouchInput();
        BlendStanceVisuals(Time.deltaTime);
        LockCursorShortcuts();
    }

    void PollMoveInput()
    {
        _moveInput = Vector2.zero;
        if (!CanProcessGameplayInput())
            return;

        if (_moveAction != null)
        {
            Vector2 v = _moveAction.ReadValue<Vector2>();
            if (swapMoveAxes)
                v = new Vector2(v.y, v.x);

            if (augmentMoveWithKeyboard)
            {
                Vector2 kb = ReadFallbackMove();
                if (Mathf.Abs(v.x) < 0.01f && Mathf.Abs(kb.x) > 0.01f)
                    v.x = kb.x;
                if (Mathf.Abs(v.y) < 0.01f && Mathf.Abs(kb.y) > 0.01f)
                    v.y = kb.y;
            }

            _moveInput = v.sqrMagnitude > 1f ? v.normalized : v;
        }
        else
            _moveInput = ReadFallbackMove();
    }

    /// <summary>XZ 평면 기준 바라보는 방향 벡터(길이 1).</summary>
    Vector3 GetPlanarWishDirection()
    {
        float x = _moveInput.x;
        float z = _moveInput.y;

        if (Mathf.Abs(x) + Mathf.Abs(z) < 0.0001f)
            return Vector3.zero;

        if (moveRelativeToCamera)
        {
            Camera cam = playerCamera != null ? playerCamera : Camera.main;

            Transform basis = cam != null ? cam.transform : transform;
            Vector3 f = Vector3.ProjectOnPlane(basis.forward, Vector3.up).normalized;
            Vector3 r = Vector3.ProjectOnPlane(basis.right, Vector3.up).normalized;
            Vector3 dir = r * x + f * z;
            if (dir.sqrMagnitude < 0.0001f)
                return Vector3.zero;
            return dir.normalized;
        }

        Vector3 world = transform.TransformDirection(new Vector3(x, 0f, z));
        world.y = 0f;
        if (world.sqrMagnitude < 0.0001f)
            return Vector3.zero;
        return world.normalized;
    }

    void PollLookInput(float dt)
    {
        if (!CanProcessGameplayInput())
            return;

        Vector2 look;

        if (_lookAction != null)
            look = _lookAction.ReadValue<Vector2>();
        else
            look = ReadFallbackLook();

        _yawDegrees += look.x * mouseSensitivityHorizontal;
        _pitchDegrees -= look.y * mouseSensitivityVertical;
        _pitchDegrees = Mathf.Clamp(_pitchDegrees, -pitchClamp, pitchClamp);

        transform.rotation = Quaternion.Euler(0f, _yawDegrees, 0f);

        Transform pivot = cameraPivot != null
            ? cameraPivot
            : playerCamera != null
                ? playerCamera.transform
                : null;

        if (pivot != null && pivot != transform)
            pivot.localEulerAngles = new Vector3(_pitchDegrees, 0f, 0f);
        else if (playerCamera != null)
        {
            playerCamera.transform.rotation = transform.rotation * Quaternion.Euler(_pitchDegrees, 0f, 0f);
        }
    }

    void PollCrouchInput()
    {
        if (!CanProcessGameplayInput())
            return;

        bool toggle = false;

        if (_crouchAction != null)
            toggle |= _crouchAction.WasPressedThisFrame();

        toggle |= ReadFallbackCrouchToggle();

        if (toggle)
        {
            bool next = !_crouchingToggle;

            // 일어날 때 머리 위 막히면 무시 (토글 상태 유지)
            if (_crouchingToggle && !next && !CapsuleVolumeClear(_standingHeight, _standingCenter))
                next = true;

            _crouchingToggle = next;
        }
    }

    bool CapsuleVolumeClear(float height, Vector3 centerLocal)
    {
        float radius = Mathf.Max(0.05f, _capsule.radius * 0.98f);
        float halfMinusR = Mathf.Max(0f, height * 0.5f - radius);

        Vector3 centerWorld = transform.TransformPoint(centerLocal);
        Vector3 bottomPt = centerWorld + transform.TransformDirection(Vector3.down * halfMinusR);
        Vector3 topPt = centerWorld + transform.TransformDirection(Vector3.up * halfMinusR);

        var hits =
            Physics.OverlapCapsule(bottomPt, topPt, radius, groundLayers, QueryTriggerInteraction.Ignore);

        foreach (Collider c in hits)
        {
            if (c == null || c.isTrigger) continue;
            if (c == _capsule) continue;
            if (c.attachedRigidbody == _rb) continue;
            if (c.transform == transform || c.transform.IsChildOf(transform)) continue;

            return false;
        }

        return true;
    }

    void BlendStanceVisuals(float dt)
    {
        float minH = Mathf.Max(crouchHeight, _capsule.radius * 2f + 0.05f);
        float targetH = _crouchingToggle ? minH : _standingHeight;
        _targetCapsuleHeight = targetH;
        _targetCapsuleCenter = new Vector3(_standingCenter.x, targetH * 0.5f, _standingCenter.z);

        if (cameraPivot != null)
        {
            float drop = _crouchingToggle ? crouchCameraDrop : 0f;
            _targetCamLocalY = _standingCamLocalY - drop;
        }

        float t = 1f - Mathf.Exp(-stanceLerpSpeed * dt);
        _capsule.height = Mathf.Lerp(_capsule.height, _targetCapsuleHeight, t);
        _capsule.center = Vector3.Lerp(_capsule.center, _targetCapsuleCenter, t);

        if (cameraPivot != null)
        {
            Vector3 lp = cameraPivot.localPosition;
            lp.y = Mathf.Lerp(lp.y, _targetCamLocalY, t);
            cameraPivot.localPosition = lp;
        }
    }

    void LockCursorShortcuts()
    {
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    bool HasSprintHeld()
    {
        if (_sprintAction != null)
            return _sprintAction.IsPressed();
        return ReadFallbackSprintHeld();
    }

    bool IsCoyoteGrounded(bool groundedProbe)
    {
        if (groundedProbe) return true;
        return Time.fixedTime - _leftGroundFixedTime <= coyoteTime;
    }

    bool IsJumpBuffered()
    {
        return Time.time - _jumpPressedTime <= jumpBufferTime;
    }

    Vector3 FeetWorldBottom()
    {
        Vector3 worldCenter = transform.TransformPoint(_capsule.center);
        float halfMinusR = Mathf.Max(0f, _capsule.height * 0.5f - _capsule.radius);
        return worldCenter + transform.TransformDirection(Vector3.down * halfMinusR);
    }

    bool ProbeGround(out RaycastHit hit)
    {
        hit = default;
        float r = Mathf.Max(0.04f, _capsule.radius * 0.95f);
        Vector3 origin = FeetWorldBottom() + Vector3.up * (r + 0.02f);

        if (Physics.SphereCast(origin, r, Vector3.down, out hit, groundProbeDistance + r, groundLayers,
                QueryTriggerInteraction.Ignore))
        {
            return Vector3.Dot(hit.normal, Vector3.up) >= groundSlopeDotMin;
        }

        return false;
    }

    void FixedUpdate()
    {
        bool groundedProbe = ProbeGround(out _);

        if (_wasGroundedLastFixed && !groundedProbe)
            _leftGroundFixedTime = Time.fixedTime;

        _wasGroundedLastFixed = groundedProbe;
        Vector3 planarVel = Vector3.ProjectOnPlane(_rb.linearVelocity, Vector3.up);

        Vector3 wish = GetPlanarWishDirection();

        bool sprintHeld = HasSprintHeld();
        float baseSpeed =
            !_crouchingToggle || allowSprintWhileCrouched
                ? sprintHeld
                    ? sprintSpeed
                    : walkSpeed
                : walkSpeed * crouchMoveSpeedMultiplier;

        if (_crouchingToggle && sprintHeld && !allowSprintWhileCrouched)
            baseSpeed = walkSpeed * crouchMoveSpeedMultiplier;

        Vector3 targetHorizontal = wish * baseSpeed;

        float accel = groundedProbe ? groundAccel : airAccel;

        planarVel =
            AccelerateToward(planarVel, targetHorizontal, accel * Time.fixedDeltaTime);

        if (groundedProbe && wish.sqrMagnitude < 0.001f && planarVel.sqrMagnitude < 2f && _rb.linearVelocity.y <= 0.1f)
            planarVel = Vector3.MoveTowards(planarVel, Vector3.zero, groundFriction * Time.fixedDeltaTime);

        Vector3 v = planarVel;

        Vector3 vv = _rb.linearVelocity;

        float vy = Mathf.Max(vv.y, -40f);

        if (IsCoyoteGrounded(groundedProbe) && IsJumpBuffered())
        {
            vy = jumpVelocity;
            _jumpPressedTime = -999f;
        }

        if (_crouchingToggle && vy < 0f && groundedProbe)
            vy = Mathf.Min(vy, -0.05f);

        _rb.linearVelocity = new Vector3(v.x, vy, v.z);
    }

    static Vector3 AccelerateToward(Vector3 current, Vector3 desired, float maxDeltaUnits)
    {
        Vector3 delta = desired - current;
        float mag = delta.magnitude;
        if (mag < Mathf.Epsilon)
            return current;
        float move = Mathf.Min(maxDeltaUnits, mag);
        return current + delta.normalized * move;
    }
}
