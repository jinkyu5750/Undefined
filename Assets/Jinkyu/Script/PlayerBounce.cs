using Unity.FPS.Gameplay;
using UnityEngine;

public class PlayerBounce : MonoBehaviour
{
    public float elasticityUpImpulse = 12f;
    public float bounceCooldown = 0.15f; // 같은 접촉에서 중복 방지
    float _lastBounceTime;
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var os = hit.gameObject.GetComponent<ObjectScript>();
        if (os == null) return;
        if (os.GetData().properties.dynamicProperty != DynamicPropertyType.Elasticity) return;
        // "바닥에 착지(Enter 느낌)"일 때만: 아래로 내려오며 바닥을 밟는 경우
     //   if (hit.normal.y < 0.5f) return;        // 옆면/천장 제외
        if (hit.moveDirection.y >= 0f) return;  // 올라가는 중 제외
        if (Time.time < _lastBounceTime + bounceCooldown) return;
        _lastBounceTime = Time.time;
        var pcc = GetComponent<PlayerCharacterController>();
        var v = pcc.CharacterVelocity;
        v.y = 0f;                    // 내려가던 속도 제거
        v.y += elasticityUpImpulse;  // 점프처럼 1번만 위로
        pcc.CharacterVelocity = v;
    }
}
