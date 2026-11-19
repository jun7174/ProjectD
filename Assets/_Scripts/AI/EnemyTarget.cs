// 파일 경로: Assets/_Scripts/AI/EnemyTarget.cs
using System.Collections.Generic;
using UnityEngine;

namespace ProjectD.AI
{
    /// <summary>
    /// 적이 보고 쫓을 수 있는 대상(플레이어)임을 나타낸다.
    /// 적은 Player 레이어의 콜라이더에서 이 컴포넌트를 찾아 시야 검사와 조준의 목표 위치로 쓴다.
    /// 쓰러진 동안처럼 노리면 안 되는 때는 SetTargetable(false)로 대상에서 빠진다.
    /// 켜져 있는 대상 목록(ActiveTargets)을 들고 있어, 휴면 검사처럼 모든 플레이어의 위치가 필요할 때 씬을 뒤지지 않아도 된다.
    /// 마스터에서 다른 플레이어의 캐릭터도 대상이 되어야 하므로 내 캐릭터 전용 목록에 넣지 않는다.
    /// 플레이어 프리팹 루트에 붙인다.
    /// </summary>
    public class EnemyTarget : MonoBehaviour
    {
        // 지금 켜져 있는 모든 대상. 켜질 때 넣고 꺼질 때 뺀다
        private static readonly List<EnemyTarget> _activeTargets = new List<EnemyTarget>();

        [Tooltip("적이 바라볼 높이 (m). 캐릭터 발밑 기준, 가슴 정도")]
        [Min(0f)]
        [SerializeField] private float _aimHeight = 1.2f;

        /// <summary>
        /// 지금 켜져 있는 모든 대상
        /// </summary>
        public static IReadOnlyList<EnemyTarget> ActiveTargets => _activeTargets;

        /// <summary>
        /// 적이 시야 검사와 조준에 쓰는 위치 (가슴 높이)
        /// </summary>
        public Vector3 AimPoint => transform.position + Vector3.up * _aimHeight;

        /// <summary>
        /// 적이 노릴 수 있는지 여부. false면 보여도 감지하지 않는다
        /// </summary>
        public bool IsTargetable { get; private set; } = true;

        /// <summary>
        /// 켜질 때 대상 목록에 넣는다.
        /// </summary>
        private void OnEnable()
        {
            _activeTargets.Add(this);
        }

        /// <summary>
        /// 꺼질 때 대상 목록에서 뺀다.
        /// </summary>
        private void OnDisable()
        {
            _activeTargets.Remove(this);
        }

        /// <summary>
        /// 적이 노릴 수 있는지를 바꾼다.
        /// </summary>
        /// <param name="isTargetable">노릴 수 있으면 true</param>
        public void SetTargetable(bool isTargetable)
        {
            IsTargetable = isTargetable;
        }
    }
}
