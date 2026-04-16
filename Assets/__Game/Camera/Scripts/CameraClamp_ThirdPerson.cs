using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>3인칭 카메라 회전(Pitch/Yaw) 및 줌 제한 관리</summary>
public class CameraClamp_ThirdPerson : CameraClampBase
{
    #region Inspector
    [SerializeField, TabGroup("Option"), LabelText("Pitch 제한")] private bool m_ClampPitch = true;
    [SerializeField, TabGroup("Option"), ShowIf("m_ClampPitch"), MinMaxSlider(-90f, 90f, true), LabelText("Pitch 범위")] private Vector2 m_PitchRange = new Vector2(-60f, 80f);

    [Space(10)]
    [SerializeField, TabGroup("Option"), LabelText("Yaw 제한")] private bool m_ClampYaw = false;
    [SerializeField, TabGroup("Option"), ShowIf("m_ClampYaw"), MinMaxSlider(-180f, 180f, true), LabelText("Yaw 범위")] private Vector2 m_YawRange = new Vector2(-180f, 180f);

    [Space(15)]
    [SerializeField, TabGroup("Option"), LabelText("줌 제한 타입")] private EZoomType m_ZoomClampType = EZoomType.PositionZoom;
    [SerializeField, TabGroup("Option"), ShowIf("IsZoomTypeCameraZoom"), MinMaxSlider(0.1f, 179.9f, true), LabelText("줌 범위(Perspective)")] private Vector2 m_FOVRange = new Vector2(30.0f, 90.0f);
    [SerializeField, TabGroup("Option"), ShowIf("IsZoomTypeCameraZoom"), MinMaxSlider(0.1f, 1000.0f, true), LabelText("줌 범위(Orthographic)")] private Vector2 m_OrthographicSizeRange = new Vector2(3.0f, 10.0f);
    [SerializeField, TabGroup("Option"), ShowIf("IsZoomTypePositionZoom"), MinMaxSlider(0, 500, true), LabelText("줌 범위(거리)")] private Vector2 m_ZoomDistanceRange = new Vector2(2.0f, 20.0f);
    #endregion
    #region Get,Set
    /// <summary>Pitch 제한 여부</summary>
    public bool ClampPitch => m_ClampPitch;
    /// <summary>Pitch 범위</summary>
    public Vector2 PitchRange => m_PitchRange;
    /// <summary>Yaw 제한 여부</summary>
    public bool ClampYaw => m_ClampYaw;
    /// <summary>Yaw 범위</summary>
    public Vector2 YawRange => m_YawRange;
    /// <summary>줌 제한 타입</summary>
    public EZoomType ZoomClampType => m_ZoomClampType;

    private bool IsZoomTypeCameraZoom => m_ZoomClampType == EZoomType.CameraZoom;
    private bool IsZoomTypePositionZoom => m_ZoomClampType == EZoomType.PositionZoom;
    #endregion

    #region Function
    public override void Clamp(ref Vector3 pos, ref Quaternion rot, ref float zoom)
    {
        ClampRot(ref rot);
        ClampZoom(ref pos, ref zoom);
    }

    private void ClampRot(ref Quaternion rot)
    {
        Vector3 euler = rot.eulerAngles;

        if (m_ClampPitch)
        {
            float pitch = NormalizeAngle(euler.x);
            pitch = Mathf.Clamp(pitch, m_PitchRange.x, m_PitchRange.y);
            euler.x = pitch;
        }

        if (m_ClampYaw)
        {
            float yaw = NormalizeAngle(euler.y);
            yaw = Mathf.Clamp(yaw, m_YawRange.x, m_YawRange.y);
            euler.y = yaw;
        }

        rot = Quaternion.Euler(euler);
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

    /// <summary>각도를 -180~180 범위로 정규화</summary>
    private static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }

    /// <summary>Pitch 범위 설정</summary>
    public void SetPitchRange(float min, float max) => m_PitchRange = new Vector2(min, max);

    /// <summary>Yaw 범위 설정</summary>
    public void SetYawRange(float min, float max) => m_YawRange = new Vector2(min, max);

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
