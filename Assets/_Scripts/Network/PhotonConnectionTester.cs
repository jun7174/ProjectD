// 파일 경로: Assets/_Scripts/Network/PhotonConnectionTester.cs

using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// Photon 마스터 서버 접속을 확인하기 위한 임시 테스트 컴포넌트.
    /// 접속 확인 용도로만 사용
    /// </summary>
    public class PhotonConnectionTester : MonoBehaviourPunCallbacks
    {
        [Tooltip("씬 시작 시 자동으로 접속을 시도할지 여부")]
        [SerializeField] private bool _connectOnStart = true;

        /// <summary>
        /// 씬이 시작되면 설정에 따라 접속을 시도한다.
        /// </summary>
        private void Start()
        {
            if (_connectOnStart)
            {
                TryConnect();
            }
        }

        /// <summary>
        /// PhotonServerSettings에 저장된 설정값으로 마스터 서버 접속을 시도한다.
        /// </summary>
        private void TryConnect()
        {
            if (PhotonNetwork.IsConnected)
            {
                Debug.Log("[접속 테스트] 이미 Photon에 접속되어 있습니다.");
                return;
            }

            Debug.Log("[접속 테스트] Photon 마스터 서버 접속을 시도합니다.");

            bool isRequestSent = PhotonNetwork.ConnectUsingSettings();
            if (!isRequestSent)
            {
                Debug.LogWarning("[접속 테스트] 접속 요청을 보내지 못했습니다. PhotonServerSettings의 App ID 설정을 확인하세요.");
            }
        }

        /// <summary>
        /// 마스터 서버 접속에 성공했을 때 Photon이 호출한다. 접속한 리전을 함께 출력한다.
        /// </summary>
        public override void OnConnectedToMaster()
        {
            Debug.Log($"[접속 테스트] 마스터 서버 접속 성공. 리전: {PhotonNetwork.CloudRegion}");
        }

        /// <summary>
        /// 접속이 끊기거나 접속에 실패했을 때 Photon이 호출한다.
        /// </summary>
        /// <param name="cause">연결이 끊긴 원인</param>
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarning($"[접속 테스트] 접속 종료 또는 실패. 원인: {cause}");
        }
    }
}
