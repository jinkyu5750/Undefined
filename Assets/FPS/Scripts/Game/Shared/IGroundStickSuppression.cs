namespace Unity.FPS.Game
{
    /// <summary>
    /// Rigidbody 기반 플레이어가 충돌 후 세로 속도를 보정할 때 이 접촉은 건너뜁니다 (예: Elasticity 튕김).
    /// </summary>
    public interface IGroundStickSuppression
    {
        bool SuppressRigidbodyGroundStick { get; }
    }
}
