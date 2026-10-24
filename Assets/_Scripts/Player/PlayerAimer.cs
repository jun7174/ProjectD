// 파일 경로: Assets/_Scripts/Player/PlayerAimer.cs
using UnityEngine;

namespace ProjectD.Player
{
    /// <summary>
    /// 플레이어의 조준을 처리한다. 전달받은 조준점을 향해 캐릭터를 수평으로 회전시키고, 정밀 조준 상태를 관리한다.
    /// 입력을 직접 읽지 않으며, 로컬 플레이어는 PlayerInputReader가, 2주차 이후 원격 플레이어는 네트워크 담당이 조준점을 전달한다.
    /// </summary>
    public class PlayerAimer : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("회피 중에는 조준 회전과 정밀 조준을 멈추기 위해 사용한다. 비워 두면 같은 오브젝트에서 찾는다")]
        [SerializeField] private PlayerDodger _dodger;

        [Header("회전")]
        [Tooltip("조준 방향으로 회전하는 속도 (도/초)")]
        [SerializeField] private float _rotationSpeed = 1080f;

        // 조준점이 캐릭터와 이 거리보다 가까우면 방향이 불안정해지므로 이전 방향을 유지한다
        private const float MinAimDistance = 0.1f;

        private Vector3 _aimPoint;
        private Vector3 _aimDirection;
        private bool _isPreciseAimRequested;

        /// <summary>
        /// 현재 조준점 (월드 좌표)
        /// </summary>
        public Vector3 AimPoint => _aimPoint;

        /// <summary>
        /// 캐릭터에서 조준점을 향하는 수평 방향 (길이 1)
        /// </summary>
        public Vector3 AimDirection => _aimDirection;

        /// <summary>
        /// 현재 정밀 조준 중인지 여부. 회피 중에는 정밀 조준 키를 누르고 있어도 false다.
        /// </summary>
        public bool IsAimingPrecise => _isPreciseAimRequested && !IsDodging;

        /// <summary>
        /// 현재 회피 중인지 여부
        /// </summary>
        private bool IsDodging => _dodger != null && _dodger.IsDodging;

        /// <summary>
        /// 참조를 확인하고, 처음 조준 방향을 캐릭터가 바라보는 방향으로 초기화한다.
        /// </summary>
        private void Awake()
        {
            if (_dodger == null)
            {
                _dodger = GetComponent<PlayerDodger>();
            }

            if (_dodger == null)
            {
                Debug.LogWarning($"[{nameof(PlayerAimer)}] PlayerDodger를 찾지 못했습니다. 회피 중 조준 제한이 동작하지 않습니다.", this);
            }

            _aimDirection = GetFlatForward();
            _aimPoint = transform.position + _aimDirection;
        }

        /// <summary>
        /// 조준점을 설정한다.
        /// </summary>
        /// <param name="worldAimPoint">조준할 월드 좌표</param>
        public void SetAimPoint(Vector3 worldAimPoint)
        {
            _aimPoint = worldAimPoint;
        }

        /// <summary>
        /// 정밀 조준 요청 여부를 설정한다.
        /// </summary>
        /// <param name="isRequested">정밀 조준 키를 누르고 있으면 true</param>
        public void SetPreciseAimRequested(bool isRequested)
        {
            _isPreciseAimRequested = isRequested;
        }

        /// <summary>
        /// 매 프레임 조준 방향을 갱신하고, 회피 중이 아니면 조준 방향으로 회전한다.
        /// </summary>
        private void Update()
        {
            UpdateAimDirection();

            // 회피 중에는 PlayerDodger가 정한 회피 방향을 유지한다
            if (IsDodging)
            {
                return;
            }

            RotateTowardAimDirection();
        }

        /// <summary>
        /// 현재 위치에서 조준점을 향하는 수평 방향을 계산한다. 조준점이 너무 가까우면 이전 방향을 유지한다.
        /// </summary>
        private void UpdateAimDirection()
        {
            Vector3 toAimPoint = _aimPoint - transform.position;
            toAimPoint.y = 0f;

            if (toAimPoint.sqrMagnitude < MinAimDistance * MinAimDistance)
            {
                return;
            }

            _aimDirection = toAimPoint.normalized;
        }

        /// <summary>
        /// 캐릭터를 조준 방향으로 회전 속도에 맞춰 회전시킨다.
        /// </summary>
        private void RotateTowardAimDirection()
        {
            Quaternion targetRotation = Quaternion.LookRotation(_aimDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }

        /// <summary>
        /// 캐릭터가 바라보는 방향을 수평으로 만든 값을 반환한다.
        /// </summary>
        /// <returns>수평 앞 방향 (길이 1)</returns>
        private Vector3 GetFlatForward()
        {
            Vector3 forward = transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0f ? forward.normalized : Vector3.forward;
        }
    }
}
