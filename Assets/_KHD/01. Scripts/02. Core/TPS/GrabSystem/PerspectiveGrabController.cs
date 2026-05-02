/// <summary>
/// 원근법을 이용해 크기를 조절하며 물건을 잡고 놓는 기믹 (KHD 전용화)
/// PlayerActor 시스템의 LocalInputManager(IsGrabPressed)와 결합되었습니다.
/// </summary>

using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KHD
{
    public class PerspectiveGrabController : MonoBehaviour
    {
        [Title("필수 참조")]
        [SerializeField, Tooltip("입력을 받아올 KHD 액터")] private GravityPlayerActor _actor;
        [SerializeField, Tooltip("보여지는 뷰포트 기준 카메라")] private Camera _playerCamera;
        [SerializeField, Tooltip("조작할 물체들이 속한 레이어")] private LayerMask _draggableLayer;

        [Title("잡기 설정")]
        [SerializeField, Tooltip("최대 상호작용 거리")] private float _maxGrabDistance = 50f;
        [SerializeField, Tooltip("물체를 잡았을 때 기준이 되는 거리")] private float _standardGrabDistance = 2f;
        [SerializeField, Tooltip("가짜 원근감을 위해 물체를 따라오게 할 보간 속도")] private float _smoothing = 10f;

        [Title("투명도 설정")]
        [SerializeField, Tooltip("잡은 물체에 씌울 반투명 머테리얼 (선택사항)")] private Material _transparentMaterial;

        // 내부 상태 변수들
        private Transform _heldObject;
        private Rigidbody _heldRigidbody;
        private float _baseRatio;
        private float _initialRadius;
        private float _initialCenterDistance;
        private Vector3 _initialObjectScale;
        private Vector3 _localPivotOffset;
        private float _currentDistance;

        // 복원 데이터 보관
        private Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();
        private Dictionary<Collider, bool> _originalTriggerStates = new Dictionary<Collider, bool>();
        private bool _wasGrabPressed = false;

        private void Start()
        {
            if (_playerCamera == null) _playerCamera = Camera.main;
            if (_actor == null) _actor = GetComponent<GravityPlayerActor>();
        }

        private void Update()
        {
            if (_actor == null) return;

            // "버튼을 갓 눌렀을 때" (Press)
            if (_actor.IsGrabPressed && !_wasGrabPressed)
            {
                if (_heldObject == null) TryGrab();
                else Release();
            }

            // 상태 업데이트
            _wasGrabPressed = _actor.IsGrabHeld;

            // 물체를 들고 있는 동안 실시간 좌표 갱신
            if (_heldObject != null)
            {
                UpdateObjectTransform();
            }
        }

        #region 핵심 로직 (Grab, Update, Release)

        private void TryGrab()
        {
            Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out RaycastHit hit, _maxGrabDistance, _draggableLayer))
            {
                _heldObject = hit.transform;
                _heldObject.rotation = _playerCamera.transform.rotation;

                Vector3 currentScale = _heldObject.localScale;
                _initialObjectScale = new Vector3(
                    Mathf.Max(currentScale.x, 0.01f),
                    Mathf.Max(currentScale.y, 0.01f),
                    Mathf.Max(currentScale.z, 0.01f)
                );

                Bounds b = GetCombinedBounds(_heldObject);
                _initialRadius = b.extents.magnitude;

                _initialCenterDistance = _standardGrabDistance + _initialRadius;
                _baseRatio = _initialRadius / _initialCenterDistance;

                _currentDistance = _initialCenterDistance;

                _localPivotOffset = _heldObject.InverseTransformPoint(b.center);

                if (_heldObject.TryGetComponent(out _heldRigidbody))
                {
                    _heldRigidbody.isKinematic = true;
                }

                SetLayerRecursive(_heldObject.gameObject, LayerMask.NameToLayer("Ignore Raycast"));
                SaveAndSetTriggerState(true);
                SetCollisionWithPlayer(_heldObject, true);

                if (_transparentMaterial != null) ApplyTransparency(_heldObject);
            }
        }

        private void UpdateObjectTransform()
        {
            Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            float hitDistance = _maxGrabDistance;
            int layerMask = ~LayerMask.GetMask("Ignore Raycast", "Player");

            if (Physics.SphereCast(ray, _initialRadius * 0.5f, out RaycastHit hitInfo, _maxGrabDistance, layerMask))
            {
                hitDistance = hitInfo.distance;
            }

            float smoothDistance = Mathf.Lerp(_currentDistance, hitDistance, Time.deltaTime * _smoothing);
            _currentDistance = smoothDistance;

            float targetRadius = smoothDistance * _baseRatio;
            float scaleMultiplier = targetRadius / _initialRadius;

            Vector3 targetScale = _initialObjectScale * scaleMultiplier;
            targetScale.x = Mathf.Clamp(targetScale.x, 0.05f, 100f);
            targetScale.y = Mathf.Clamp(targetScale.y, 0.05f, 100f);
            targetScale.z = Mathf.Clamp(targetScale.z, 0.05f, 100f);

            _heldObject.localScale = targetScale;
            _heldObject.rotation = Quaternion.Slerp(_heldObject.rotation, _playerCamera.transform.rotation, Time.deltaTime * _smoothing);

            Vector3 currentScaledPivotOffset = _heldObject.TransformPoint(_localPivotOffset) - _heldObject.position;
            Vector3 targetPivotPos = ray.origin + ray.direction * smoothDistance;
            _heldObject.position = targetPivotPos - currentScaledPivotOffset;
        }

        private void Release()
        {
            if (_heldObject == null) return;

            RestoreTransparency();
            SaveAndSetTriggerState(false);
            SetCollisionWithPlayer(_heldObject, false);

            if (_heldRigidbody != null)
            {
                _heldRigidbody.isKinematic = false;
                // 구형 커스텀 중력(GravityAttractor)을 위해 유니티 기본 중력은 끕니다.
                _heldRigidbody.useGravity = false;

                _heldRigidbody.linearVelocity = Vector3.zero;
                _heldRigidbody.angularVelocity = Vector3.zero;
                _heldRigidbody.WakeUp();
            }

            SetLayerRecursive(_heldObject.gameObject, LayerMask.NameToLayer("Draggable"));
            _heldObject = null;
            _heldRigidbody = null;
        }

        #endregion

        #region 유틸리티
        private Bounds GetCombinedBounds(Transform parent)
        {
            Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(parent.position, Vector3.one);

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }

        private void SetLayerRecursive(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursive(child.gameObject, newLayer);
            }
        }

        private void SaveAndSetTriggerState(bool isTrigger)
        {
            Collider[] colliders = _heldObject.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                if (isTrigger) _originalTriggerStates[col] = col.isTrigger;
                col.isTrigger = isTrigger;
            }
        }

        private void SetCollisionWithPlayer(Transform obj, bool ignore)
        {
            Collider[] objColliders = obj.GetComponentsInChildren<Collider>();
            Collider playerCollider = _actor.GetComponent<Collider>();

            if (playerCollider == null) return;

            foreach (var col in objColliders)
            {
                Physics.IgnoreCollision(playerCollider, col, ignore);
            }
        }

        private void ApplyTransparency(Transform target)
        {
            _originalMaterials.Clear();
            foreach (var rend in target.GetComponentsInChildren<Renderer>())
            {
                _originalMaterials[rend] = rend.materials;
                Material[] newMats = new Material[rend.materials.Length];
                for (int i = 0; i < newMats.Length; i++) newMats[i] = _transparentMaterial;
                rend.materials = newMats;
            }
        }

        private void RestoreTransparency()
        {
            if (_transparentMaterial == null) return;
            foreach (var rend in _heldObject.GetComponentsInChildren<Renderer>())
            {
                if (_originalMaterials.TryGetValue(rend, out Material[] origMats))
                {
                    rend.materials = origMats;
                }
            }
            _originalMaterials.Clear();
        }
        #endregion
    }
}