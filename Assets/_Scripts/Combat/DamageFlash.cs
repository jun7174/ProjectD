// 파일 경로: Assets/_Scripts/Combat/DamageFlash.cs
using System.Collections;
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 데미지를 받으면 렌더러 색을 잠깐 바꿔서 맞았다는 것을 보여준다.
    /// 머티리얼을 복제하지 않도록 MaterialPropertyBlock으로 색만 덮어쓴다.
    /// </summary>
    public class DamageFlash : MonoBehaviour
    {
        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private Health _health;
        [SerializeField] private Renderer _targetRenderer;

        [Header("표시")]
        [Tooltip("맞았을 때 표시할 색")]
        [SerializeField] private Color _flashColor = Color.white;

        [Tooltip("색이 유지되는 시간 (초)")]
        [Min(0f)]
        [SerializeField] private float _flashDuration = 0.08f;

        private MaterialPropertyBlock _propertyBlock;
        private Coroutine _flashRoutine;

        /// <summary>
        /// 참조를 찾고 프로퍼티 블록을 만든다.
        /// </summary>
        private void Awake()
        {
            if (_health == null)
            {
                _health = GetComponent<Health>();
            }

            if (_targetRenderer == null)
            {
                _targetRenderer = GetComponent<Renderer>();
            }

            if (_health == null || _targetRenderer == null)
            {
                Debug.LogWarning($"[{nameof(DamageFlash)}] Health 또는 Renderer를 찾지 못했습니다. 피격 색 표시가 동작하지 않습니다.", this);
            }

            _propertyBlock = new MaterialPropertyBlock();
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
        /// 데미지 이벤트 구독을 해제하고 원래 색으로 되돌린다.
        /// </summary>
        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDamaged -= HandleDamaged;
            }

            if (_targetRenderer != null)
            {
                _targetRenderer.SetPropertyBlock(null);
            }

            _flashRoutine = null;
        }

        /// <summary>
        /// 데미지를 받으면 깜빡임을 처음부터 다시 시작한다.
        /// </summary>
        /// <param name="amount">데미지 양</param>
        /// <param name="hitPoint">맞은 위치</param>
        private void HandleDamaged(float amount, Vector3 hitPoint)
        {
            if (_targetRenderer == null)
            {
                return;
            }

            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
            }

            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        /// <summary>
        /// 색을 바꾸고 일정 시간 뒤 원래 색으로 되돌린다.
        /// </summary>
        /// <returns>코루틴 진행용 열거자</returns>
        private IEnumerator FlashRoutine()
        {
            _propertyBlock.SetColor(ShaderPropertyIds.BaseColor, _flashColor);
            _targetRenderer.SetPropertyBlock(_propertyBlock);

            yield return new WaitForSeconds(_flashDuration);

            _targetRenderer.SetPropertyBlock(null);
            _flashRoutine = null;
        }
    }
}
