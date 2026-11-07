// 파일 경로: Assets/_Scripts/Network/EnemyNetworkSetup.cs
using Photon.Pun;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 적이 이 컴퓨터에서 AI를 직접 실행해야 하는지 판단해서 컴포넌트를 켜고 끈다.
    /// 적 AI는 마스터에서만 실행한다. 마스터가 아닌 클라이언트는 NavMeshAgent와 AI를 끄고, 받은 위치와 상태로만 보여 준다.
    /// 끌 컴포넌트는 인스펙터 목록으로 받기 때문에 Network 모듈이 AI 모듈의 개별 클래스를 알 필요가 없다 (PlayerNetworkSetup과 같은 방식).
    /// 적 프리팹 루트에 붙인다.
    /// </summary>
    // 다른 컴포넌트가 움직이기 전에 켜고 끄는 처리를 먼저 하도록 기본 스크립트보다 먼저 실행한다
    [DefaultExecutionOrder(-100)]
    public class EnemyNetworkSetup : MonoBehaviourPun
    {
        [Header("AI를 실행하는 쪽에서만 켜 둘 컴포넌트")]
        [Tooltip("마스터가 아니면 끈다. NavMeshAgent, 감지, 상태 머신, 공격 등 AI 컴포넌트를 넣는다")]
        [SerializeField] private Behaviour[] _authorityOnlyBehaviours;

        [Header("네트워크 방 안에서만 켜 둘 컴포넌트")]
        [Tooltip("방 밖(Sandbox 씬)에서는 끈다. PhotonTransformView처럼 동기화를 담당하는 컴포넌트를 넣는다")]
        [SerializeField] private Behaviour[] _networkOnlyBehaviours;

        /// <summary>
        /// 이 컴퓨터가 이 적의 AI를 실행하는지 여부
        /// </summary>
        public bool HasAuthority { get; private set; }

        /// <summary>
        /// AI 실행 여부를 판단하고 컴포넌트를 켜고 끈다.
        /// 방 오브젝트(InstantiateRoomObject로 만든 적)는 마스터 쪽에서 IsMine이 true이므로 IsMine으로 판단한다.
        /// </summary>
        private void Awake()
        {
            // 방 밖이면 Sandbox에 직접 놓은 적이다. AI는 그대로 실행하고 동기화 컴포넌트만 끈다
            if (!PhotonNetwork.InRoom)
            {
                HasAuthority = true;
                SetBehavioursEnabled(_networkOnlyBehaviours, false);
                return;
            }

            HasAuthority = photonView.IsMine;

            if (!HasAuthority)
            {
                SetBehavioursEnabled(_authorityOnlyBehaviours, false);
            }
        }

        /// <summary>
        /// 목록의 컴포넌트를 한 번에 켜거나 끈다. 비어 있는 칸은 경고 후 건너뛴다.
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
                    Debug.LogWarning($"[{nameof(EnemyNetworkSetup)}] 컴포넌트 목록에 비어 있는 칸이 있습니다. 프리팹 인스펙터를 확인해 주세요.", this);
                    continue;
                }

                behaviour.enabled = isEnabled;
            }
        }
    }
}
