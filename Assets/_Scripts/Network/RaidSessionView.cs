// 파일 경로: Assets/_Scripts/UI/RaidSessionView.cs
using System.Text;
using ProjectD.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectD.UI
{
    /// <summary>
    /// Raid 씬의 임시 방 정보 화면. 방 이름, 참가자 목록, 나가기 버튼을 보여 준다.
    /// 중도 참가와 나가기를 확인하기 위한 테스트용 화면이며, 정식 HUD와 메뉴는 추후에 만든다.
    /// </summary>
    public class RaidSessionView : MonoBehaviour
    {
        [Header("로직 참조 (비워 두면 씬에서 찾는다)")]
        [SerializeField] private RaidSessionWatcher _sessionWatcher;

        [Header("UI 참조")]
        [SerializeField] private TMP_Text _roomNameText;
        [SerializeField] private TMP_Text _playerListText;
        [SerializeField] private Button _leaveButton;

        [Tooltip("방장 이탈 같은 안내 문구. 비워 두면 표시하지 않는다")]
        [SerializeField] private TMP_Text _statusText;

        private readonly StringBuilder _builder = new StringBuilder();

        /// <summary>
        /// 참조를 찾고, 필수 참조가 비어 있으면 경고 후 이 컴포넌트를 끈다.
        /// </summary>
        private void Awake()
        {
            if (_sessionWatcher == null)
            {
                _sessionWatcher = FindFirstObjectByType<RaidSessionWatcher>();
            }

            if (_sessionWatcher == null || _roomNameText == null || _playerListText == null || _leaveButton == null)
            {
                Debug.LogWarning($"[{nameof(RaidSessionView)}] RaidSessionWatcher 또는 UI 참조(방 이름, 참가자 목록, 나가기 버튼) 중 비어 있는 것이 있습니다. 방 정보 화면이 동작하지 않습니다.", this);
                enabled = false;
            }
        }

        /// <summary>
        /// 버튼과 로직 이벤트를 구독한다.
        /// </summary>
        private void OnEnable()
        {
            _leaveButton.onClick.AddListener(HandleLeaveClicked);
            _sessionWatcher.OnPlayerListChanged += Redraw;
            _sessionWatcher.OnMasterLeft += HandleMasterLeft;
        }

        /// <summary>
        /// 구독을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            _leaveButton.onClick.RemoveListener(HandleLeaveClicked);
            _sessionWatcher.OnPlayerListChanged -= Redraw;
            _sessionWatcher.OnMasterLeft -= HandleMasterLeft;
        }

        /// <summary>
        /// 첫 화면을 그린다. 방 정보는 RaidSessionWatcher가 Start에서 읽은 뒤 이벤트로도 다시 알려 준다.
        /// </summary>
        private void Start()
        {
            SetStatus(string.Empty);
            Redraw();
        }

        /// <summary>
        /// 방 이름과 참가자 목록을 다시 그린다. 방장 이름 옆에는 (방장)을 붙인다.
        /// </summary>
        private void Redraw()
        {
            _roomNameText.text = _sessionWatcher.IsOfflineMode ? "혼자 플레이" : _sessionWatcher.RoomDisplayName;

            _builder.Clear();
            _builder.Append($"참가자 {_sessionWatcher.PlayerNames.Count}/{_sessionWatcher.MaxPlayers}");

            foreach (string playerName in _sessionWatcher.PlayerNames)
            {
                _builder.Append('\n');
                _builder.Append(playerName);

                if (playerName == _sessionWatcher.MasterPlayerName)
                {
                    _builder.Append(" (방장)");
                }
            }

            _playerListText.text = _builder.ToString();
        }

        /// <summary>
        /// 나가기 버튼을 누르면 버튼을 막고 레이드에서 나간다.
        /// </summary>
        private void HandleLeaveClicked()
        {
            _leaveButton.interactable = false;
            SetStatus("나가는 중...");
            _sessionWatcher.LeaveRaid();
        }

        /// <summary>
        /// 방장이 나가 세션이 끝나면 안내 문구를 보여 준다.
        /// </summary>
        private void HandleMasterLeft()
        {
            _leaveButton.interactable = false;
            SetStatus("방장이 나가 레이드가 종료됩니다.");
        }

        /// <summary>
        /// 안내 문구를 바꾼다. 안내 문구가 연결되어 있지 않으면 무시한다.
        /// </summary>
        /// <param name="message">보여 줄 문구</param>
        private void SetStatus(string message)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
            }
        }
    }
}
