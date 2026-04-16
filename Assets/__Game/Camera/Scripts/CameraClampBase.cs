using UnityEngine;

/// <summary>카메라 클램프 베이스 클래스, 위치/회전/줌 제한 추상 정의</summary>
public abstract class CameraClampBase : MonoBehaviour
{
    #region Type
    public enum EZoomType
    {
        None,
        CameraZoom,
        PositionZoom,
    }
    #endregion
    #region Get,Set
    /// <summary>부모 카메라 매니저</summary>
    public LocalCameraManager ParCamMgr { get; private set; }
    #endregion

    #region Event
    /// <summary>클램프 초기화 및 부모 매니저 연결</summary>
    public void Init(LocalCameraManager cameraManager)
    {
        ParCamMgr = cameraManager;
    }
    #endregion
    #region Function
    /// <summary>위치/회전/줌 값 제한 처리</summary>
    public abstract void Clamp(ref Vector3 pos, ref Quaternion rot, ref float zoom);
    #endregion
}
