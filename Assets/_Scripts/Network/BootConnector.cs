// 파일 경로: Assets/_Scripts/Network/BootConnector.cs
using System;
using Photon.Pun;
using Photon.Realtime;
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// Boot 씬에서 플레이어 이름을 정하고 Photon 마스터 서버에 접속한다. 접속에 성공하면 Lobby 씬으로 이동한다.
    /// UI는 모르며, 접속 시작과 실패를 이벤트로 알린다. 로비 입장은 Lobby 씬의 RoomListService가 맡는다.
    /// 리전(kr)과 앱 버전은 코드가 아니라 PhotonServerSettings 에셋에서 설정한다.
    /// </summary>
    public class BootConnector : MonoBehaviourPunCallbacks
    {
        [Header("이름 규칙")]
        [Tooltip("플레이어 이름 최대 글자 수. 앞뒤 공백을 뺀 길이로 검사한다")]
        [Range(1, 20)]
        [SerializeField] private int _maxNameLength = 12;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private SoloPlayStarter _soloPlayStarter;

        private bool _isConnecting;

        /// <summary>
        /// 접속을 시작했을 때 발생한다.
        /// </summary>
        public event Action OnConnectStarted;

        /// <summary>
        /// 접속에 실패했을 때 발생한다. 인자는 화면에 보여 줄 실패 이유다.
        /// </summary>
        public event Action<string> OnConnectFailed;

        /// <summary>
        /// 플레이어 이름 최대 글자 수
        /// </summary>
        public int MaxNameLength => _maxNameLength;

        /// <summary>
        /// 마지막으로 사용한 플레이어 이름. 저장된 이름이 없으면 빈 문자열
        /// </summary>
        public string SavedPlayerName => PlayerPrefs.GetString(PlayerPrefsKeys.PlayerName, string.Empty);

        /// <summary>
        /// 씬 자동 동기화를 끄고 참조를 찾는다.
        /// </summary>
        private void Awake()
        {
            // 마스터가 씬을 불러오면 같은 방의 다른 클라이언트도 같은 씬을 불러온다. 중도 참가자도 이 설정으로 Raid 씬에 들어간다
            PhotonNetwork.AutomaticallySyncScene = false;

            if (_soloPlayStarter == null)
            {
                _soloPlayStarter = GetComponent<SoloPlayStarter>();
            }

            if (_soloPlayStarter == null)
            {
                Debug.LogWarning($"[{nameof(BootConnector)}] SoloPlayStarter를 찾지 못했습니다. 혼자 플레이 중 끊김을 접속 실패로 잘못 표시할 수 있습니다.", this);
            }
        }

        /// <summary>
        /// 혼자 플레이를 마치고 돌아온 경우 오프라인 모드를 끈다. 오프라인 모드가 켜져 있으면 온라인 접속을 할 수 없다.
        /// </summary>
        private void Start()
        {
            if (PhotonNetwork.OfflineMode)
            {
                PhotonNetwork.OfflineMode = false;
            }
        }

        /// <summary>
        /// 이름이 규칙에 맞는지 검사한다. 앞뒤 공백을 뺀 뒤 1글자 이상, 최대 글자 수 이하여야 한다.
        /// </summary>
        /// <param name="rawName">입력칸에 적힌 이름</param>
        /// <returns>사용할 수 있는 이름이면 true</returns>
        public bool IsValidPlayerName(string rawName)
        {
            string playerName = NormalizeName(rawName);
            return playerName.Length > 0 && playerName.Length <= _maxNameLength;
        }

        /// <summary>
        /// 이름이 규칙에 맞으면 Photon 닉네임으로 정하고 다음 실행을 위해 저장한다.
        /// 접속과 혼자 플레이 모두 이 함수로 이름을 정한다.
        /// </summary>
        /// <param name="rawName">입력칸에 적힌 이름</param>
        /// <returns>이름을 적용했으면 true</returns>
        public bool TryApplyPlayerName(string rawName)
        {
            if (!IsValidPlayerName(rawName))
            {
                return false;
            }

            string playerName = NormalizeName(rawName);
            PhotonNetwork.NickName = playerName;
            PlayerPrefs.SetString(PlayerPrefsKeys.PlayerName, playerName);
            PlayerPrefs.Save();
            return true;
        }

        /// <summary>
        /// 이름을 적용하고 Photon 마스터 서버 접속을 시작한다.
        /// </summary>
        /// <param name="rawName">입력칸에 적힌 이름</param>
        /// <returns>접속을 시작했으면 true. 이름이 잘못됐거나 이미 접속 중이면 false</returns>
        public bool TryConnect(string rawName)
        {
            if (_isConnecting)
            {
                return false;
            }

            if (!TryApplyPlayerName(rawName))
            {
                return false;
            }

            _isConnecting = true;
            OnConnectStarted?.Invoke();

            // 이미 마스터 서버에 붙어 있으면 다시 접속할 필요 없이 바로 Lobby로 간다
            if (PhotonNetwork.IsConnectedAndReady)
            {
                MoveToLobby();
                return true;
            }

            if (!PhotonNetwork.ConnectUsingSettings())
            {
                _isConnecting = false;
                OnConnectFailed?.Invoke("접속을 시작하지 못했습니다. PhotonServerSettings 설정을 확인해 주세요.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 마스터 서버 접속에 성공하면 Lobby 씬으로 이동한다. 혼자 플레이(오프라인 모드)는 SoloPlayStarter가 처리하므로 무시한다.
        /// </summary>
        public override void OnConnectedToMaster()
        {
            if (PhotonNetwork.OfflineMode || !_isConnecting)
            {
                return;
            }

            MoveToLobby();
        }

        /// <summary>
        /// 접속이 끊기거나 실패하면 이유를 알린다. 혼자 플레이를 위해 일부러 끊은 경우는 무시한다.
        /// </summary>
        /// <param name="cause">끊긴 원인</param>
        public override void OnDisconnected(DisconnectCause cause)
        {
            if (_soloPlayStarter != null && _soloPlayStarter.IsStarting)
            {
                return;
            }

            _isConnecting = false;
            OnConnectFailed?.Invoke($"서버에 접속하지 못했습니다. ({cause})");
        }

        /// <summary>
        /// Lobby 씬을 불러온다.
        /// PhotonNetwork.LoadLevel은 씬을 불러오는 동안 네트워크 메시지 처리를 멈춰서, 그 사이에 온 콜백을 다음 씬이 받을 수 있게 한다.
        /// </summary>
        private void MoveToLobby()
        {
            _isConnecting = false;
            PhotonNetwork.LoadLevel(SceneNames.Lobby);
        }

        /// <summary>
        /// 이름의 앞뒤 공백을 제거한다. null이면 빈 문자열로 바꾼다.
        /// </summary>
        /// <param name="rawName">입력칸에 적힌 이름</param>
        /// <returns>정리된 이름</returns>
        private static string NormalizeName(string rawName)
        {
            return rawName == null ? string.Empty : rawName.Trim();
        }
    }
}
