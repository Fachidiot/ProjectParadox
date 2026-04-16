using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>씬 전환 애니메이션의 추상 베이스 클래스로 StartAni→PostChange→PostEnd 순으로 호출</summary>
public abstract class SceneChangeAni : MonoBehaviour
{
    #region Event
    /// <summary>애니메이션 컴포넌트를 초기화하고 비활성화</summary>
    public virtual void Init()
    {
        gameObject.SetActive(false);
    }

    #endregion
    #region Manual Function
    /// <summary>씬 전환 시작 애니메이션을 재생</summary>
    public virtual void StartAni()
    {
        gameObject.SetActive(true);
    }
    /// <summary>씬 전환 종료 애니메이션을 재생</summary>
    public virtual void EndAni()
    {
    }

    /// <summary>SceneChangeManager에 씬 변경을 요청</summary>
    protected void PostChange()
    {
        SceneChangeManager.instance.OnChange();
    }
    /// <summary>애니메이션 완료를 SceneChangeManager에 알림</summary>
    protected void PostEnd()
    {
        gameObject.SetActive(false);
        SceneChangeManager.instance.OnEnd();
    }
    #endregion
}
