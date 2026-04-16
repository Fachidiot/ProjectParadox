/// <summary>
/// Superliminal 스타일의 강제 원근법(Forced Perspective) 상호작용을 처리하는 컴포넌트.
/// 물체를 잡았을 때 시각적 크기를 유지하면서 거리에 따라 실제 물리 스케일을 조정합니다.
/// </summary>

using UnityEngine;
using System.Collections.Generic;
public class PerspectiveGrab : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] private LayerMask _draggableLayer;
    [SerializeField] private LayerMask _environmentLayer;
    [SerializeField] private float _maxGrabDistance = 10f;
    [SerializeField] private float _minScale = 0.1f;
    [SerializeField] private float _maxScale = 50f;
    [SerializeField] private float _smoothing = 10f;
    [SerializeField] private float _playerPadding = 1.0f;
    [SerializeField] private float _heldTransparency = 0.5f;
    [SerializeField] private float _standardGrabDistance = 3.0f;
    [SerializeField] private float _actionCooldown = 0.2f;

    public enum PivotMode { TransformPivot, CenterOfBounds }
    [Header("피벗 설정")]
    [SerializeField] private PivotMode _pivotMode = PivotMode.CenterOfBounds;

    private PlayerInputHandler _inputHandler;
    private Transform _heldObject;
    private Camera _playerCamera;
    private float _lastActionTime;
    private Collider _playerCollider;

    // 수학적 원근법 계산을 위한 불변 데이터
    private float _initialCenterDistance; // 초기 상태의 카메라-중심점 거리
    private float _initialRadius;         // 초기 상태의 물체 반지름(Bounding Sphere)
    private float _baseRatio;             // 원근법 비례 상수 (Radius / Distance)
    private Vector3 _initialObjectScale;   // 잡는 순간의 로컬 스케일
    private Vector3 _localPivotOffset;    // 모델 피벗과 바운드 중심 간의 로컬 오프셋

    // 상태 복구를 위한 데이터 저장소
    private Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();
    private Dictionary<Collider, bool> _originalTriggerStates = new Dictionary<Collider, bool>();

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _playerCamera = Camera.main;
        _playerCollider = GetComponent<Collider>();

        if (_inputHandler == null)
            Debug.LogError("[PerspectiveGrab] PlayerInputHandler를 찾을 수 없습니다!");
    }

    private void Update()
    {
        HandleInput();

        if (_heldObject != null)
        {
            UpdateObjectTransform();
        }
    }

    /// <summary>
    /// 입력 처리 및 잡기/놓기 토글 관리
    /// </summary>
    private void HandleInput()
    {
        if (_inputHandler == null) return;

        if (_inputHandler.ActionTriggered && Time.time >= _lastActionTime + _actionCooldown)
        {
            _lastActionTime = Time.time;
            if (_heldObject == null) TryGrab();
            else Release();
        }
    }

    /// <summary>
    /// 시선 정중앙의 레이캐스트를 통해 물체를 잡습니다.
    /// </summary>
    private void TryGrab()
    {
        Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, _maxGrabDistance, _draggableLayer))
        {
            _heldObject = hit.transform;

            // 1. 초기 스케일 저장 및 클램핑
            Vector3 currentScale = _heldObject.localScale;
            _initialObjectScale = new Vector3(
                Mathf.Min(currentScale.x, _maxScale),
                Mathf.Min(currentScale.y, _maxScale),
                Mathf.Min(currentScale.z, _maxScale)
            );
            _heldObject.localScale = _initialObjectScale;

            // 2. 바운드 계산 전 회전 동기화 (원근법 일관성을 위해 카메라 정면으로 회전)
            _heldObject.rotation = _playerCamera.transform.rotation;

            // 3. 기하학적 불변량 계산 (수학적 원근법의 핵심)
            Bounds b = GetCombinedBounds(_heldObject);
            _initialRadius = b.extents.magnitude;

            // 시각적 크기를 고정하기 위한 기준 거리와 비율 설정
            _initialCenterDistance = _standardGrabDistance + _initialRadius;
            _baseRatio = _initialRadius / _initialCenterDistance;

            // 4. 피벗 보정 (CenterOfBounds 모드일 때만)
            _localPivotOffset = Vector3.zero;
            if (_pivotMode == PivotMode.CenterOfBounds)
            {
                _localPivotOffset = _heldObject.InverseTransformVector(_heldObject.position - b.center);
            }

            // 5. 초기 위치 스냅
            Vector3 worldPivotOffset = _heldObject.TransformVector(_localPivotOffset);
            _heldObject.position = ray.GetPoint(_initialCenterDistance) + worldPivotOffset;

            // 6. 물리 상태 전이
            PrepareHeldObject(true);
        }
    }

    /// <summary>
    /// 물체의 크기를 실시간으로 업데이트하여 원근법 효과를 구현합니다.
    /// </summary>
    private void UpdateObjectTransform()
    {
        Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // 1. 벽면 스캐닝 (최소한의 물리 판정용 구체 사용)
        float testRadius = 0.1f;
        float searchDist = 100f;
        float L; // 카메라에서 벽면 표면까지의 거리

        if (Physics.SphereCast(ray.origin, testRadius, ray.direction, out RaycastHit hit, searchDist, _environmentLayer))
        {
            L = hit.distance + testRadius;
        }
        else
        {
            L = searchDist;
        }

        // 2. 수학적 비례식을 통한 목표 거리(D) 산출
        // 공식: D(중심 거리) + R(반지름) = L(벽면 거리), R = D * BaseRatio
        // => D * (1 + BaseRatio) = L  =>  D = L / (1 + BaseRatio)
        float DIdeal = L / (1f + _baseRatio);

        // 3. 플레이어와의 최소 간격(Padding) 보장
        float minD = _playerPadding / Mathf.Max(0.01f, 1f - _baseRatio);
        float D = Mathf.Max(DIdeal, minD);

        // 4. 스케일 계산 및 클램핑
        float scaleMultiplier = D / _initialCenterDistance;
        Vector3 targetScale = _initialObjectScale * scaleMultiplier;
        targetScale.x = Mathf.Clamp(targetScale.x, _minScale, _maxScale);
        targetScale.y = Mathf.Clamp(targetScale.y, _minScale, _maxScale);
        targetScale.z = Mathf.Clamp(targetScale.z, _minScale, _maxScale);

        // 5. 결과 적용 (Lerp를 사용하여 부드럽게 이동/크기 조정)
        Vector3 currentWorldOffset = _heldObject.TransformVector(_localPivotOffset);
        Vector3 targetPivotPos = ray.GetPoint(D) + currentWorldOffset;

        _heldObject.position = Vector3.Lerp(_heldObject.position, targetPivotPos, Time.deltaTime * _smoothing);
        _heldObject.localScale = Vector3.Lerp(_heldObject.localScale, targetScale, Time.deltaTime * _smoothing);
    }

    /// <summary>
    /// 잡은 물체의 물리 및 시각적 상태를 설정합니다.
    /// </summary>
    private void PrepareHeldObject(bool held)
    {
        if (held)
        {
            ApplyTriggerState(_heldObject, true);
            SetCollisionWithPlayer(_heldObject, true);
            ApplyTransparency(_heldObject);
            SetLayerRecursive(_heldObject.gameObject, LayerMask.NameToLayer("Ignore Raycast"));

            if (_heldObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void Release()
    {
        if (_heldObject == null) return;

        RestoreTransparency();
        RestoreTriggerState();
        SetCollisionWithPlayer(_heldObject, false);

        if (_heldObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
            // 구형 커스텀 중력(GravityAttractor)을 위해 유니티 기본 중력은 끕니다.
            rb.useGravity = true;

            // [수정] 물체를 놓는 순간 표면에 맞춰 즉시 뚝! 하고 꺾여버리는(Snap) 로직 제거.
            // 대신 GravityAttractor 쪽의 FixedUpdate에서 Slerp를 통해 부드럽게 지면에 안착(붙도록) 위임합니다.

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.WakeUp();
        }

        SetLayerRecursive(_heldObject.gameObject, LayerMask.NameToLayer("Draggable"));
        _heldObject = null;
    }

    #region Helper Methods (Layer, Transparency, Colliders)

    private void SetLayerRecursive(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, newLayer);
        }
    }

    private void ApplyTransparency(Transform target)
    {
        _originalMaterials.Clear();
        foreach (var rend in target.GetComponentsInChildren<Renderer>())
        {
            _originalMaterials[rend] = rend.materials;
            foreach (var mat in rend.materials)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    c.a = _heldTransparency;
                    mat.color = c;
                }
            }
        }
    }

    private void RestoreTransparency()
    {
        foreach (var entry in _originalMaterials)
        {
            if (entry.Key != null) entry.Key.materials = entry.Value;
        }
        _originalMaterials.Clear();
    }

    private void ApplyTriggerState(Transform target, bool isTrigger)
    {
        _originalTriggerStates.Clear();
        foreach (var col in target.GetComponentsInChildren<Collider>())
        {
            _originalTriggerStates[col] = col.isTrigger;
            col.isTrigger = isTrigger;
        }
    }

    private void RestoreTriggerState()
    {
        foreach (var entry in _originalTriggerStates)
        {
            if (entry.Key != null) entry.Key.isTrigger = entry.Value;
        }
        _originalTriggerStates.Clear();
    }

    private void SetCollisionWithPlayer(Transform target, bool ignore)
    {
        if (_playerCollider == null) return;
        foreach (var col in target.GetComponentsInChildren<Collider>(true))
        {
            Physics.IgnoreCollision(_playerCollider, col, ignore);
        }
    }

    private Bounds GetCombinedBounds(Transform target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(target.position, Vector3.one * 0.5f);

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        return b;
    }

    #endregion
}
