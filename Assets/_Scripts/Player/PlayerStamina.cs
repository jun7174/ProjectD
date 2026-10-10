// 파일 경로: Assets/_Scripts/Player/PlayerStamina.cs
using System;
using UnityEngine;

namespace ProjectD.Player
{
    /// <summary>
    /// 플레이어 스태미나를 관리한다. 소모, 회복 대기, 회복을 처리하고 값이 바뀌면 이벤트로 알린다.
    /// 달리기, 회피, 근접 공격이 이 컴포넌트를 통해 스태미나를 사용한다.
    /// 로컬 전용 값이며 네트워크로 동기화하지 않는다.
    /// </summary>
    public class PlayerStamina : MonoBehaviour
    {
        [Tooltip("최대 스태미나")]
        [SerializeField] private float _maxStamina = 100f;

        [Tooltip("초당 회복량")]
        [SerializeField] private float _regenPerSecond = 20f;

        [Tooltip("마지막으로 소모한 뒤 회복이 시작되기까지 기다리는 시간 (초)")]
        [SerializeField] private float _regenDelay = 1f;

        private float _currentStamina;
        private float _lastConsumeTime;

        /// <summary>
        /// 스태미나 값이 바뀌었을 때 호출된다. 첫 번째 값은 현재 스태미나, 두 번째 값은 최대 스태미나다.
        /// UI는 이 이벤트를 구독해서 표시만 한다.
        /// </summary>
        public event Action<float, float> OnStaminaChanged;

        /// <summary>
        /// 현재 스태미나
        /// </summary>
        public float CurrentStamina => _currentStamina;

        /// <summary>
        /// 최대 스태미나
        /// </summary>
        public float MaxStamina => _maxStamina;

        /// <summary>
        /// 스태미나가 조금이라도 남아 있는지 여부
        /// </summary>
        public bool HasStamina => _currentStamina > 0f;

        /// <summary>
        /// 시작할 때 스태미나를 가득 채운다.
        /// </summary>
        private void Awake()
        {
            _currentStamina = _maxStamina;
        }

        /// <summary>
        /// 매 프레임 회복 조건을 확인하고 스태미나를 회복한다.
        /// </summary>
        private void Update()
        {
            Regenerate();
        }

        /// <summary>
        /// 남은 양과 관계없이 스태미나를 소모한다. 달리기처럼 계속 소모하는 행동에 사용한다.
        /// 남은 양보다 많이 소모하면 0이 된다.
        /// </summary>
        /// <param name="amount">소모할 양</param>
        public void Drain(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            _lastConsumeTime = Time.time;
            SetStamina(_currentStamina - amount);
        }

        /// <summary>
        /// 스태미나가 충분할 때만 소모한다. 회피, 근접 공격처럼 한 번에 일정량이 필요한 행동에 사용한다.
        /// </summary>
        /// <param name="amount">소모할 양</param>
        /// <returns>소모에 성공하면 true, 스태미나가 부족하면 false</returns>
        public bool TryConsume(float amount)
        {
            if (amount <= 0f)
            {
                return true;
            }

            if (_currentStamina < amount)
            {
                return false;
            }

            _lastConsumeTime = Time.time;
            SetStamina(_currentStamina - amount);
            return true;
        }

        /// <summary>
        /// 마지막 소모 후 회복 대기 시간이 지났으면 스태미나를 회복한다.
        /// </summary>
        private void Regenerate()
        {
            if (_currentStamina >= _maxStamina)
            {
                return;
            }

            if (Time.time - _lastConsumeTime < _regenDelay)
            {
                return;
            }

            SetStamina(_currentStamina + _regenPerSecond * Time.deltaTime);
        }

        /// <summary>
        /// 스태미나를 0과 최대값 사이로 제한해서 설정하고, 값이 바뀌었으면 이벤트를 호출한다.
        /// </summary>
        /// <param name="value">설정할 스태미나 값</param>
        private void SetStamina(float value)
        {
            float clampedValue = Mathf.Clamp(value, 0f, _maxStamina);

            if (Mathf.Approximately(clampedValue, _currentStamina))
            {
                return;
            }

            _currentStamina = clampedValue;
            OnStaminaChanged?.Invoke(_currentStamina, _maxStamina);
        }
    }
}
