/// <summary>
/// 사용자 입력키 재설정(Rebinding) 기능을 정의하는 인터페이스.
/// 특정 액션의 키를 변경하거나 바인딩 정보를 저장 및 로드하는 기능을 포함함.
/// </summary>

using System;

public interface IRebind
{
    /// <summary>
    /// 특정 입력 액션의 키 바인딩 변경 프로세스를 시작함.
    /// </summary>
    /// <param name="actionName">변경할 액션 이름</param>
    /// <param name="bindingIndex">변경할 바인딩 인덱스(복합키 등)</param>
    /// <param name="onComplete">변경 완료 시 콜백</param>
    /// <param name="onCancel">변경 취소 시 콜백</param>
    public void StartRebinding(string actionName, int bindingIndex, Action onComplete, Action onCancel = null);

    /// <summary>
    /// 변경된 키 바인딩 정보를 데이터로 저장함.
    /// </summary>
    public void SaveBindings();

    /// <summary>
    /// 저장된 키 바인딩 정보를 로드하여 시스템에 적용함.
    /// </summary>
    public void LoadBindings();
}
