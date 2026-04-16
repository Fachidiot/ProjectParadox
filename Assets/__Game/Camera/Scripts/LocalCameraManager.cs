using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>카메라 관리 매니저, 위치/회전/줌 애니메이션 및 Clamp 지원</summary>
public class LocalCameraManager : LocalManagerBase
{
    public static LocalCameraManager instance { get; private set; }

    #region Inspector
    [SerializeField, TabGroup("Component"), LabelText("카메라")] private Camera m_Camera;
    [SerializeField, TabGroup("Component"), LabelText("카메라 회전 기준")] private Transform m_CameraRotRoot;
    [SerializeField, TabGroup("Component"), LabelText("카메라 위치 기준")] private Transform m_CameraPosRoot;
    [SerializeField, TabGroup("Component"), LabelText("카메라 제한")] private CameraClampBase m_CameraClamp;

    [SerializeField, TabGroup("Option"), LabelText("줌 타입")] private CameraClampBase.EZoomType m_ZoomType = CameraClampBase.EZoomType.CameraZoom;

    [SerializeField, TabGroup("Animation"), LabelText("이동 커브")] private AnimationCurve m_PosCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField, TabGroup("Animation"), LabelText("이동 속도")] private float m_PosSpeed = 3.0f;
    [SerializeField, TabGroup("Animation"), LabelText("회전 커브")] private AnimationCurve m_RotCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField, TabGroup("Animation"), LabelText("회전 속도")] private float m_RotSpeed = 3.0f;
    [SerializeField, TabGroup("Animation"), LabelText("줌 커브")] private AnimationCurve m_ZoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField, TabGroup("Animation"), LabelText("줌 속도")] private float m_ZoomSpeed = 3.0f;
    #endregion
    #region Get,Set
    /// <summary>현재 카메라</summary>
    public Camera CurCam => m_Camera;
    /// <summary>회전 기준 Transform</summary>
    public Transform RotRoot => m_CameraRotRoot;
    /// <summary>위치 기준 Transform</summary>
    public Transform PosRoot => m_CameraPosRoot;
    /// <summary>현재 카메라 제한 컴포넌트</summary>
    public CameraClampBase CurClamp => m_CameraClamp;
    /// <summary>Orthographic 카메라 여부</summary>
    public bool IsOrthographic => m_Camera.orthographic;
    /// <summary>줌 타입</summary>
    public CameraClampBase.EZoomType ZoomType => m_ZoomType;
    /// <summary>PositionZoom 여부</summary>
    public bool IsPositionZoom => m_ZoomType == CameraClampBase.EZoomType.PositionZoom;

    /// <summary>목표 위치</summary>
    public Vector3 TargetPos => m_TargetPos ?? NowPos;
    /// <summary>목표 회전</summary>
    public Quaternion TargetRot => m_TargetRot ?? NowRot;
    /// <summary>목표 줌</summary>
    public float TargetZoom => IsPositionZoom ? -(m_TargetPos ?? NowPos).z : (m_TargetZoom ?? CurrentCameraZoom);

    /// <summary>현재 위치</summary>
    public Vector3 NowPos => m_CameraPosRoot.localPosition;
    /// <summary>현재 회전</summary>
    public Quaternion NowRot => m_CameraRotRoot.localRotation;
    /// <summary>현재 카메라 줌 (FOV/OrthoSize)</summary>
    public float CurrentCameraZoom => m_Camera.orthographic ? m_Camera.orthographicSize : m_Camera.fieldOfView;
    /// <summary>현재 줌 (타입에 따라 거리 또는 카메라줌)</summary>
    public float CurrentZoom => IsPositionZoom ? -NowPos.z : CurrentCameraZoom;
    #endregion
    #region Value
    private Vector3? m_TargetPos;
    private Vector3 m_StartPos;
    private float m_PosTimer;

    private Quaternion? m_TargetRot;
    private Quaternion m_StartRot;
    private float m_RotTimer;

    private float? m_TargetZoom;
    private float m_StartZoom;
    private float m_ZoomTimer;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    public override void Init()
    {
        m_CameraClamp?.Init(this);
        base.Init();
    }
    private void Update()
    {
        if (m_TargetPos.HasValue)
        {
            m_PosTimer += Time.deltaTime * m_PosSpeed;
            float t = m_PosCurve.Evaluate(Mathf.Clamp01(m_PosTimer));
            SetPosInternal(Vector3.LerpUnclamped(m_StartPos, m_TargetPos.Value, t));
            if (m_PosTimer >= 1.0f)
            {
                SetPosInternal(m_TargetPos.Value);
                m_TargetPos = null;
            }
        }

        if (m_TargetRot.HasValue)
        {
            m_RotTimer += Time.deltaTime * m_RotSpeed;
            float t = m_RotCurve.Evaluate(Mathf.Clamp01(m_RotTimer));
            SetRotInternal(Quaternion.LerpUnclamped(m_StartRot, m_TargetRot.Value, t));
            if (m_RotTimer >= 1.0f)
            {
                SetRotInternal(m_TargetRot.Value);
                m_TargetRot = null;
            }
        }

        if (m_TargetZoom.HasValue)
        {
            m_ZoomTimer += Time.deltaTime * m_ZoomSpeed;
            float t = m_ZoomCurve.Evaluate(Mathf.Clamp01(m_ZoomTimer));
            SetZoomInternal(Mathf.LerpUnclamped(m_StartZoom, m_TargetZoom.Value, t));
            if (m_ZoomTimer >= 1.0f)
            {
                SetZoomInternal(m_TargetZoom.Value);
                m_TargetZoom = null;
            }
        }
    }
    #endregion
    #region Function
    /// <summary>스크린 좌표로부터 레이 생성</summary>
    public Ray GetScreenRay(Vector2 _screenPos)
    {
        return m_Camera.ScreenPointToRay(new Vector3(_screenPos.x, _screenPos.y, 0));
    }
    /// <summary>레이와 평면의 교차점 월드 좌표 반환</summary>
    public Vector3? GetWorldScreenPos_Plane(Ray ray, Plane plane)
    {
        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);
        return null;
    }
    /// <summary>레이와 평면의 교차점 로컬 좌표 반환</summary>
    public Vector3? GetLocalScreenPos_Plane(Ray ray, Plane plane)
    {
        Vector3? worldScreenPos = GetWorldScreenPos_Plane(ray, plane);
        if (worldScreenPos.HasValue)
            return m_Camera.transform.InverseTransformPoint(worldScreenPos.Value);
        return null;
    }

    /// <summary>위치 즉시 설정</summary>
    public void SetPos(Vector3 pos)
    {
        m_TargetPos = null;
        SetPosInternal(pos);
        ClampCurrent();
    }
    /// <summary>회전 즉시 설정</summary>
    public void SetRot(Quaternion rot)
    {
        m_TargetRot = null;
        SetRotInternal(rot);
        ClampCurrent();
    }
    /// <summary>줌 즉시 설정</summary>
    public void SetZoom(float zoom)
    {
        if (IsPositionZoom)
        {
            m_TargetPos = null;
            Vector3 pos = NowPos;
            pos.z = -zoom;
            SetPosInternal(pos);
            ClampCurrent();
        }
        else
        {
            m_TargetZoom = null;
            SetZoomInternal(zoom);
            ClampCurrent();
        }
    }

    /// <summary>위치 부드럽게 변경</summary>
    public void ChangePos(Vector3 pos)
    {
        m_TargetPos = pos;
        m_StartPos = NowPos;
        m_PosTimer = 0.0f;
        ClampTarget();
    }
    /// <summary>회전 부드럽게 변경</summary>
    public void ChangeRot(Quaternion rot)
    {
        m_TargetRot = rot;
        m_StartRot = NowRot;
        m_RotTimer = 0.0f;
        ClampTarget();
    }
    /// <summary>줌 부드럽게 변경</summary>
    public void ChangeZoom(float zoom)
    {
        if (IsPositionZoom)
        {
            Vector3 pos = TargetPos;
            pos.z = -zoom;
            m_TargetPos = pos;
            m_StartPos = NowPos;
            m_PosTimer = 0.0f;
            ClampTarget();
        }
        else
        {
            m_TargetZoom = zoom;
            m_StartZoom = CurrentCameraZoom;
            m_ZoomTimer = 0.0f;
            ClampTarget();
        }
    }

    /// <summary>목표 값들을 제한 범위 내로 클램프</summary>
    public void ClampTarget()
    {
        Vector3 pos = TargetPos;
        Quaternion rot = TargetRot;
        float zoom = TargetZoom;

        m_CameraClamp?.Clamp(ref pos, ref rot, ref zoom);

        if (m_TargetPos.HasValue)
            m_TargetPos = pos;
        else
            SetPosInternal(pos);

        if (m_TargetRot.HasValue)
            m_TargetRot = rot;
        else
            SetRotInternal(rot);

        if (!IsPositionZoom)
        {
            if (m_TargetZoom.HasValue)
                m_TargetZoom = zoom;
            else
                SetZoomInternal(zoom);
        }
    }
    /// <summary>현재 값들을 제한 범위 내로 클램프</summary>
    public void ClampCurrent()
    {
        Vector3 pos = NowPos;
        Quaternion rot = NowRot;
        float zoom = CurrentZoom;

        m_CameraClamp?.Clamp(ref pos, ref rot, ref zoom);

        SetPosInternal(pos);
        SetRotInternal(rot);
        if (!IsPositionZoom)
            SetZoomInternal(zoom);
    }

    private void SetPosInternal(Vector3 pos) => m_CameraPosRoot.localPosition = pos;
    private void SetRotInternal(Quaternion rot) => m_CameraRotRoot.localRotation = rot;
    private void SetZoomInternal(float zoom)
    {
        if (m_Camera.orthographic)
            m_Camera.orthographicSize = zoom;
        else
            m_Camera.fieldOfView = zoom;
    }
    #endregion
    #region Event - Editor
#if UNITY_EDITOR
    protected virtual void Reset()
    {
        CameraManagerInitContext();
    }
#endif
    #endregion
    #region Function - Editor
#if UNITY_EDITOR
    /// <summary>컨텍스트 메뉴에서 카메라 매니저 초기화</summary>
    [ContextMenu("CameraManager Init")]
    public virtual void CameraManagerInitContext()
    {
        Undo.RecordObject(gameObject, $"CameraManager Init {gameObject.name}");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.SetDirty(this);

        m_Camera = GetComponentInChildren<Camera>();
        m_CameraPosRoot = m_Camera?.transform.parent;
        m_CameraRotRoot = m_CameraPosRoot?.transform.parent;
    }
#endif
    #endregion
}
