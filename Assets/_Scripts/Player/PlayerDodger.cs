// 파일 경로: Assets/_Scripts/Player/PlayerDodger.cs
using System;
using UnityEngine;

namespace ProjectD.Player
{
    /// <summary>
    /// 플레이어의 회피를 처리한다. 스태미나를 소모해 정해진 시간 동안 정해진 거리만큼 이동하고,
    /// 회피 앞부분 일정 시간 동안 무적 상태가 된다.
    /// 무적 여부는 외부에서 IsInvincible로 조회한다 (3주차 피격자 최종 확인에서 사용).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerDodger : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("회피에 사용할 스태미나. 비워 두면 같은 오브젝트에서 찾는다")]
        [SerializeField] private PlayerStamina _stamina;

        [Header("회피 수치")]
        [Tooltip("회피 1회에 필요한 스태미나")]
        [SerializeField] private float _staminaCost = 25f;

        [Tooltip("회피로 이동하는 거리 (m)")]
        [SerializeField] private float _distance = 4f;

        [Tooltip("회피 전체 시간 (초)")]
        [Min(0.01f)]
        [SerializeField] private float _duration = 0.4f;

        [Tooltip("회피 시작부터 무적이 유지되는 시간 (초). 회피 전체 시간보다 길 수 없다")]
        [Min(0f)]
        [SerializeField] private float _invincibleDuration = 0.25f;

        // 이 값보다 작은 입력은 이동 입력이 없는 것으로 본다
        private const float MoveInputDeadZone = 0.01f;

        private CharacterController _characterController;
        private Vector3 _dodgeDirection;
        private float _elapsedTime;

        /// <summary>
        /// 회피가 시작될 때 호출된다. 재장전 취소 등 회피에 반응해야 하는 기능이 구독한다.
        /// </summary>
        public event Action OnDodgeStarted;

        /// <summary>
        /// 현재 회피 중인지 여부
        /// </summary>
        public bool IsDodging { get; private set; }

        /// <summary>
        /// 현재 무적 상태인지 여부. 회피 시작 후 무적 시간 안에만 true다.
        /// </summary>
        public bool IsInvincible => IsDodging && _elapsedTime < _invincibleDuration;

        /// <summary>
        /// 필요한 컴포넌트를 캐싱하고 필수 참조를 확인한다.
        /// </summary>
        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            if (_stamina == null)
            {
                _stamina = GetComponent<PlayerStamina>();
            }

            if (_stamina == null)
            {
                Debug.LogWarning($"[{nameof(PlayerDodger)}] PlayerStamina를 찾지 못했습니다. 스태미나가 없으면 회피할 수 없습니다.", this);
            }
        }

        /// <summary>
        /// 인스펙터 값이 바뀌면 무적 시간이 회피 전체 시간을 넘지 않도록 맞춘다.
        /// </summary>
        private void OnValidate()
        {
            _invincibleDuration = Mathf.Min(_invincibleDuration, _duration);
        }

        /// <summary>
        /// 회피를 시도한다. 이미 회피 중이거나 스태미나가 부족하면 실패한다.
        /// </summary>
        /// <param name="moveInput">현재 이동 입력. 입력이 없으면 바라보는 방향으로 회피한다</param>
        /// <returns>회피를 시작했으면 true</returns>
        public bool TryStartDodge(Vector2 moveInput)
        {
            if (IsDodging)
            {
                return false;
            }

            if (_stamina == null || !_stamina.TryConsume(_staminaCost))
            {
                return false;
            }

            _dodgeDirection = CalculateDodgeDirection(moveInput);
            _elapsedTime = 0f;
            IsDodging = true;

            // 회피 방향을 바라보게 한다. 회피가 끝나면 PlayerAimer가 다시 조준 방향으로 돌린다
            transform.rotation = Quaternion.LookRotation(_dodgeDirection);

            OnDodgeStarted?.Invoke();
            return true;
        }

        /// <summary>
        /// 회피 중이면 회피 방향으로 이동시키고, 회피 시간이 끝나면 회피를 종료한다.
        /// </summary>
        private void Update()
        {
            if (!IsDodging)
            {
                return;
            }

            // 마지막 프레임에 회피 시간을 넘겨 더 멀리 가지 않도록 남은 시간만큼만 이동한다
            float remainingTime = _duration - _elapsedTime;
            float stepTime = Mathf.Min(Time.deltaTime, remainingTime);
            float speed = _distance / _duration;

            _characterController.Move(_dodgeDirection * (speed * stepTime));
            _elapsedTime += stepTime;

            if (_elapsedTime >= _duration)
            {
                IsDodging = false;
            }
        }

        /// <summary>
        /// 이동 입력이 있으면 그 방향, 없으면 캐릭터가 바라보는 수평 방향을 회피 방향으로 정한다.
        /// </summary>
        /// <param name="moveInput">현재 이동 입력</param>
        /// <returns>월드 기준 수평 회피 방향 (길이 1)</returns>
        private Vector3 CalculateDodgeDirection(Vector2 moveInput)
        {
            if (moveInput.sqrMagnitude > MoveInputDeadZone)
            {
                return new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            }

            Vector3 forward = transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0f ? forward.normalized : Vector3.forward;
        }
    }
}
