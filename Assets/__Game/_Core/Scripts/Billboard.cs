using UnityEngine;

/// <summary>항상 카메라를 향하도록 회전하는 빌보드</summary>
public class Billboard : MonoBehaviour
{
    [SerializeField] private bool m_YAxisOnly;
    private Transform m_Camera;

    private void Start()
    {
        m_Camera = LocalCameraManager.instance.CurCam.transform;
    }

    private void LateUpdate()
    {
        if (m_YAxisOnly)
        {
            var forward = m_Camera.forward;
            forward.y = 0;
            transform.forward = forward;
        }
        else
        {
            transform.forward = m_Camera.forward;
        }
    }
}
