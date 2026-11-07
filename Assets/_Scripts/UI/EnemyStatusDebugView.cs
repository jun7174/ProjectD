// 파일 경로: Assets/_Scripts/UI/EnemyStatusDebugView.cs
using ProjectD.AI;
using ProjectD.Core.Define;
using TMPro;
using UnityEngine;

namespace ProjectD.UI
{
    /// <summary>
    /// 적 머리 위에 상태 이름과 경계 게이지를 글자로 보여 주는 디버그 표시.
    /// EnemyStatus만 읽으므로 마스터와 다른 클라이언트 모두에서 같은 값이 보인다.
    /// 값이 계속 바뀌므로 이벤트 대신 매 프레임 읽는다.
    /// 적 프리팹 자식의 TextMeshPro(3D 글자) 오브젝트에 붙인다.
    /// </summary>
    public class EnemyStatusDebugView : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("비워 두면 부모에서 찾는다")]
        [SerializeField] private EnemyStatus _status;

        [Tooltip("비워 두면 같은 오브젝트에서 찾는다")]
        [SerializeField] private TMP_Text _text;

        [Header("색")]
        [Tooltip("게이지가 0일 때 색")]
        [SerializeField] private Color _calmColor = Color.white;

        [Tooltip("게이지가 가득 찼을 때 색")]
        [SerializeField] private Color _alertColor = Color.red;

        private Camera _camera;
        private int _lastShownPercent = -1;
        private EnemyState _lastShownState;

        /// <summary>
        /// 참조를 찾고, 비어 있으면 경고 후 이 컴포넌트를 끈다.
        /// </summary>
        private void Awake()
        {
            if (_status == null)
            {
                _status = GetComponentInParent<EnemyStatus>();
            }

            if (_text == null && !TryGetComponent(out _text))
            {
                Debug.LogWarning($"[{nameof(EnemyStatusDebugView)}] TextMeshPro 글자를 찾지 못했습니다.", this);
            }

            if (_status == null || _text == null)
            {
                Debug.LogWarning($"[{nameof(EnemyStatusDebugView)}] EnemyStatus 또는 글자 참조가 비어 있어 디버그 표시를 끕니다.", this);
                enabled = false;
                return;
            }

            _camera = Camera.main;
        }

        /// <summary>
        /// 상태 이름과 경계 게이지를 글자와 색으로 표시하고, 글자가 항상 카메라를 향하게 한다.
        /// 글자 갱신은 값이 바뀔 때만 해서 매 프레임 문자열을 만들지 않는다.
        /// </summary>
        private void LateUpdate()
        {
            int percent = Mathf.RoundToInt(_status.AlertLevel * 100f);
            EnemyState state = _status.CurrentState;

            if (percent != _lastShownPercent || state != _lastShownState)
            {
                _lastShownPercent = percent;
                _lastShownState = state;
                _text.text = $"{GetStateName(state)}\n경계 {percent}%";
                _text.color = Color.Lerp(_calmColor, _alertColor, _status.AlertLevel);
            }

            if (_camera != null)
            {
                transform.rotation = _camera.transform.rotation;
            }
        }

        /// <summary>
        /// 상태를 화면에 보일 한글 이름으로 바꾼다.
        /// </summary>
        /// <param name="state">적 상태</param>
        /// <returns>한글 상태 이름</returns>
        private static string GetStateName(EnemyState state)
        {
            switch (state)
            {
                case EnemyState.Patrol:
                    return "순찰";
                case EnemyState.Investigate:
                    return "조사";
                case EnemyState.Chase:
                    return "추적";
                case EnemyState.Attack:
                    return "공격";
                case EnemyState.Return:
                    return "복귀";
                case EnemyState.Dead:
                    return "사망";
                default:
                    return state.ToString();
            }
        }
    }
}
