/// <summary>공격 가능한 오브젝트 인터페이스</summary>
public interface IAttackable
{
    void OnAttacked(PlayerActor _attacker);
}
