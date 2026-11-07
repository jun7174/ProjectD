// 파일 경로: Assets/_Scripts/AI/EnemyWeapon.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectD.AI
{
    /// <summary>
    /// 적의 공격 수단. 돌진 중 몸에 닿은 대상을 찾고, 공격 결과(근접 명중, 발사)를 이벤트로 알린다.
    /// 데미지를 주거나 투사체를 만드는 일은 하지 않는다. 그 일은 이벤트를 구독한 네트워크 쪽(EnemyAttackSync)이 맡아,
    /// AI는 네트워크와 투사체 구현을 몰라도 된다.
    /// 적 프리팹 루트에 붙인다.
    /// </summary>
    public class EnemyWeapon : MonoBehaviour
    {
        [Header("몸 접촉")]
        [Tooltip("돌진 중 닿았다고 보는 반경 (m). 몸 중심 기준")]
        [Min(0.1f)]
        [SerializeField] private float _contactRadius = 0.9f;

        [Tooltip("몸 중심 높이 (m). 발밑 기준")]
        [Min(0f)]
        [SerializeField] private float _bodyCenterHeight = 1f;

        [Tooltip("닿을 수 있는 대상의 레이어 (Player)")]
        [SerializeField] private LayerMask _targetMask;

        [Header("발사")]
        [Tooltip("탄이 나가는 위치. 비워 두면 몸 중심에서 쏜다")]
        [SerializeField] private Transform _muzzle;

        // 한 번의 접촉 검사에서 걸릴 수 있는 최대 콜라이더 수. 플레이어가 최대 4명이라 충분하다
        private const int MaxContacts = 8;

        private readonly Collider[] _contactBuffer = new Collider[MaxContacts];

        /// <summary>
        /// 돌진 중 대상에 닿았을 때 발생한다. 인자는 닿은 대상이다.
        /// </summary>
        public event Action<EnemyTarget> OnMeleeHit;

        /// <summary>
        /// 탄 한 발을 쐈을 때 발생한다. 인자는 발사 방향이고, 시작 위치는 MuzzlePosition이다.
        /// </summary>
        public event Action<Vector3> OnFired;

        /// <summary>
        /// 탄이 나가는 위치
        /// </summary>
        public Vector3 MuzzlePosition => _muzzle != null ? _muzzle.position : transform.position + Vector3.up * _bodyCenterHeight;

        /// <summary>
        /// 대상 레이어가 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_targetMask.value == 0)
            {
                Debug.LogWarning($"[{nameof(EnemyWeapon)}] Target Mask가 비어 있습니다. 돌진이 플레이어에게 닿지 않습니다.", this);
            }
        }

        /// <summary>
        /// 지금 몸에 닿아 있는 노릴 수 있는 대상을 모두 찾아 목록에 담는다. 목록은 먼저 비운다.
        /// </summary>
        /// <param name="results">찾은 대상을 담을 목록</param>
        public void CollectTouchingTargets(List<EnemyTarget> results)
        {
            results.Clear();

            Vector3 center = transform.position + Vector3.up * _bodyCenterHeight;
            int count = Physics.OverlapSphereNonAlloc(center, _contactRadius, _contactBuffer, _targetMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                EnemyTarget target = _contactBuffer[i].GetComponentInParent<EnemyTarget>();
                if (target != null && target.IsTargetable && !results.Contains(target))
                {
                    results.Add(target);
                }
            }
        }

        /// <summary>
        /// 대상에게 몸으로 명중했다고 알린다.
        /// </summary>
        /// <param name="target">맞은 대상</param>
        public void ReportMeleeHit(EnemyTarget target)
        {
            OnMeleeHit?.Invoke(target);
        }

        /// <summary>
        /// 탄 한 발을 쐈다고 알린다.
        /// </summary>
        /// <param name="direction">발사 방향</param>
        public void Fire(Vector3 direction)
        {
            OnFired?.Invoke(direction.normalized);
        }

        /// <summary>
        /// 선택했을 때 Scene 뷰에 몸 접촉 범위를 그린다.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * _bodyCenterHeight, _contactRadius);
        }
    }
}
