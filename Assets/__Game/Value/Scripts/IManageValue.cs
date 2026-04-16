using UnityEngine;

/// <summary>ValueBase를 관리하기 위한 인터페이스로 ManagerBase가 구현</summary>
public interface IManageValue
{
    /// <summary>값을 매니저에서 관리하도록 등록</summary>
    public void ManageValue(ValueBase _value);
}
