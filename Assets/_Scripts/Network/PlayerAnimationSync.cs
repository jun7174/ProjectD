// 파일 경로: Assets/_Scripts/Network/PlayerAnimationSync.cs
using Photon.Pun;
using ProjectD.Core.Define;
using ProjectD.Player;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 플레이어 애니메이션 상태를 네트워크로 동기화한다 (Week2 D2 B안).
    /// 내 캐릭터는 PlayerAnimationController가 정한 State 번호, 이동 클립 방향, 회피 횟수를 보내고,
    /// 남의 캐릭터는 받은 값을 PlayerAnimationController에 그대로 적용한다. Animator에 값을 쓰는 곳은 여전히 PlayerAnimationController 하나다.
    /// 회피 횟수는 연속 회피처럼 State가 회피로 유지되는 동안에도 회피 클립을 처음부터 다시 재생하기 위해 보낸다.
    /// 플레이어 프리팹 루트에 붙이고, PhotonView의 Observed Components에 등록한다.
    /// </summary>
    public class PlayerAnimationSync : MonoBehaviourPun, IPunObservable
    {
        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private PlayerAnimationController _animationController;
        [SerializeField] private PlayerDodger _dodger;

        // 내 캐릭터에서 회피를 시작한 횟수. byte라 255 다음에 0으로 돌아가지만, 바뀌었는지만 비교하므로 문제없다
        private byte _sentDodgeCount;

        // 남의 캐릭터에서 마지막으로 받은 회피 횟수와 처음 받았는지 여부
        private byte _receivedDodgeCount;
        private bool _hasReceived;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_animationController == null)
            {
                _animationController = GetComponent<PlayerAnimationController>();
            }

            if (_dodger == null)
            {
                _dodger = GetComponent<PlayerDodger>();
            }

            if (_animationController == null)
            {
                Debug.LogWarning($"[{nameof(PlayerAnimationSync)}] PlayerAnimationController를 찾지 못했습니다. 애니메이션이 동기화되지 않습니다.", this);
            }

            if (_dodger == null)
            {
                Debug.LogWarning($"[{nameof(PlayerAnimationSync)}] PlayerDodger를 찾지 못했습니다. 연속 회피 재시작이 동기화되지 않습니다.", this);
            }
        }

        /// <summary>
        /// 회피 시작 이벤트를 구독한다. 남의 캐릭터에서는 PlayerDodger가 꺼져 있어 이벤트가 발생하지 않는다.
        /// </summary>
        private void OnEnable()
        {
            if (_dodger != null)
            {
                _dodger.OnDodgeStarted += HandleDodgeStarted;
            }
        }

        /// <summary>
        /// 회피 시작 이벤트 구독을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            if (_dodger != null)
            {
                _dodger.OnDodgeStarted -= HandleDodgeStarted;
            }
        }

        /// <summary>
        /// PhotonView가 정기적으로 호출한다. 내 캐릭터는 값을 보내고, 남의 캐릭터는 값을 받아 적용한다.
        /// 보내는 순서와 읽는 순서가 반드시 같아야 한다.
        /// </summary>
        /// <param name="stream">보내거나 받을 데이터 스트림</param>
        /// <param name="info">메시지 정보</param>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (_animationController == null)
            {
                return;
            }

            if (stream.IsWriting)
            {
                WriteState(stream);
            }
            else
            {
                ReadState(stream);
            }
        }

        /// <summary>
        /// 현재 상태 번호, 뒷걸음 여부, 회피 횟수를 보낸다. 값마다 가장 작은 자료형을 써서 전송량을 줄인다.
        /// </summary>
        /// <param name="stream">보낼 데이터 스트림</param>
        private void WriteState(PhotonStream stream)
        {
            bool isBackward = _animationController.CurrentMoveDirection < 0f;

            stream.SendNext((byte)_animationController.CurrentState);
            stream.SendNext(isBackward);
            stream.SendNext(_sentDodgeCount);
        }

        /// <summary>
        /// 받은 상태를 적용한다. 회피 횟수가 바뀌었으면 회피 클립을 처음부터 다시 재생한다.
        /// 처음 받은 값은 기준으로만 저장해서, 중도 참가 직후에 지난 회피가 재생되지 않게 한다.
        /// </summary>
        /// <param name="stream">받은 데이터 스트림</param>
        private void ReadState(PhotonStream stream)
        {
            PlayerAnimState state = (PlayerAnimState)(byte)stream.ReceiveNext();
            bool isBackward = (bool)stream.ReceiveNext();
            byte dodgeCount = (byte)stream.ReceiveNext();

            _animationController.ApplyExternalState(state, isBackward ? -1f : 1f);

            if (_hasReceived && dodgeCount != _receivedDodgeCount)
            {
                _animationController.PlayDodgeFromStart();
            }

            _receivedDodgeCount = dodgeCount;
            _hasReceived = true;
        }

        /// <summary>
        /// 내 캐릭터가 회피를 시작하면 회피 횟수를 늘린다.
        /// </summary>
        private void HandleDodgeStarted()
        {
            _sentDodgeCount++;
        }
    }
}
