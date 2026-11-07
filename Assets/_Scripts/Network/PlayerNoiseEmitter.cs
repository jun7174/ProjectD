// 파일 경로: Assets/_Scripts/Network/PlayerNoiseEmitter.cs
using Photon.Pun;
using ProjectD.AI;
using ProjectD.Core.Define;
using ProjectD.Player;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 플레이어의 행동에 따라 적에게 소음을 알린다
    /// 적 AI가 마스터에서만 돌기 때문에 소음도 마스터에서만 만든다.
    /// 마스터는 다른 플레이어의 애니메이션 상태(달리기, 회피)와 발사를 이미 동기화로 받고 있으므로, 새 전송 없이 그 정보로 소음을 만든다.
    /// 마스터 화면의 모든 플레이어 캐릭터(내 것과 남의 것)에서 동작해야 하므로 내 캐릭터 전용 목록에 넣지 않는다.
    /// 플레이어 프리팹 루트에 붙인다.
    /// </summary>
    public class PlayerNoiseEmitter : MonoBehaviour
    {
        [Header("소음 반경 (m)")]
        [Tooltip("걷기, 걸으며 사격 자세")]
        [Min(0f)]
        [SerializeField] private float _walkNoiseRadius = 2f;

        [Tooltip("달리기")]
        [Min(0f)]
        [SerializeField] private float _runNoiseRadius = 10f;

        [Tooltip("회피")]
        [Min(0f)]
        [SerializeField] private float _dodgeNoiseRadius = 8f;

        [Tooltip("사격")]
        [Min(0f)]
        [SerializeField] private float _shotNoiseRadius = 20f;

        [Header("설정")]
        [Tooltip("이동 소음을 내는 간격 (초)")]
        [Min(0.05f)]
        [SerializeField] private float _movementNoiseInterval = 0.3f;

        [Tooltip("소음을 들을 대상의 레이어 (Enemy)")]
        [SerializeField] private LayerMask _enemyMask;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private PlayerAnimationController _animationController;
        [SerializeField] private PlayerFireSync _fireSync;

        private float _nextMovementNoiseTime;

        /// <summary>
        /// 이 클라이언트가 소음을 만들어야 하는지 여부. 방 밖(Sandbox)이거나 마스터일 때만 만든다
        /// </summary>
        private static bool IsNoiseAuthority => !PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_animationController == null && !TryGetComponent(out _animationController))
            {
                Debug.LogWarning($"[{nameof(PlayerNoiseEmitter)}] PlayerAnimationController를 찾지 못했습니다. 이동 소음이 나지 않습니다.", this);
            }

            if (_fireSync == null && !TryGetComponent(out _fireSync))
            {
                Debug.LogWarning($"[{nameof(PlayerNoiseEmitter)}] PlayerFireSync를 찾지 못했습니다. 사격 소음이 나지 않습니다.", this);
            }

            if (_enemyMask.value == 0)
            {
                Debug.LogWarning($"[{nameof(PlayerNoiseEmitter)}] Enemy Mask가 비어 있습니다. 소음이 적에게 전달되지 않습니다.", this);
            }
        }

        /// <summary>
        /// 발사 확인 이벤트를 구독한다.
        /// </summary>
        private void OnEnable()
        {
            if (_fireSync != null)
            {
                _fireSync.OnShotObserved += HandleShotObserved;
            }
        }

        /// <summary>
        /// 구독을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            if (_fireSync != null)
            {
                _fireSync.OnShotObserved -= HandleShotObserved;
            }
        }

        /// <summary>
        /// 일정 간격으로 현재 이동 상태에 맞는 소음을 낸다.
        /// </summary>
        private void Update()
        {
            if (!IsNoiseAuthority || _animationController == null || Time.time < _nextMovementNoiseTime)
            {
                return;
            }

            _nextMovementNoiseTime = Time.time + _movementNoiseInterval;
            NoiseSystem.Emit(transform.position, GetMovementNoiseRadius(_animationController.CurrentState), _enemyMask);
        }

        /// <summary>
        /// 이 캐릭터의 발사가 확인되면 사격 소음을 낸다.
        /// </summary>
        private void HandleShotObserved()
        {
            if (!IsNoiseAuthority)
            {
                return;
            }

            NoiseSystem.Emit(transform.position, _shotNoiseRadius, _enemyMask);
        }

        /// <summary>
        /// 애니메이션 상태로 이동 소음 반경을 정한다. 멈춰 있거나 사격 자세로 서 있으면 소음이 없다.
        /// </summary>
        /// <param name="state">현재 애니메이션 상태</param>
        /// <returns>소음 반경. 소음이 없으면 0</returns>
        private float GetMovementNoiseRadius(PlayerAnimState state)
        {
            switch (state)
            {
                case PlayerAnimState.Run:
                    return _runNoiseRadius;
                case PlayerAnimState.Dodge:
                    return _dodgeNoiseRadius;
                case PlayerAnimState.Walk:
                case PlayerAnimState.WalkShoot:
                    return _walkNoiseRadius;
                default:
                    return 0f;
            }
        }
    }
}
