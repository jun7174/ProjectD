// 파일 경로: Assets/_Scripts/AI/EnemyTelegraphView.cs
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.AI
{
    /// <summary>
    /// 공격 예고 동작을 색과 크기 변화로 보여 준다.
    /// EnemyStatus의 예고 여부만 읽으므로 마스터와 다른 클라이언트 모두에서 같게 보인다.
    /// 예고가 바뀌는 순간에만 색과 크기를 바꾼다.
    /// 적 프리팹 루트에 붙이고, 모든 클라이언트에서 켜 둔다.
    /// </summary>
    public class EnemyTelegraphView : MonoBehaviour
    {
        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private EnemyStatus _status;
        [SerializeField] private Renderer _targetRenderer;

        [Tooltip("크기를 바꿀 대상. 비워 두면 렌더러가 붙은 오브젝트")]
        [SerializeField] private Transform _scaleTarget;

        [Header("표시")]
        [Tooltip("예고 중 색")]
        [SerializeField] private Color _telegraphColor = Color.yellow;

        [Tooltip("예고 중 크기 배율")]
        [Min(1f)]
        [SerializeField] private float _scaleMultiplier = 1.15f;

        private MaterialPropertyBlock _propertyBlock;
        private Vector3 _baseScale;
        private bool _isShowing;

        /// <summary>
        /// 참조를 찾고 원래 크기를 기억한다. 필수 참조가 비어 있으면 경고 후 이 컴포넌트를 끈다.
        /// </summary>
        private void Awake()
        {
            if (_status == null && !TryGetComponent(out _status))
            {
                Debug.LogWarning($"[{nameof(EnemyTelegraphView)}] EnemyStatus를 찾지 못했습니다.", this);
            }

            if (_targetRenderer == null && !TryGetComponent(out _targetRenderer))
            {
                Debug.LogWarning($"[{nameof(EnemyTelegraphView)}] Renderer를 찾지 못했습니다. 인스펙터에서 연결해 주세요.", this);
            }

            if (_status == null || _targetRenderer == null)
            {
                enabled = false;
                return;
            }

            if (_scaleTarget == null)
            {
                _scaleTarget = _targetRenderer.transform;
            }

            _baseScale = _scaleTarget.localScale;
            _propertyBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        /// 예고 여부가 바뀌었으면 표시를 바꾼다.
        /// </summary>
        private void LateUpdate()
        {
            if (_status.IsTelegraphing == _isShowing)
            {
                return;
            }

            _isShowing = _status.IsTelegraphing;
            ApplyAppearance(_isShowing);
        }

        /// <summary>
        /// 꺼질 때 원래 모습으로 되돌린다.
        /// </summary>
        private void OnDisable()
        {
            if (_isShowing)
            {
                _isShowing = false;
                ApplyAppearance(false);
            }
        }

        /// <summary>
        /// 예고 중이면 색과 크기를 바꾸고, 아니면 원래대로 되돌린다.
        /// </summary>
        /// <param name="isTelegraphing">예고 중이면 true</param>
        private void ApplyAppearance(bool isTelegraphing)
        {
            if (isTelegraphing)
            {
                _propertyBlock.SetColor(ShaderPropertyIds.BaseColor, _telegraphColor);
                _targetRenderer.SetPropertyBlock(_propertyBlock);
                _scaleTarget.localScale = _baseScale * _scaleMultiplier;
            }
            else
            {
                _targetRenderer.SetPropertyBlock(null);
                _scaleTarget.localScale = _baseScale;
            }
        }
    }
}
