// 파일 경로: Assets/_Scripts/Network/EnemyHealthScaler.cs
using Photon.Pun;
using ProjectD.Combat;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 방 인원에 맞춰 적의 최대 체력을 정한다. 최대 체력 = EnemyData의 기본 체력 × 인원별 배율.
    /// 마스터가 생성 직후와 인원이 바뀔 때마다 다시 계산해 모두에게 보낸다. 남은 체력 비율은 유지된다.
    /// 죽은 적은 다시 계산하지 않는다.
    /// 적 프리팹 루트에 붙인다.
    /// </summary>
    public class EnemyHealthScaler : MonoBehaviourPunCallbacks
    {
        [Header("데이터")]
        [Tooltip("기본 최대 체력을 읽는다")]
        [SerializeField] private EnemyData _data;

        [Tooltip("인원별 체력 배율을 읽는다")]
        [SerializeField] private EnemyBalanceSettings _balanceSettings;

        [Tooltip("최대 체력을 바꿀 때마다 콘솔에 기록할지 여부. 배율을 확인할 때 켠다")]
        [SerializeField] private bool _logScaling;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private Health _health;
        [SerializeField] private NetworkHealth _networkHealth;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_health == null && !TryGetComponent(out _health))
            {
                Debug.LogWarning($"[{nameof(EnemyHealthScaler)}] Health를 찾지 못했습니다.", this);
            }

            if (_networkHealth == null && !TryGetComponent(out _networkHealth))
            {
                Debug.LogWarning($"[{nameof(EnemyHealthScaler)}] NetworkHealth를 찾지 못했습니다.", this);
            }

            if (_data == null || _balanceSettings == null)
            {
                Debug.LogWarning($"[{nameof(EnemyHealthScaler)}] EnemyData 또는 EnemyBalanceSettings가 비어 있습니다. 인원별 체력이 적용되지 않습니다.", this);
            }
        }

        /// <summary>
        /// 생성 직후 지금 인원으로 최대 체력을 정한다.
        /// </summary>
        private void Start()
        {
            ApplyHealthScale();
        }

        /// <summary>
        /// 누가 들어오면 최대 체력을 다시 정한다.
        /// </summary>
        /// <param name="newPlayer">들어온 플레이어</param>
        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            ApplyHealthScale();
        }

        /// <summary>
        /// 누가 나가면 최대 체력을 다시 정한다.
        /// </summary>
        /// <param name="otherPlayer">나간 플레이어</param>
        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            ApplyHealthScale();
        }

        /// <summary>
        /// 지금 인원으로 최대 체력을 계산해 적용한다. 마스터(방 밖이면 이 화면)만 정한다.
        /// </summary>
        private void ApplyHealthScale()
        {
            bool decides = !PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient;
            if (!decides || _health == null || _health.IsDead || _data == null || _balanceSettings == null)
            {
                return;
            }

            int playerCount = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.PlayerCount : 1;
            float maxHealth = _data.MaxHealth * _balanceSettings.GetHealthMultiplier(playerCount);

            if (PhotonNetwork.InRoom && _networkHealth != null)
            {
                _networkHealth.SetMaxHealthAsMaster(maxHealth);
            }
            else
            {
                _health.SetMaxHealth(maxHealth);
            }

            if (_logScaling)
            {
                Debug.Log($"[{nameof(EnemyHealthScaler)}] {name}: {playerCount}명 기준 최대 체력 {maxHealth:F0}", this);
            }
        }
    }
}
