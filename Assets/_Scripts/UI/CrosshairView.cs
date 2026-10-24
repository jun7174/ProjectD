// 파일 경로: Assets/_Scripts/UI/CrosshairView.cs
using ProjectD.Combat;
using ProjectD.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectD.UI
{
    /// <summary>
    /// 마우스 위치에 원형 조준점을 그리고, 현재 탄퍼짐 크기에 맞춰 원의 크기를 바꾼다.
    /// 조준점은 매 프레임 바뀌는 값을 그리는 UI라 이벤트 대신 읽기 전용 값을 매 프레임 읽는다.
    /// 게임 로직은 이 UI를 모른다. 로컬 플레이어 전용이다.
    /// Screen Space - Overlay 캔버스 아래의 이미지에 붙인다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class CrosshairView : MonoBehaviour
    {
        [Header("참조 (비워 두면 씬에서 찾는다)")]
        [SerializeField] private PlayerAimer _aimer;
        [SerializeField] private PlayerWeaponSpread _weaponSpread;
        [SerializeField] private Camera _camera;

        [Header("표시")]
        [Tooltip("조준점 원의 최소 지름 (캔버스 단위)")]
        [Min(0f)]
        [SerializeField] private float _minDiameter = 24f;

        [Tooltip("시스템 마우스 커서를 숨길지 여부")]
        [SerializeField] private bool _hideSystemCursor = true;

        private RectTransform _rectTransform;
        private Canvas _canvas;

        /// <summary>
        /// 컴포넌트를 캐싱하고 참조를 찾는다.
        /// </summary>
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();

            if (_aimer == null)
            {
                _aimer = FindFirstObjectByType<PlayerAimer>();
            }

            if (_weaponSpread == null)
            {
                _weaponSpread = FindFirstObjectByType<PlayerWeaponSpread>();
            }

            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_aimer == null || _camera == null)
            {
                Debug.LogWarning($"[{nameof(CrosshairView)}] PlayerAimer 또는 카메라를 찾지 못했습니다. 조준점 크기가 바뀌지 않습니다.", this);
            }
        }

        /// <summary>
        /// 켜질 때 시스템 마우스 커서를 숨긴다.
        /// </summary>
        private void OnEnable()
        {
            if (_hideSystemCursor)
            {
                Cursor.visible = false;
            }
        }

        /// <summary>
        /// 꺼질 때 시스템 마우스 커서를 다시 보이게 한다.
        /// </summary>
        private void OnDisable()
        {
            Cursor.visible = true;
        }

        /// <summary>
        /// 매 프레임 조준점을 마우스 위치로 옮기고 크기를 갱신한다.
        /// </summary>
        private void LateUpdate()
        {
            UpdatePosition();
            UpdateSize();
        }

        /// <summary>
        /// 조준점을 마우스 화면 위치로 옮긴다.
        /// 카메라 이동과 상관없이 커서처럼 바로 따라오도록 월드 조준점 대신 마우스 위치를 쓴다.
        /// </summary>
        private void UpdatePosition()
        {
            if (Mouse.current == null)
            {
                return;
            }

            _rectTransform.position = Mouse.current.position.ReadValue();
        }

        /// <summary>
        /// 탄퍼짐 각도를 조준점 거리에서의 실제 반지름으로 바꾸고, 화면 크기로 변환해서 원의 지름을 정한다.
        /// </summary>
        private void UpdateSize()
        {
            if (_aimer == null || _camera == null || _weaponSpread == null)
            {
                _rectTransform.sizeDelta = Vector2.one * _minDiameter;
                return;
            }

            Vector3 aimPoint = _aimer.AimPoint;
            Vector3 toAimPoint = aimPoint - _aimer.transform.position;
            toAimPoint.y = 0f;

            // 조준점까지 거리에서 탄이 벗어날 수 있는 최대 폭
            float worldRadius = Mathf.Tan(_weaponSpread.CurrentSpreadAngle * Mathf.Deg2Rad) * toAimPoint.magnitude;

            Vector3 centerOnScreen = _camera.WorldToScreenPoint(aimPoint);
            Vector3 edgeOnScreen = _camera.WorldToScreenPoint(aimPoint + _camera.transform.right * worldRadius);
            float screenRadius = Vector2.Distance(centerOnScreen, edgeOnScreen);

            float canvasScale = _canvas != null ? _canvas.scaleFactor : 1f;
            float diameter = Mathf.Max(_minDiameter, screenRadius * 2f / canvasScale);
            _rectTransform.sizeDelta = Vector2.one * diameter;
        }
    }
}
