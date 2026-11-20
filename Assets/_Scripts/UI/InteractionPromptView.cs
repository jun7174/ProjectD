// 파일 경로: Assets/_Scripts/UI/InteractionPromptView.cs
using ProjectD.Core;
using ProjectD.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectD.UI
{
    /// <summary>
    /// 상호작용 안내 ("F  열기: 상자")와 길게 누르기 진행 막대를 화면에 보여 준다.
    /// 대상이 바뀌면 이벤트로 글자를 바꾸고, 진행 막대와 안내 문구(빈 상자 등)는 값이 계속 바뀌므로 짧은 간격으로 읽는다.
    /// Raid 씬(Sandbox 포함) Canvas 아래에 두고, LocalPlayerUiBinder가 내 캐릭터와 연결한다.
    /// </summary>
    public class InteractionPromptView : MonoBehaviour
    {
        [Tooltip("안내 전체. 대상이 없으면 꺼진다")]
        [SerializeField] private GameObject _root;

        [SerializeField] private TMP_Text _promptText;

        [Tooltip("길게 누르기 진행 막대 (Image Type을 Filled로)")]
        [SerializeField] private Image _progressFill;

        [Tooltip("안내 문구 앞에 붙일 키 이름")]
        [SerializeField] private string _keyLabel = "F";

        [Tooltip("안내 문구를 다시 읽는 간격 (초)")]
        [Min(0.05f)]
        [SerializeField] private float _refreshInterval = 0.25f;

        private PlayerInteractor _interactor;
        private float _nextRefreshTime;

        /// <summary>
        /// 안내를 숨겨 둔다.
        /// </summary>
        private void Awake()
        {
            if (_root == null || _promptText == null)
            {
                Debug.LogWarning($"[{nameof(InteractionPromptView)}] 화면 참조가 비어 있습니다. 상호작용 안내가 보이지 않습니다.", this);
                enabled = false;
                return;
            }

            _root.SetActive(false);
        }

        /// <summary>
        /// 구독을 해제한다.
        /// </summary>
        private void OnDestroy()
        {
            if (_interactor != null)
            {
                _interactor.OnTargetChanged -= HandleTargetChanged;
            }
        }

        /// <summary>
        /// 내 캐릭터의 상호작용 컴포넌트를 연결한다.
        /// </summary>
        /// <param name="interactor">상호작용 컴포넌트</param>
        public void SetTarget(PlayerInteractor interactor)
        {
            if (_interactor != null)
            {
                _interactor.OnTargetChanged -= HandleTargetChanged;
            }

            _interactor = interactor;

            if (_interactor != null)
            {
                _interactor.OnTargetChanged += HandleTargetChanged;
                HandleTargetChanged(_interactor.CurrentTarget);
            }
        }

        /// <summary>
        /// 진행 막대를 그리고, 일정 간격으로 안내 문구를 다시 읽는다.
        /// </summary>
        private void LateUpdate()
        {
            if (_interactor == null || !_root.activeSelf)
            {
                return;
            }

            IInteractable target = _interactor.CurrentTarget;
            if (target == null)
            {
                _root.SetActive(false);
                return;
            }

            if (_progressFill != null)
            {
                _progressFill.fillAmount = _interactor.HoldProgress;
            }

            if (Time.time >= _nextRefreshTime)
            {
                RefreshText(target);
            }
        }

        /// <summary>
        /// 대상이 바뀌면 안내를 켜거나 끄고 글자를 바꾼다.
        /// </summary>
        /// <param name="target">새 대상. 없으면 null</param>
        private void HandleTargetChanged(IInteractable target)
        {
            _root.SetActive(target != null);

            if (target != null)
            {
                RefreshText(target);
            }
        }

        /// <summary>
        /// 안내 문구를 다시 쓴다.
        /// </summary>
        /// <param name="target">대상</param>
        private void RefreshText(IInteractable target)
        {
            _nextRefreshTime = Time.time + _refreshInterval;
            _promptText.text = $"{_keyLabel}  {target.Prompt}";
        }
    }
}
