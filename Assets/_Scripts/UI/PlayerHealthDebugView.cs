// 파일 경로: Assets/_Scripts/UI/PlayerHealthDebugView.cs
using ProjectD.Combat;
using ProjectD.Network;
using TMPro;
using UnityEngine;

namespace ProjectD.UI
{
    /// <summary>
    /// 내 캐릭터 머리 위에 체력을 글자로 보여 주는 임시 표시. 체력이 0이면 "사망(임시)"를 보여 준다.
    /// 체력 이벤트와 회복 이벤트를 받을 때만 글자를 바꾼다.
    /// 플레이어 프리팹 자식의 TextMeshPro(3D 글자) 오브젝트에 붙이고,
    /// 이 컴포넌트와 TextMeshPro를 PlayerNetworkSetup의 Local Only 목록에 넣어 내 캐릭터에서만 보이게 한다.
    /// </summary>
    public class PlayerHealthDebugView : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("비워 두면 부모에서 찾는다")]
        [SerializeField] private Health _health;

        [Tooltip("비워 두면 부모에서 찾는다")]
        [SerializeField] private PlayerTemporaryDeath _temporaryDeath;

        [Tooltip("비워 두면 같은 오브젝트에서 찾는다")]
        [SerializeField] private TMP_Text _text;

        [Header("색")]
        [Tooltip("살아 있을 때 색")]
        [SerializeField] private Color _aliveColor = Color.green;

        [Tooltip("쓰러졌을 때 색")]
        [SerializeField] private Color _downColor = Color.red;

        private Camera _camera;

        /// <summary>
        /// 참조를 찾고, 비어 있으면 경고 후 이 컴포넌트를 끈다.
        /// </summary>
        private void Awake()
        {
            if (_health == null)
            {
                _health = GetComponentInParent<Health>();
            }

            if (_temporaryDeath == null)
            {
                _temporaryDeath = GetComponentInParent<PlayerTemporaryDeath>();
            }

            if (_text == null && !TryGetComponent(out _text))
            {
                Debug.LogWarning($"[{nameof(PlayerHealthDebugView)}] TextMeshPro 글자를 찾지 못했습니다.", this);
            }

            if (_health == null || _text == null)
            {
                Debug.LogWarning($"[{nameof(PlayerHealthDebugView)}] Health 또는 글자 참조가 비어 있어 체력 표시를 끕니다.", this);
                enabled = false;
                return;
            }

            _camera = Camera.main;
        }

        /// <summary>
        /// 체력과 회복 이벤트를 구독하고 지금 값을 한 번 그린다.
        /// </summary>
        private void OnEnable()
        {
            if (_health == null)
            {
                return;
            }

            _health.OnDamaged += HandleDamaged;
            _health.OnDied += Refresh;

            if (_temporaryDeath != null)
            {
                _temporaryDeath.OnRevived += Refresh;
            }

            Refresh();
        }

        /// <summary>
        /// 구독을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            if (_health == null)
            {
                return;
            }

            _health.OnDamaged -= HandleDamaged;
            _health.OnDied -= Refresh;

            if (_temporaryDeath != null)
            {
                _temporaryDeath.OnRevived -= Refresh;
            }
        }

        /// <summary>
        /// 글자가 항상 카메라를 향하게 한다.
        /// </summary>
        private void LateUpdate()
        {
            if (_camera != null)
            {
                transform.rotation = _camera.transform.rotation;
            }
        }

        /// <summary>
        /// 데미지를 받으면 글자를 다시 그린다.
        /// </summary>
        /// <param name="amount">데미지 양</param>
        /// <param name="hitPoint">맞은 위치</param>
        private void HandleDamaged(float amount, Vector3 hitPoint)
        {
            Refresh();
        }

        /// <summary>
        /// 지금 체력을 글자와 색으로 그린다.
        /// </summary>
        private void Refresh()
        {
            if (_health.IsDead)
            {
                _text.text = "사망(임시)";
                _text.color = _downColor;
                return;
            }

            _text.text = $"HP {Mathf.CeilToInt(_health.CurrentHealth)} / {Mathf.CeilToInt(_health.MaxHealth)}";
            _text.color = _aliveColor;
        }
    }
}
