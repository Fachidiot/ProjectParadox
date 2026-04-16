using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>인스펙터 메모용 컴포넌트, 에디터 전용</summary>
public class Info : MonoBehaviour
{
#if UNITY_EDITOR
    /// <summary>메모 내용</summary>
    [SerializeField, MultiLineProperty(10)] public string InfoText;
#endif
}
