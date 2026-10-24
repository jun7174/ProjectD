// 파일 경로: Assets/_Scripts/Combat/Health.cs
using System;
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 체력을 관리하고 데미지를 받는다. 체력이 0이 되면 사망 이벤트를 보낸다.
    /// 피격 연출, 데미지 숫자, 사망 처리는 이 컴포넌트의 이벤트를 구독해서 각자 처리한다.
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [Tooltip("최대 체력")]
        [Min(1f)]
        [SerializeField] private float _maxHealth = 100f;

        private float _currentHealth;

        /// <summary>
        /// 데미지를 받았을 때 호출된다. 첫 번째 값은 데미지 양, 두 번째 값은 맞은 위치다.
        /// </summary>
        public event Action<float, Vector3> OnDamaged;

        /// <summary>
        /// 체력이 0이 되었을 때 한 번 호출된다.
        /// </summary>
        public event Action OnDied;

        /// <summary>
        /// 현재 체력
        /// </summary>
        public float CurrentHealth => _currentHealth;

        /// <summary>
        /// 최대 체력
        /// </summary>
        public float MaxHealth => _maxHealth;

        /// <summary>
        /// 체력이 0이 되었는지 여부
        /// </summary>
        public bool IsDead => _currentHealth <= 0f;

        /// <summary>
        /// 시작할 때 체력을 가득 채운다.
        /// </summary>
        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        /// <summary>
        /// 데미지를 받아 체력을 줄인다. 이미 죽었거나 데미지가 0 이하면 무시한다.
        /// </summary>
        /// <param name="amount">데미지 양</param>
        /// <param name="hitPoint">맞은 위치 (월드 좌표)</param>
        public void TakeDamage(float amount, Vector3 hitPoint)
        {
            if (IsDead || amount <= 0f)
            {
                return;
            }

            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            OnDamaged?.Invoke(amount, hitPoint);

            if (IsDead)
            {
                OnDied?.Invoke();
            }
        }

        /// <summary>
        /// 체력을 최대로 되돌린다. 더미 적 재등장 등에 사용한다.
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
        }
    }
}
