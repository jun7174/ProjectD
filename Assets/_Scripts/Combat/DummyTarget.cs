// 파일 경로: Assets/_Scripts/Combat/DummyTarget.cs
using System.Collections;
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 사격 테스트용 더미 적. 체력이 0이 되면 사라졌다가 일정 시간 뒤 체력을 회복하고 다시 나타난다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class DummyTarget : MonoBehaviour
    {
        [Tooltip("사라진 뒤 다시 나타날 때까지 걸리는 시간 (초)")]
        [Min(0f)]
        [SerializeField] private float _respawnDelay = 2f;

        private Health _health;
        private Renderer[] _renderers;
        private Collider[] _colliders;

        /// <summary>
        /// 체력과, 숨기고 보일 렌더러와 콜라이더를 캐싱한다.
        /// </summary>
        private void Awake()
        {
            _health = GetComponent<Health>();
            _renderers = GetComponentsInChildren<Renderer>();
            _colliders = GetComponentsInChildren<Collider>();
        }

        /// <summary>
        /// 사망 이벤트를 구독한다.
        /// </summary>
        private void OnEnable()
        {
            _health.OnDied += HandleDied;
        }

        /// <summary>
        /// 사망 이벤트 구독을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            _health.OnDied -= HandleDied;
        }

        /// <summary>
        /// 죽으면 재등장 과정을 시작한다.
        /// </summary>
        private void HandleDied()
        {
            StartCoroutine(RespawnRoutine());
        }

        /// <summary>
        /// 모습과 충돌을 끄고, 일정 시간 뒤 체력을 회복해서 다시 켠다.
        /// </summary>
        /// <returns>코루틴 진행용 열거자</returns>
        private IEnumerator RespawnRoutine()
        {
            SetVisible(false);

            yield return new WaitForSeconds(_respawnDelay);

            _health.ResetHealth();
            SetVisible(true);
        }

        /// <summary>
        /// 렌더러와 콜라이더를 함께 켜거나 끈다. 꺼진 동안에는 투사체가 통과한다.
        /// </summary>
        /// <param name="isVisible">보이게 하려면 true</param>
        private void SetVisible(bool isVisible)
        {
            foreach (Renderer targetRenderer in _renderers)
            {
                targetRenderer.enabled = isVisible;
            }

            foreach (Collider targetCollider in _colliders)
            {
                targetCollider.enabled = isVisible;
            }
        }
    }
}
