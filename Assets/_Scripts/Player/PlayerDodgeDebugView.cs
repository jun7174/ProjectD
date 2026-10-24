// 파일 경로: Assets/_Scripts/Player/PlayerDodgeDebugView.cs
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.Player
{
    /// <summary>
    /// 회피 상태를 캐릭터 색으로 표시하는 디버그용 컴포넌트.
    /// 무적 구간과 무적이 아닌 회피 구간을 다른 색으로 보여준다.
    /// </summary>
    public class PlayerDodgeDebugView : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("회피 상태를 읽을 대상. 비워 두면 같은 오브젝트에서 찾는다")]
        [SerializeField] private PlayerDodger _dodger;

        [Tooltip("색을 바꿀 렌더러. 비워 두면 같은 오브젝트에서 찾는다")]
        [SerializeField] private Renderer _targetRenderer;

        [Header("표시 색")]
        [Tooltip("무적 구간 색")]
        [SerializeField] private Color _invincibleColor = Color.yellow;

        [Tooltip("무적이 끝난 뒤 남은 회피 구간 색")]
        [SerializeField] private Color _vulnerableDodgeColor = Color.cyan;

        private MaterialPropertyBlock _propertyBlock;
        private bool _wasDodging;
        private bool _wasInvincible;

        /// <summary>
        /// 참조를 확인하고 색 변경에 사용할 프로퍼티 블록을 만든다.
        /// </summary>
        private void Awake()
        {
            if (_dodger == null)
            {
                _dodger = GetComponent<PlayerDodger>();
            }

            if (_targetRenderer == null)
            {
                _targetRenderer = GetComponent<Renderer>();
            }

            if (_dodger == null || _targetRenderer == null)
            {
                Debug.LogWarning($"[{nameof(PlayerDodgeDebugView)}] PlayerDodger 또는 Renderer를 찾지 못했습니다. 회피 색 표시가 동작하지 않습니다.", this);
            }

            _propertyBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        /// 회피 상태가 바뀐 프레임에만 색을 갱신한다.
        /// </summary>
        private void Update()
        {
            if (_dodger == null || _targetRenderer == null)
            {
                return;
            }

            bool isDodging = _dodger.IsDodging;
            bool isInvincible = _dodger.IsInvincible;

            if (isDodging == _wasDodging && isInvincible == _wasInvincible)
            {
                return;
            }

            _wasDodging = isDodging;
            _wasInvincible = isInvincible;
            ApplyColor(isDodging, isInvincible);
        }

        /// <summary>
        /// 회피 상태에 맞는 색을 렌더러에 적용한다. 회피 중이 아니면 원래 머티리얼 색으로 되돌린다.
        /// </summary>
        /// <param name="isDodging">회피 중인지 여부</param>
        /// <param name="isInvincible">무적 상태인지 여부</param>
        private void ApplyColor(bool isDodging, bool isInvincible)
        {
            if (!isDodging)
            {
                _targetRenderer.SetPropertyBlock(null);
                return;
            }

            _propertyBlock.SetColor(ShaderPropertyIds.BaseColor, isInvincible ? _invincibleColor : _vulnerableDodgeColor);
            _targetRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
