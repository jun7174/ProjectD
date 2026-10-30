// 파일 경로: Assets/_Scripts/UI/BootView.cs
using ProjectD.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectD.UI
{
    /// <summary>
    /// Boot 씬 화면. 이름 입력칸, 접속 버튼, 상태 문구, 혼자 플레이 버튼을 관리한다.
    /// 접속과 혼자 플레이 처리는 BootConnector, SoloPlayStarter에 맡기고, 결과는 이벤트로 받아 화면만 바꾼다.
    /// 혼자 플레이 버튼은 접속에 실패했을 때만 보인다.
    /// </summary>
    public class BootView : MonoBehaviour
    {
        [Header("로직 참조 (비워 두면 씬에서 찾는다)")]
        [SerializeField] private BootConnector _connector;
        [SerializeField] private SoloPlayStarter _soloPlayStarter;

        [Header("UI 참조")]
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private Button _connectButton;
        [SerializeField] private Button _soloPlayButton;
        [SerializeField] private TMP_Text _statusText;

        /// <summary>
        /// 참조를 찾고, 필수 참조가 비어 있으면 경고 후 이 컴포넌트를 끈다.
        /// </summary>
        private void Awake()
        {
            if (_connector == null)
            {
                _connector = FindFirstObjectByType<BootConnector>();
            }

            if (_soloPlayStarter == null)
            {
                _soloPlayStarter = FindFirstObjectByType<SoloPlayStarter>();
            }

            if (!HasRequiredReferences())
            {
                enabled = false;
            }
        }

        /// <summary>
        /// 버튼, 입력칸, 로직 이벤트를 구독한다.
        /// </summary>
        private void OnEnable()
        {
            _connectButton.onClick.AddListener(HandleConnectClicked);
            _soloPlayButton.onClick.AddListener(HandleSoloPlayClicked);
            _nameInput.onValueChanged.AddListener(HandleNameChanged);
            _connector.OnConnectStarted += HandleConnectStarted;
            _connector.OnConnectFailed += HandleConnectFailed;
            _soloPlayStarter.OnSoloPlayFailed += HandleSoloPlayFailed;
        }

        /// <summary>
        /// 구독을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            _connectButton.onClick.RemoveListener(HandleConnectClicked);
            _soloPlayButton.onClick.RemoveListener(HandleSoloPlayClicked);
            _nameInput.onValueChanged.RemoveListener(HandleNameChanged);
            _connector.OnConnectStarted -= HandleConnectStarted;
            _connector.OnConnectFailed -= HandleConnectFailed;
            _soloPlayStarter.OnSoloPlayFailed -= HandleSoloPlayFailed;
        }

        /// <summary>
        /// 이름 글자 수 제한과 저장된 이름을 입력칸에 넣고 첫 화면 상태로 만든다.
        /// </summary>
        private void Start()
        {
            _nameInput.characterLimit = _connector.MaxNameLength;
            _nameInput.text = _connector.SavedPlayerName;
            _soloPlayButton.gameObject.SetActive(false);
            SetInputInteractable(true);
            SetStatus("이름을 입력하고 접속하세요.");
        }

        /// <summary>
        /// 접속 버튼을 누르면 접속을 시작한다. 이름이 규칙에 맞지 않으면 안내 문구를 보여 준다.
        /// </summary>
        private void HandleConnectClicked()
        {
            if (!_connector.TryConnect(_nameInput.text))
            {
                SetStatus(BuildNameRuleMessage());
            }
        }

        /// <summary>
        /// 혼자 플레이 버튼을 누르면 이름을 적용하고 혼자 플레이를 시작한다.
        /// </summary>
        private void HandleSoloPlayClicked()
        {
            if (!_connector.TryApplyPlayerName(_nameInput.text))
            {
                SetStatus(BuildNameRuleMessage());
                return;
            }

            SetInputInteractable(false);
            SetStatus("혼자 플레이를 준비하는 중...");
            _soloPlayStarter.StartSoloPlay();
        }

        /// <summary>
        /// 이름이 바뀔 때마다 접속 버튼을 누를 수 있는지 갱신한다.
        /// </summary>
        /// <param name="value">바뀐 이름</param>
        private void HandleNameChanged(string value)
        {
            _connectButton.interactable = _nameInput.interactable && _connector.IsValidPlayerName(value);
        }

        /// <summary>
        /// 접속이 시작되면 입력을 막고 진행 문구를 보여 준다.
        /// </summary>
        private void HandleConnectStarted()
        {
            SetInputInteractable(false);
            _soloPlayButton.gameObject.SetActive(false);
            SetStatus("서버에 접속하는 중...");
        }

        /// <summary>
        /// 접속에 실패하면 입력을 다시 열고 실패 이유와 혼자 플레이 버튼을 보여 준다.
        /// </summary>
        /// <param name="message">실패 이유</param>
        private void HandleConnectFailed(string message)
        {
            SetInputInteractable(true);
            _soloPlayButton.gameObject.SetActive(true);
            SetStatus($"{message}\n다시 시도하거나 혼자 플레이를 선택하세요.");
        }

        /// <summary>
        /// 혼자 플레이 시작에 실패하면 입력을 다시 열고 실패 이유를 보여 준다.
        /// </summary>
        /// <param name="message">실패 이유</param>
        private void HandleSoloPlayFailed(string message)
        {
            SetInputInteractable(true);
            SetStatus(message);
        }

        /// <summary>
        /// 이름 입력칸과 버튼들을 누를 수 있는지 한 번에 바꾼다. 접속 버튼은 이름이 규칙에 맞을 때만 켠다.
        /// </summary>
        /// <param name="interactable">누를 수 있게 하려면 true</param>
        private void SetInputInteractable(bool interactable)
        {
            _nameInput.interactable = interactable;
            _soloPlayButton.interactable = interactable;
            _connectButton.interactable = interactable && _connector.IsValidPlayerName(_nameInput.text);
        }

        /// <summary>
        /// 상태 문구를 바꾼다.
        /// </summary>
        /// <param name="message">보여 줄 문구</param>
        private void SetStatus(string message)
        {
            _statusText.text = message;
        }

        /// <summary>
        /// 이름 규칙 안내 문구를 만든다.
        /// </summary>
        /// <returns>안내 문구</returns>
        private string BuildNameRuleMessage()
        {
            return $"이름은 공백을 제외하고 1~{_connector.MaxNameLength}자로 입력해 주세요.";
        }

        /// <summary>
        /// 필수 참조가 모두 연결되어 있는지 확인하고, 비어 있으면 경고를 출력한다.
        /// </summary>
        /// <returns>모두 연결되어 있으면 true</returns>
        private bool HasRequiredReferences()
        {
            bool isValid = true;

            if (_connector == null)
            {
                Debug.LogWarning($"[{nameof(BootView)}] BootConnector를 찾지 못했습니다. 접속 화면이 동작하지 않습니다.", this);
                isValid = false;
            }

            if (_soloPlayStarter == null)
            {
                Debug.LogWarning($"[{nameof(BootView)}] SoloPlayStarter를 찾지 못했습니다. 접속 화면이 동작하지 않습니다.", this);
                isValid = false;
            }

            if (_nameInput == null || _connectButton == null || _soloPlayButton == null || _statusText == null)
            {
                Debug.LogWarning($"[{nameof(BootView)}] UI 참조(이름 입력칸, 접속 버튼, 혼자 플레이 버튼, 상태 문구) 중 비어 있는 것이 있습니다. 인스펙터를 확인해 주세요.", this);
                isValid = false;
            }

            return isValid;
        }
    }
}
