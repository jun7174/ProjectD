// 파일 경로: Assets/_Scripts/Combat/Health.cs
using System;
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 체력을 관리하고 데미지를 받는다. 체력이 0이 되면 사망 이벤트를 보낸다.
    /// 피격 연출, 데미지 숫자, 사망 처리는 이 컴포넌트의 이벤트를 구독해서 각자 처리한다.
    /// 네트워크에서는 마스터가 정한 데미지를 모든 클라이언트가 같은 순서로 TakeDamage에 넣어 체력을 맞추고,
    /// 중도 참가자는 SetMaxHealth와 SetCurrentHealth로 현재 값을 한 번에 맞춘다.
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [Tooltip("최대 체력. 적은 실행 중에 EnemyData와 인원 배율로 다시 정해진다")]
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
        /// 체력을 주어진 값으로 바로 맞춘다. 중도 참가자가 마스터에게서 현재 체력을 받았을 때 쓴다.
        /// 데미지 연출은 하지 않고, 살아 있다가 이 값으로 0이 되면 사망 이벤트만 보낸다.
        /// </summary>
        /// <param name="currentHealth">맞출 체력 값. 0과 최대 체력 사이로 제한한다</param>
        public void SetCurrentHealth(float currentHealth)
        {
            bool wasDead = IsDead;
            _currentHealth = Mathf.Clamp(currentHealth, 0f, _maxHealth);

            if (!wasDead && IsDead)
            {
                OnDied?.Invoke();
            }
        }

        /// <summary>
        /// 최대 체력을 바꾼다. 남은 체력 비율은 유지한다 (절반 남았으면 바뀐 최대 체력의 절반).
        /// 인원이 바뀌어 적 체력 배율이 달라질 때 쓴다. 죽어 있으면 0을 유지한다.
        /// </summary>
        /// <param name="maxHealth">새 최대 체력. 1보다 작으면 1로 맞춘다</param>
        public void SetMaxHealth(float maxHealth)
        {
            float ratio = _currentHealth / _maxHealth;
            _maxHealth = Mathf.Max(1f, maxHealth);
            _currentHealth = _maxHealth * ratio;
        }

        /// <summary>
        /// 체력을 최대로 되돌린다. 더미 적 재등장, 플레이어 임시 회복에 쓴다.
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
        }
    }
}
