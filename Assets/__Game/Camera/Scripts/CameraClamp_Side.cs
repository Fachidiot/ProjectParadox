using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>사이드뷰 카메라 위치 및 줌 제한 관리</summary>
public class CameraClamp_Side : CameraClampBase
{
    #region Inspector
    [SerializeField, TabGroup("Option"), LabelText("기준 깊이")] private float m_TestDepth = 0;
    [SerializeField, TabGroup("Option"), LabelText("Rect 왼쪽 아래")] private Vector2 m_RectLeftBot = new Vector2(0, 0);
    [SerializeField, TabGroup("Option"), LabelText("Rect 오른쪽 위")] private Vector2 m_RectRightTop = new Vector2(0, 0);

    [Space(15)]
    [SerializeField, TabGroup("Option"), LabelText("줌 제한 타입")] private EZoomType m_ZoomClampType = EZoomType.CameraZoom;
    [SerializeField, TabGroup("Option"), ShowIf("IsZoomTypeCameraZoom"), MinMaxSlider(0.1f, 179.9f, true), LabelText("줌 범위(Perspective)")] private Vector2 m_FOVRange = new Vector2(30.0f, 90.0f);
    [SerializeField, TabGroup("Option"), ShowIf("IsZoomTypeCameraZoom"), MinMaxSlider(0.1f, 1000.0f, true), LabelText("줌 범위(Orthographic)")] private Vector2 m_OrthographicSizeRange = new Vector2(3.0f, 10.0f);
    [SerializeField, TabGroup("Option"), ShowIf("IsZoomTypePositionZoom"), MinMaxSlider(0, 500, true), LabelText("줌 범위(거리)")] private Vector2 m_ZoomDistanceRange = new Vector2(10.0f, 100.0f);
    #endregion
    #region Get,Set
    /// <summary>테스트 기준 깊이</summary>
    public float TestDepth => m_TestDepth;
    /// <summary>클램프 Rect 왼쪽 아래 좌표</summary>
    public Vector2 RectLeftBot => m_RectLeftBot;
    /// <summary>클램프 Rect 오른쪽 위 좌표</summary>
    public Vector2 RectRightTop => m_RectRightTop;
    /// <summary>줌 제한 타입</summary>
    public EZoomType ZoomClampType => m_ZoomClampType;

    private bool IsZoomTypeCameraZoom => m_ZoomClampType == EZoomType.CameraZoom;
    private bool IsZoomTypePositionZoom => m_ZoomClampType == EZoomType.PositionZoom;
    #endregion

    #region Function
    public override void Clamp(ref Vector3 pos, ref Quaternion rot, ref float zoom)
    {
        ClampPos(ref pos);
        ClampZoom(ref pos, ref zoom);
    }

    private void ClampPos(ref Vector3 pos)
    {
        Ray screenRay = ParCamMgr.GetScreenRay(new Vector2(Screen.width, Screen.height));
        Plane testPlane = GetTestPlane(m_TestDepth);
        Vector2? halfSize = ParCamMgr.GetLocalScreenPos_Plane(screenRay, testPlane);
        if (halfSize.HasValue)
        {
            pos.x = Mathf.Clamp(pos.x, m_RectLeftBot.x + halfSize.Value.x, m_RectRightTop.x - halfSize.Value.x);
            pos.y = Mathf.Clamp(pos.y, m_RectLeftBot.y + halfSize.Value.y, m_RectRightTop.y - halfSize.Value.y);
        }
    }
    private void ClampZoom(ref Vector3 pos, ref float zoom)
    {
        if (m_ZoomClampType == EZoomType.CameraZoom)
        {
            if (ParCamMgr.IsOrthographic)
                zoom = Mathf.Clamp(zoom, m_OrthographicSizeRange.x, m_OrthographicSizeRange.y);
            else
                zoom = Mathf.Clamp(zoom, m_FOVRange.x, m_FOVRange.y);
        }
        else if (m_ZoomClampType == EZoomType.PositionZoom)
        {
            pos.z = Mathf.Clamp(pos.z, -m_ZoomDistanceRange.y, -m_ZoomDistanceRange.x);
        }
    }

    /// <summary>테스트 깊이 기준 Plane 생성</summary>
    public static Plane GetTestPlane(float testDepth)
    {
        return new Plane(new Vector3(0, 0, -1), new Vector3(0, 0, testDepth));
    }

    /// <summary>테스트 깊이 설정</summary>
    public void SetTestDepth(float value) => m_TestDepth = value;

    /// <summary>클램프 Rect 설정</summary>
    public void SetClampRect(Vector2 leftBot, Vector2 rightTop)
    {
        m_RectLeftBot = leftBot;
        m_RectRightTop = rightTop;
    }

    /// <summary>줌 제한 타입 설정</summary>
    public void SetZoomClampType(EZoomType value) => m_ZoomClampType = value;

    /// <summary>줌 범위 설정</summary>
    public void SetZoomRange(float min, float max)
    {
        if (ParCamMgr.IsOrthographic)
            m_OrthographicSizeRange = new Vector2(min, max);
        else
            m_FOVRange = new Vector2(min, max);
    }

    /// <summary>줌 거리 범위 설정</summary>
    public void SetZoomDistRange(float min, float max) => m_ZoomDistanceRange = new Vector2(min, max);
    #endregion
}
