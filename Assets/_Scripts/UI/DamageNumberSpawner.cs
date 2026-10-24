// 파일 경로: Assets/_Scripts/UI/DamageNumberSpawner.cs
using ProjectD.Combat;
using UnityEngine;
using UnityEngine.Pool;

namespace ProjectD.UI
{
    /// <summary>
    /// Health의 데미지 이벤트를 구독해서 맞은 위치에 데미지 숫자를 띄운다.
    /// 숫자는 풀에서 재사용한다. 데미지를 받는 오브젝트에 함께 붙인다.
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("데미지 이벤트를 받을 대상. 비워 두면 같은 오브젝트에서 찾는다")]
        [SerializeField] private Health _health;

        [Tooltip("띄울 데미지 숫자 프리팹")]
        [SerializeField] private DamageNumber _damageNumberPrefab;

        [Header("표시 위치")]
        [Tooltip("맞은 위치에서 위로 띄울 높이 (m)")]
        [SerializeField] private float _heightOffset = 0.5f;

        [Tooltip("숫자끼리 겹치지 않도록 좌우로 흩뿌리는 최대 거리 (m)")]
        [Min(0f)]
        [SerializeField] private float _randomHorizontalOffset = 0.3f;

        private ObjectPool<DamageNumber> _pool;
        private Transform _cameraTransform;

        /// <summary>
        /// 참조를 확인하고 숫자 풀을 만든다.
        /// </summary>
        private void Awake()
        {
            if (_health == null)
            {
                _health = GetComponent<Health>();
            }

            if (_health == null || _damageNumberPrefab == null)
            {
                Debug.LogWarning($"[{nameof(DamageNumberSpawner)}] Health 또는 데미지 숫자 프리팹이 비어 있습니다. 데미지 숫자가 표시되지 않습니다.", this);
            }

            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            _pool = new ObjectPool<DamageNumber>(CreateNumber, OnGetNumber, OnReleaseNumber, OnDestroyNumber);
        }

        /// <summary>
        /// 데미지 이벤트를 구독한다.
        /// </summary>
        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnDamaged += HandleDamaged;
            }
        }

        /// <summary>
        /// 데미지 이벤트 구독을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDamaged -= HandleDamaged;
            }
        }

        /// <summary>
        /// 맞은 위치 근처에 데미지 숫자를 띄운다.
        /// </summary>
        /// <param name="amount">데미지 양</param>
        /// <param name="hitPoint">맞은 위치</param>
        private void HandleDamaged(float amount, Vector3 hitPoint)
        {
            if (_damageNumberPrefab == null)
            {
                return;
            }

            Vector2 randomOffset = Random.insideUnitCircle * _randomHorizontalOffset;
            Vector3 position = hitPoint + new Vector3(randomOffset.x, _heightOffset, randomOffset.y);

            DamageNumber damageNumber = _pool.Get();
            damageNumber.Play(amount, position, _cameraTransform, _pool.Release);
        }

        /// <summary>
        /// 풀이 비었을 때 새 숫자를 만든다.
        /// </summary>
        /// <returns>새로 만든 데미지 숫자</returns>
        private DamageNumber CreateNumber()
        {
            return Instantiate(_damageNumberPrefab);
        }

        /// <summary>
        /// 풀에서 꺼낼 때 숫자를 켠다.
        /// </summary>
        /// <param name="damageNumber">꺼낸 숫자</param>
        private void OnGetNumber(DamageNumber damageNumber)
        {
            damageNumber.gameObject.SetActive(true);
        }

        /// <summary>
        /// 풀로 돌아올 때 숫자를 끈다.
        /// </summary>
        /// <param name="damageNumber">돌아온 숫자</param>
        private void OnReleaseNumber(DamageNumber damageNumber)
        {
            damageNumber.gameObject.SetActive(false);
        }

        /// <summary>
        /// 풀이 가득 찬 상태에서 돌아온 숫자를 삭제한다.
        /// </summary>
        /// <param name="damageNumber">삭제할 숫자</param>
        private void OnDestroyNumber(DamageNumber damageNumber)
        {
            Destroy(damageNumber.gameObject);
        }
    }
}
