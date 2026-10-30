// 파일 경로: Assets/_Scripts/Network/PlayerNetworkSetup.cs
using Photon.Pun;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 플레이어 캐릭터가 내 캐릭터인지 남의 캐릭터인지 판단해서 컴포넌트를 켜고 끈다.
    /// 내 캐릭터 / 남의 캐릭터 분기는 이 컴포넌트 한곳에서만 하고, 다른 스크립트에는 IsMine 확인을 흩어 두지 않는다.
    /// 끌 컴포넌트는 인스펙터 목록으로 받기 때문에 Network 모듈이 Player, Combat의 개별 클래스를 알 필요가 없다.
    /// 플레이어 프리팹 루트에 붙인다.
    /// </summary>
    // 다른 컴포넌트가 입력을 받거나 움직이기 전에 켜고 끄는 처리를 먼저 하도록 기본 스크립트보다 먼저 실행한다
    [DefaultExecutionOrder(-100)]
    public class PlayerNetworkSetup : MonoBehaviourPun
    {
        [Header("내 캐릭터에서만 켜 둘 컴포넌트")]
        [Tooltip("남의 캐릭터에서는 끈다. 입력, 이동, 조준, 회피, 스태미나, 카메라 관련 컴포넌트를 넣는다")]
        [SerializeField] private Behaviour[] _localOnlyBehaviours;

        [Header("네트워크 방 안에서만 켜 둘 컴포넌트")]
        [Tooltip("방 밖(Sandbox 씬)에서는 끈다. PhotonTransformView처럼 동기화를 담당하는 컴포넌트를 넣는다")]
        [SerializeField] private Behaviour[] _networkOnlyBehaviours;

        /// <summary>
        /// 이 캐릭터가 이 컴퓨터의 플레이어가 조작하는 캐릭터인지 여부
        /// </summary>
        public bool IsLocalPlayer { get; private set; }

        /// <summary>
        /// 내 캐릭터인지 판단하고 컴포넌트를 켜고 끈다.
        /// PUN 2는 PhotonNetwork.Instantiate로 만든 오브젝트의 PhotonView를 설정한 뒤 활성화하므로 Awake에서 IsMine을 읽을 수 있다.
        /// </summary>
        private void Awake()
        {
            // 방 밖이면 서버 없이 Sandbox 씬에서 실행한 경우다. 내 캐릭터로 두고 동기화 컴포넌트만 끈다.
            // 동기화 컴포넌트를 켜 두면 받은 값이 없는 위치(원점)로 캐릭터를 끌어당긴다
            if (!PhotonNetwork.InRoom)
            {
                IsLocalPlayer = true;
                SetBehavioursEnabled(_networkOnlyBehaviours, false);
                return;
            }

            IsLocalPlayer = photonView.IsMine;

            if (!IsLocalPlayer)
            {
                SetBehavioursEnabled(_localOnlyBehaviours, false);
            }

            gameObject.name = BuildObjectName();
        }

        /// <summary>
        /// 목록의 컴포넌트를 한 번에 켜거나 끈다. 비어 있는 칸은 건너뛴다.
        /// </summary>
        /// <param name="behaviours">켜고 끌 컴포넌트 목록</param>
        /// <param name="isEnabled">켜려면 true</param>
        private void SetBehavioursEnabled(Behaviour[] behaviours, bool isEnabled)
        {
            if (behaviours == null)
            {
                return;
            }

            foreach (Behaviour behaviour in behaviours)
            {
                if (behaviour == null)
                {
                    Debug.LogWarning($"[{nameof(PlayerNetworkSetup)}] 컴포넌트 목록에 비어 있는 칸이 있습니다. 프리팹 인스펙터를 확인해 주세요.", this);
                    continue;
                }

                behaviour.enabled = isEnabled;
            }
        }

        /// <summary>
        /// 하이어라키에서 누구의 캐릭터인지 알아보기 쉽도록 오브젝트 이름을 만든다. 예: Player_형준 (내 캐릭터)
        /// </summary>
        /// <returns>오브젝트 이름</returns>
        private string BuildObjectName()
        {
            string ownerName = photonView.Owner != null ? photonView.Owner.NickName : "알 수 없음";
            string suffix = IsLocalPlayer ? " (내 캐릭터)" : string.Empty;
            return $"Player_{ownerName}{suffix}";
        }
    }
}
