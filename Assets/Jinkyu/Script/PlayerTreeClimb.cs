using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
[RequireComponent(typeof(PlayerSystem))]
[RequireComponent(typeof(PlayerCharacterController))]


public class PlayerTreeClimb : MonoBehaviour
{
    [Header("Entry")]
    [Tooltip("W 또는 S 입력 시 타기 시작")]
    [SerializeField] private bool requireVerticalInputToMount = true;
    PlayerSystem playerSystem;
    [SerializeField] Tree_Growth activeTree;
    PlayerCharacterController controller;
    [Header("Tree Climb")]
    public float ClimbSpeed = 3f;

    public bool IsClimbing => controller != null && controller.IsClimbing;
    private float climbAngle;   // 실린더 축 기준 현재 각도
    private float climbRadius;  // 실린더 표면까지의 반지름 (고정)
    private Vector3 climbRefDir; // 각도 계산 기준이 되는 방향 (클라이밍 시작할 때 1회 설정)

    void Awake()
    {
        playerSystem = GetComponent<PlayerSystem>();
        controller = GetComponent<PlayerCharacterController>();
    }

    private void FixedUpdate()
    {
        if (controller.IsClimbing)
        {
            HandleTreeClimbMovement();
            // 내리는 조건: 바닥 닿음
            if (controller.IsGrounded && controller.GetCachedMoveInput().y < -0.1f)
                ExitTreeClimb();
            return;
        }


    }

    public void TickUpdate()
    {
        if (!controller.IsClimbing && Keyboard.current.fKey.wasPressedThisFrame)
            TryMount();
    }
    public void EnterTreeClimb(Tree_Growth tree)
    {
        if (tree == null || IsClimbing)
            return;

        activeTree = tree;
        controller.SetIsClimbing(true);
        controller.UseGravity(false);
        // 나무 위에서는 중력/지면 이동 대신 직접 속도 제어
        controller.CharacterVelocity = Vector3.zero;
        GetComponent<Rigidbody>().position += Vector3.up;

        Vector3 axis = activeTree.transform.up; // 실린더가 기울어져 있으면 실제 축으로 교체
        Vector3 offset = transform.position - activeTree.transform.position;

        float height = Vector3.Dot(offset, axis);
        Vector3 flatOffset = offset - axis * height;
        climbRadius = flatOffset.magnitude;

        // 기준 방향(0도) 설정 ? 이후 계속 이 값을 기준으로 각도 계산
        climbRefDir = flatOffset.normalized;
        climbAngle = 0f;
    }
    public void ExitTreeClimb()
    {
        controller.SetIsClimbing(false);
        controller.UseGravity(true);
        activeTree = null;
    }
    void TryMount()
    {
        if (!playerSystem.canClimbTree || playerSystem.detectedTree == null)
            return;
        Vector3 moveInput = controller.GetCachedMoveInput();

        /*      if (requireVerticalInputToMount && Mathf.Abs(moveInput.y) < 0.1f)
                  return;*/

        activeTree = playerSystem.detectedTree;
        EnterTreeClimb(activeTree);
    }
    /// <summary>Controller에서 점프/착지 시 호출</summary>
    void HandleTreeClimbMovement()
    {
        if (!controller.canMove || activeTree == null)
            return;

   
        Vector3 axis = activeTree.transform.up;
        Vector3 input = controller.GetCachedMoveInput();
       
        // A/D  각속도로 변환 (반지름으로 나눠서 표면 이동 속도를 유지) // -input.x인 이유는 axis를 위에서 내려봤을때 시계방향이 양수이기떄문에
        float deltaAngle = (-input.x * ClimbSpeed / Mathf.Max(climbRadius, 0.01f)) * Time.fixedDeltaTime;
        climbAngle += deltaAngle;

        // 축 기준 회전 벡터 계산 (climbRefDir을 axis 기준으로 회전)
        Quaternion rot = Quaternion.AngleAxis(climbAngle * Mathf.Rad2Deg, axis);
        Vector3 radialDir = rot * climbRefDir;
        Vector3 tangentDir = Vector3.Cross(axis, radialDir);

        Vector3 jumpDir = -radialDir;
        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 tangential = tangentDir * (-input.x);
            Vector3 axial = axis * input.y;
            jumpDir = (-radialDir + tangential + axial).normalized;
        }


        // 점프 = 내리는 조건
        if (controller.TryApplyJump(jumpDir))
        {
            ExitTreeClimb();
            return;
        }
        
        // 현재 높이 유지한 채, 반지름 방향만 갱신
        Vector3 offset = transform.position - activeTree.transform.position;
        float currentHeight = Vector3.Dot(offset, axis);
        float newHeight = currentHeight + input.y * ClimbSpeed * Time.fixedDeltaTime;

        Vector3 targetPos = activeTree.transform.position
                           + axis * newHeight
                           + radialDir * climbRadius;

        Vector3 velocity = (targetPos - transform.position) / Time.fixedDeltaTime;
        controller.CharacterVelocity = velocity;

        // 캐릭터가 표면을 마주 보도록 회전
      // transform.rotation = Quaternion.LookRotation(-radialDir, axis);
    }


    //    public Tree GetActiveTree() => activeTree;
}