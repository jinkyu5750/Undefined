using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    // Rigidbody 기반으로 교체 (이름/공개 API는 기존 스크립트와 호환 유지)
    //  [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(PlayerInputHandler), typeof(AudioSource))]
    public class PlayerCharacterController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the main camera used for the player")]
        public Camera PlayerCamera;

        [Tooltip("Audio source for footsteps, jump, etc...")]
        public AudioSource AudioSource;

        [Header("General")]
        [Tooltip("Force applied downward when in the air")]
        public float GravityDownForce = 20f;

        [Tooltip("Physic layers checked to consider the player grounded")]
        public LayerMask GroundCheckLayers = -1;

        [Tooltip("distance from the bottom of the capsule to test for grounded")]
        public float GroundCheckDistance = 0.05f;

        [Header("Movement")]
        [Tooltip("Max movement speed when grounded (when not sprinting)")]
        public float MaxSpeedOnGround = 10f;

        [Tooltip("Sharpness for the movement when grounded")]
        public float MovementSharpnessOnGround = 15;

        [Tooltip("Max movement speed when crouching")]
        [Range(0, 1)]
        public float MaxSpeedCrouchedRatio = 0.5f;

        [Tooltip("Max movement speed when not grounded")]
        public float MaxSpeedInAir = 10f;

        [Tooltip("Acceleration speed when in the air")]
        public float AccelerationSpeedInAir = 25f;

        [Tooltip("Multiplicator for the sprint speed (based on grounded speed)")]
        public float SprintSpeedModifier = 2f;

        [Tooltip("Height at which the player dies instantly when falling off the map")]
        public float KillHeight = -50f;

        [Header("Rotation")]
        [Tooltip("Rotation speed for moving the camera")]
        public float RotationSpeed = 200f;

        [Range(0.1f, 1f)]
        [Tooltip("Rotation speed multiplier when aiming")]
        public float AimingRotationMultiplier = 0.4f;

        [Header("Jump")]
        [Tooltip("Upward speed applied when jumping")]
        public float JumpForce = 9f;

        [Header("Stance")]
        [Tooltip("Ratio (0-1) of the character height where the camera will be at")]
        public float CameraHeightRatio = 0.9f;

        [Tooltip("Height of character when standing")]
        public float CapsuleHeightStanding = 1.8f;

        [Tooltip("Height of character when crouching")]
        public float CapsuleHeightCrouching = 0.9f;

        [Tooltip("Speed of crouching transitions")]
        public float CrouchingSharpness = 10f;

        [Header("Audio")]
        [Tooltip("Amount of footstep sounds played when moving one meter")]
        public float FootstepSfxFrequency = 1f;

        [Tooltip("Amount of footstep sounds played when moving one meter while sprinting")]
        public float FootstepSfxFrequencyWhileSprinting = 1f;

        [Tooltip("Sound played for footsteps")]
        public AudioClip FootstepSfx;

        [Tooltip("Sound played when jumping")] public AudioClip JumpSfx;
        [Tooltip("Sound played when landing")] public AudioClip LandSfx;

        [Tooltip("Sound played when taking damage froma fall")]
        public AudioClip FallDamageSfx;

        [Header("Fall Damage")]
        public bool RecievesFallDamage;
        public float MinSpeedForFallDamage = 10f;
        public float MaxSpeedForFallDamage = 30f;
        public float FallDamageAtMinSpeed = 10f;
        public float FallDamageAtMaxSpeed = 50f;

        public UnityAction<bool> OnStanceChanged;

        // 외부(예: Jetpack)에서 이 값을 읽고/쓰기 때문에 Rigidbody.velocity와 동기화
        public Vector3 CharacterVelocity
        {
            get => m_Rigidbody ? m_Rigidbody.linearVelocity : Vector3.zero;
            set
            {
                if (m_Rigidbody)
                    m_Rigidbody.linearVelocity = value;
            }
        }

        public bool IsGrounded { get; private set; }
        public bool HasJumpedThisFrame { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsCrouching { get; private set; }

        public float RotationMultiplier => 1f;

        Health m_Health;
        PlayerInputHandler m_InputHandler;
        PlayerWeaponsManager m_WeaponsManager;
        Actor m_Actor;
        Rigidbody m_Rigidbody;
        CapsuleCollider m_Capsule;

        Vector3 m_GroundNormal;
        Vector3 m_LatestImpactSpeed;
        float m_LastTimeJumped;
        float m_CameraVerticalAngle;
        float m_FootstepDistanceCounter;
        float m_TargetCapsuleHeight;

        Vector3 m_MoveInput;
        bool m_SprintHeld;
        float m_JumpQueuedTime = -1f;
        const float k_JumpBufferWindow = 0.15f; // 입력을 0.15초간 유지
        const float k_JumpGroundingPreventionTime = 0.2f;
        const float k_GroundCheckDistanceInAir = 0.07f;
        const float k_GroundStickVelocity = -2f;

        void Awake()
        {
            ActorsManager actorsManager = FindFirstObjectByType<ActorsManager>();
            if (actorsManager != null)
                actorsManager.SetPlayer(gameObject);
        }

        void Start()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            DebugUtility.HandleErrorIfNullGetComponent<Rigidbody, PlayerCharacterController>(m_Rigidbody, this,
                gameObject);

            m_Capsule = GetComponent<CapsuleCollider>();
            DebugUtility.HandleErrorIfNullGetComponent<CapsuleCollider, PlayerCharacterController>(m_Capsule, this,
                gameObject);

            m_InputHandler = GetComponent<PlayerInputHandler>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandler, PlayerCharacterController>(m_InputHandler,
                this, gameObject);

            m_WeaponsManager = GetComponent<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerWeaponsManager, PlayerCharacterController>(
                m_WeaponsManager, this, gameObject);

            m_Health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, PlayerCharacterController>(m_Health, this, gameObject);

            m_Actor = GetComponent<Actor>();
            DebugUtility.HandleErrorIfNullGetComponent<Actor, PlayerCharacterController>(m_Actor, this, gameObject);

            m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            m_Rigidbody.useGravity = false;

            SetCrouchingState(false, true);
            UpdateCharacterHeight(true);
        }

        void Update()
        {
            if (!IsDead && transform.position.y < KillHeight)
                m_Health.Kill();

            HasJumpedThisFrame = false;

            // look rotation (non-physics)
            transform.Rotate(
                new Vector3(0f, m_InputHandler.GetLookInputsHorizontal() * RotationSpeed * RotationMultiplier, 0f),
                Space.Self);

            m_CameraVerticalAngle += m_InputHandler.GetLookInputsVertical() * RotationSpeed * RotationMultiplier;
            m_CameraVerticalAngle = Mathf.Clamp(m_CameraVerticalAngle, -89f, 89f);
            PlayerCamera.transform.localEulerAngles = new Vector3(m_CameraVerticalAngle, 0, 0);

            // crouch toggle
            if (m_InputHandler.GetCrouchInputDown())
                SetCrouchingState(!IsCrouching, false);

            UpdateCharacterHeight(false);

            // cache input for FixedUpdate
            m_MoveInput = m_InputHandler.GetMoveInput();
            m_SprintHeld = m_InputHandler.GetSprintInputHeld();
            if (m_InputHandler.GetJumpInputDown())
                m_JumpQueuedTime = Time.time;
        }

        void FixedUpdate()
        {
            bool wasGrounded = IsGrounded;
            GroundCheck();

            if (IsGrounded && !wasGrounded)
            {
                float fallSpeed = -Mathf.Min(CharacterVelocity.y, m_LatestImpactSpeed.y);
                float fallSpeedRatio = (fallSpeed - MinSpeedForFallDamage) /
                                       (MaxSpeedForFallDamage - MinSpeedForFallDamage);
                if (RecievesFallDamage && fallSpeedRatio > 0f)
                {
                    float dmgFromFall = Mathf.Lerp(FallDamageAtMinSpeed, FallDamageAtMaxSpeed, fallSpeedRatio);
                    m_Health.TakeDamage(dmgFromFall, null);
                    AudioSource.PlayOneShot(FallDamageSfx);
                }
                else
                {
                    AudioSource.PlayOneShot(LandSfx);
                }
            }

            HandleCharacterMovement();
         
        }

        void GroundCheck()
        {
            float chosenGroundCheckDistance = IsGrounded ? GroundCheckDistance : k_GroundCheckDistanceInAir;
            IsGrounded = false;
            m_GroundNormal = Vector3.up;

            if (Time.time < m_LastTimeJumped + k_JumpGroundingPreventionTime)
                return;

            float radius = Mathf.Max(0.05f, m_Capsule.radius);
            float halfHeight = Mathf.Max(radius, m_Capsule.height * 0.5f);

            Vector3 centerWorld = transform.TransformPoint(m_Capsule.center);
            Vector3 bottom = centerWorld + Vector3.down * (halfHeight - radius);

            if (Physics.SphereCast(bottom + Vector3.up * 0.01f, radius, Vector3.down, out RaycastHit hit,
                    chosenGroundCheckDistance + 0.02f, GroundCheckLayers, QueryTriggerInteraction.Ignore))
            {
                m_GroundNormal = hit.normal;
                if (Vector3.Dot(hit.normal, transform.up) > 0f && IsNormalUnderSlopeLimit(m_GroundNormal))
                    IsGrounded = true;
            }
        }

        void HandleCharacterMovement()
        {
            bool isSprinting = m_SprintHeld;
            if (isSprinting)
                isSprinting = SetCrouchingState(false, false);

            float speedModifier = isSprinting ? SprintSpeedModifier : 1f;
            Vector3 worldspaceMoveInput = transform.TransformVector(m_MoveInput);

            Vector3 velocity = CharacterVelocity;
            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);

            if (IsGrounded)
            {
                Vector3 targetVelocity = worldspaceMoveInput * MaxSpeedOnGround * speedModifier;
                if (IsCrouching)
                    targetVelocity *= MaxSpeedCrouchedRatio;

                targetVelocity = GetDirectionReorientedOnSlope(targetVelocity.normalized, m_GroundNormal) *
                                 targetVelocity.magnitude;

                Vector3 newHorizontal = Vector3.Lerp(horizontalVelocity, targetVelocity,
                    MovementSharpnessOnGround * Time.fixedDeltaTime);

                float yVel = velocity.y;
                bool jumpQueued = Time.time < m_JumpQueuedTime + k_JumpBufferWindow;
                if (jumpQueued && SetCrouchingState(false, false))
                {
                    m_JumpQueuedTime = -1f;
                    yVel = JumpForce;
                    AudioSource.PlayOneShot(JumpSfx);
                    m_LastTimeJumped = Time.time;
                    HasJumpedThisFrame = true;
                    IsGrounded = false;
                    m_GroundNormal = Vector3.up;
                }

                CharacterVelocity = new Vector3(newHorizontal.x, yVel, newHorizontal.z);

                float chosenFootstepSfxFrequency =
                    (isSprinting ? FootstepSfxFrequencyWhileSprinting : FootstepSfxFrequency);
                if (chosenFootstepSfxFrequency > 0f && m_FootstepDistanceCounter >= 1f / chosenFootstepSfxFrequency)
                {
                    m_FootstepDistanceCounter = 0f;
                    AudioSource.PlayOneShot(FootstepSfx);
                }

                m_FootstepDistanceCounter += newHorizontal.magnitude * Time.fixedDeltaTime;
            }
            else
            {
                horizontalVelocity += worldspaceMoveInput * AccelerationSpeedInAir * Time.fixedDeltaTime;
                horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, MaxSpeedInAir * speedModifier);

                float yVel = velocity.y - GravityDownForce * Time.fixedDeltaTime;
                CharacterVelocity = new Vector3(horizontalVelocity.x, yVel, horizontalVelocity.z);

            }

            m_LatestImpactSpeed = CharacterVelocity;
        }

        bool IsNormalUnderSlopeLimit(Vector3 normal)
        {
            // 45도 기준 (cos 45°)
            return Vector3.Dot(normal.normalized, Vector3.up) >= 0.70710677f;
        }

        public Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
        {
            Vector3 directionRight = Vector3.Cross(direction, transform.up);
            return Vector3.Cross(slopeNormal, directionRight).normalized;
        }

        void UpdateCharacterHeight(bool force)
        {
            if (force)
            {
                m_Capsule.height = m_TargetCapsuleHeight;
                m_Capsule.center = Vector3.up * m_Capsule.height * 0.5f;
                PlayerCamera.transform.localPosition = Vector3.up * m_TargetCapsuleHeight * CameraHeightRatio;
            }
            else if (!Mathf.Approximately(m_Capsule.height, m_TargetCapsuleHeight))
            {
                m_Capsule.height = Mathf.Lerp(m_Capsule.height, m_TargetCapsuleHeight, CrouchingSharpness * Time.deltaTime);
                m_Capsule.center = Vector3.up * m_Capsule.height * 0.5f;
                PlayerCamera.transform.localPosition = Vector3.Lerp(PlayerCamera.transform.localPosition,
                    Vector3.up * m_TargetCapsuleHeight * CameraHeightRatio, CrouchingSharpness * Time.deltaTime);
            }
        }

        bool SetCrouchingState(bool crouched, bool ignoreObstructions)
        {
            if (crouched)
            {
                m_TargetCapsuleHeight = CapsuleHeightCrouching;
            }
            else
            {
                if (!ignoreObstructions)
                {
                    float radius = Mathf.Max(0.05f, m_Capsule.radius);
                    float halfStanding = Mathf.Max(radius, CapsuleHeightStanding * 0.5f);

                    Vector3 centerWorld = transform.TransformPoint(m_Capsule.center);
                    Vector3 bottom = centerWorld + Vector3.down * (halfStanding - radius);
                    Vector3 top = centerWorld + Vector3.up * (halfStanding - radius);

                    Collider[] overlaps =
                        Physics.OverlapCapsule(bottom, top, radius, -1, QueryTriggerInteraction.Ignore);
                    foreach (var c in overlaps)
                    {
                        if (c != m_Capsule)
                            return false;
                    }
                }

                m_TargetCapsuleHeight = CapsuleHeightStanding;
            }

            OnStanceChanged?.Invoke(crouched);
            IsCrouching = crouched;
            return true;
        }
    }
}