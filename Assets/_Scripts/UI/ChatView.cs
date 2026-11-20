// 파일 경로: Assets/_Scripts/UI/ChatView.cs
using System.Collections.Generic;
using ProjectD.Core.Define;
using ProjectD.Network;
using ProjectD.Player;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectD.UI
{
    /// <summary>
    /// 레이드 채팅 화면. Enter로 입력창을 열고, Enter로 보내고, Esc로 취소한다.
    /// 입력하는 동안 내 캐릭터의 게임 입력을 모두 막는다 (PlayerInputGate).
    /// 왼쪽 아래에 최근 몇 줄을 보여 주고, 새 메시지가 없으면 잠시 뒤 흐려진다. 입력창을 열면 다시 보인다.
    /// 혼자 플레이(오프라인)에서는 숨긴다.
    /// Raid 씬 Canvas 아래에 두고, LocalPlayerUiBinder가 내 캐릭터와 연결한다.
    /// </summary>
    public class ChatView : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("Raid 씬의 채팅 (비워 두면 씬에서 찾는다)")]
        [SerializeField] private RaidChat _chat;

        [Tooltip("채팅 전체. 채팅을 쓸 수 없으면 꺼진다")]
        [SerializeField] private GameObject _root;

        [Tooltip("메시지 목록 글자")]
        [SerializeField] private TMP_Text _logText;

        [Tooltip("메시지 목록을 흐리게 할 때 쓴다")]
        [SerializeField] private CanvasGroup _logGroup;

        [Tooltip("입력창 (한 줄)")]
        [SerializeField] private TMP_InputField _inputField;

        [Header("표시")]
        [Tooltip("보여 줄 최대 줄 수")]
        [Min(1)]
        [SerializeField] private int _maxLines = 6;

        [Tooltip("마지막 메시지 후 이 시간(초)이 지나면 흐려지기 시작한다")]
        [Min(0f)]
        [SerializeField] private float _fadeDelay = 8f;

        [Tooltip("흐려지는 데 걸리는 시간 (초)")]
        [Min(0.01f)]
        [SerializeField] private float _fadeDuration = 1f;

        [Tooltip("시스템 알림(입장, 퇴장) 색")]
        [SerializeField] private Color _systemColor = new Color(1f, 0.85f, 0.4f);

        private readonly Queue<string> _lines = new Queue<string>();

        private PlayerControls _controls;
        private PlayerInputGate _inputGate;
        private bool _isTyping;
        private int _closedFrame = -1;
        private float _lastMessageTime = float.NegativeInfinity;

        /// <summary>
        /// 입력 클래스를 만들고, 채팅을 찾고, 입력창을 닫아 둔다.
        /// </summary>
        private void Awake()
        {
            _controls = new PlayerControls();

            if (_chat == null)
            {
                _chat = FindFirstObjectByType<RaidChat>();
            }

            if (_chat == null || _root == null || _logText == null || _inputField == null)
            {
                Debug.LogWarning($"[{nameof(ChatView)}] RaidChat 또는 화면 참조가 비어 있습니다. 채팅이 동작하지 않습니다.", this);
                enabled = false;
                return;
            }

            _inputField.characterLimit = _chat.MaxLength;
            _inputField.gameObject.SetActive(false);
            _logText.text = string.Empty;
        }

        /// <summary>
        /// 입력을 켜고 채팅 이벤트를 구독한다.
        /// </summary>
        private void OnEnable()
        {
            _controls.Player.Enable();

            _chat.OnMessageReceived += HandleMessageReceived;
            _chat.OnSystemMessage += HandleSystemMessage;
            _inputField.onSubmit.AddListener(HandleSubmit);
        }

        /// <summary>
        /// 입력을 끄고 구독을 해제한다. 입력 중이었으면 막아 둔 게임 입력을 푼다.
        /// </summary>
        private void OnDisable()
        {
            _controls.Player.Disable();

            if (_chat != null)
            {
                _chat.OnMessageReceived -= HandleMessageReceived;
                _chat.OnSystemMessage -= HandleSystemMessage;
            }

            if (_inputField != null)
            {
                _inputField.onSubmit.RemoveListener(HandleSubmit);
            }

            CloseInput();
        }

        /// <summary>
        /// 입력 클래스를 정리한다.
        /// </summary>
        private void OnDestroy()
        {
            _controls.Dispose();
        }

        /// <summary>
        /// 내 캐릭터의 입력 막기 컴포넌트를 연결한다.
        /// </summary>
        /// <param name="inputGate">입력 막기</param>
        public void SetTarget(PlayerInputGate inputGate)
        {
            _inputGate = inputGate;
        }

        /// <summary>
        /// Enter로 입력창을 열고 Esc로 닫는다. 채팅을 쓸 수 없으면 숨긴다. 메시지 목록을 흐리게 한다.
        /// </summary>
        private void Update()
        {
            bool isAvailable = _chat.IsAvailable;
            if (_root.activeSelf != isAvailable)
            {
                _root.SetActive(isAvailable);
            }

            if (!isAvailable)
            {
                return;
            }

            // 보내기로 입력창을 닫은 같은 프레임에 Enter가 다시 입력창을 여는 것을 막는다
            if (!_isTyping && Time.frameCount != _closedFrame && _controls.Player.Chat.WasPressedThisFrame())
            {
                OpenInput();
            }
            else if (_isTyping && _controls.Player.Pause.WasPressedThisFrame())
            {
                CloseInput();
            }

            UpdateFade();
        }

        /// <summary>
        /// 입력창을 열고 게임 입력을 막는다.
        /// </summary>
        private void OpenInput()
        {
            _isTyping = true;
            _inputField.gameObject.SetActive(true);
            _inputField.text = string.Empty;
            _inputField.Select();
            _inputField.ActivateInputField();

            if (_inputGate != null)
            {
                _inputGate.SetBlocked(InputBlockReason.Chat, true);
            }
        }

        /// <summary>
        /// 입력창을 닫고 막아 둔 게임 입력을 푼다.
        /// </summary>
        private void CloseInput()
        {
            if (!_isTyping)
            {
                return;
            }

            _isTyping = false;
            _closedFrame = Time.frameCount;
            _inputField.DeactivateInputField();
            _inputField.gameObject.SetActive(false);

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            if (_inputGate != null)
            {
                _inputGate.SetBlocked(InputBlockReason.Chat, false);
            }
        }

        /// <summary>
        /// 입력창에서 Enter를 누르면 메시지를 보내고 입력창을 닫는다.
        /// </summary>
        /// <param name="text">입력한 문장</param>
        private void HandleSubmit(string text)
        {
            if (!_isTyping)
            {
                return;
            }

            _chat.Send(text);
            CloseInput();
        }

        /// <summary>
        /// 받은 메시지를 목록에 더한다.
        /// </summary>
        /// <param name="senderName">보낸 사람 이름</param>
        /// <param name="message">메시지</param>
        private void HandleMessageReceived(string senderName, string message)
        {
            AddLine($"{senderName}: {message}");
        }

        /// <summary>
        /// 시스템 알림을 색을 입혀 목록에 더한다.
        /// </summary>
        /// <param name="message">알림 문구</param>
        private void HandleSystemMessage(string message)
        {
            AddLine($"<color=#{ColorUtility.ToHtmlStringRGB(_systemColor)}>{message}</color>");
        }

        /// <summary>
        /// 목록에 한 줄을 더하고, 넘치면 오래된 줄을 지운다.
        /// </summary>
        /// <param name="line">더할 줄</param>
        private void AddLine(string line)
        {
            _lines.Enqueue(line);
            while (_lines.Count > _maxLines)
            {
                _lines.Dequeue();
            }

            _logText.text = string.Join("\n", _lines);
            _lastMessageTime = Time.time;
        }

        /// <summary>
        /// 입력 중이면 진하게, 아니면 마지막 메시지 후 시간에 따라 흐리게 한다.
        /// </summary>
        private void UpdateFade()
        {
            if (_logGroup == null)
            {
                return;
            }

            if (_isTyping)
            {
                _logGroup.alpha = 1f;
                return;
            }

            float elapsed = Time.time - _lastMessageTime - _fadeDelay;
            _logGroup.alpha = 1f - Mathf.Clamp01(elapsed / _fadeDuration);
        }
    }
}
