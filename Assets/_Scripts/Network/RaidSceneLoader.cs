// 파일 경로: Assets/_Scripts/Network/RaidSceneLoader.cs
using Photon.Pun;
using ProjectD.Core.Define;

namespace ProjectD.Network
{
    /// <summary>
    /// 방에 들어가면 Raid 씬으로 이동한다.
    /// 방 만들기, 목록에서 참가, 혼자 플레이 모두 "방에 들어간 순간"이 같으므로 씬 이동을 이 클래스 하나에 모은다.
    /// 마스터와 중도 참가자 모두 각자 이 클래스로 Raid 씬을 불러온다.
    /// Boot 씬과 Lobby 씬에 하나씩 둔다.
    /// </summary>
    public class RaidSceneLoader : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// 방에 들어가면 바로 Raid 씬을 불러온다.
        /// PhotonNetwork.LoadLevel은 씬을 불러오는 동안 네트워크 메시지 처리를 멈춘다.
        /// 중도 참가자는 방에 들어가자마자 먼저 들어온 플레이어들의 생성 정보(서버에 보관된 Instantiate 이벤트)를 받는데,
        /// 처리를 멈춰 두지 않으면 그 캐릭터들이 Lobby 씬에 생성되었다가 씬이 바뀌면서 함께 사라진다.
        /// 그래서 방에 들어간 바로 이 콜백 안에서 LoadLevel을 호출해, 생성 정보가 Raid 씬을 불러온 뒤에 처리되게 한다.
        /// </summary>
        public override void OnJoinedRoom()
        {
            PhotonNetwork.LoadLevel(SceneNames.Raid);
        }
    }
}
