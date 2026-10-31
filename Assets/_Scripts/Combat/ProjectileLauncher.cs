// 파일 경로: Assets/_Scripts/Combat/ProjectileLauncher.cs
using System;
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 발사 정보를 받아 풀에서 투사체를 꺼내 날린다.
    /// Launch는 판정용 발사로, 맞으면 명중 처리(IHitHandler)를 호출한다.
    /// LaunchVisualOnly는 다른 플레이어가 쏜 탄을 내 화면에 보여 주기만 하는 시각용 발사다 (명중 처리 없음).
    /// 명중 처리는 기본으로 바로 데미지를 주고(Sandbox), 네트워크 방 안에서는 SetHitHandler로 마스터 보고 방식으로 교체된다.
    /// </summary>
    public class ProjectileLauncher : MonoBehaviour
    {
        [Tooltip("투사체를 꺼낼 풀. 비워 두면 씬에서 찾는다")]
        [SerializeField] private ProjectilePool _projectilePool;

        [Tooltip("투사체가 명중 검사를 할 레이어 (벽, 바닥, 적 등)")]
        [SerializeField] private LayerMask _hitMask;

        // 판정용 투사체가 맞았을 때 할 일. 기본은 바로 데미지
        private IHitHandler _hitHandler = new DirectDamageHitHandler();

        /// <summary>
        /// 풀 참조를 확인한다.
        /// </summary>
        private void Awake()
        {
            if (_projectilePool == null)
            {
                _projectilePool = FindFirstObjectByType<ProjectilePool>();
            }

            if (_projectilePool == null)
            {
                Debug.LogWarning($"[{nameof(ProjectileLauncher)}] 씬에서 ProjectilePool을 찾지 못했습니다. 투사체가 발사되지 않습니다.", this);
            }
        }

        /// <summary>
        /// 판정용 투사체가 맞았을 때 할 일을 바꾼다. null을 넣으면 기본(바로 데미지)으로 되돌린다.
        /// 네트워크 방 안에서 PlayerHitReporter가 자신을 넣는다.
        /// </summary>
        /// <param name="hitHandler">새 명중 처리</param>
        public void SetHitHandler(IHitHandler hitHandler)
        {
            _hitHandler = hitHandler ?? new DirectDamageHitHandler();
        }

        /// <summary>
        /// 판정용 투사체 한 발을 날린다. 맞으면 명중 처리를 호출한다.
        /// </summary>
        /// <param name="fireInfo">발사 정보</param>
        public void Launch(FireInfo fireInfo)
        {
            LaunchProjectile(fireInfo, true);
        }

        /// <summary>
        /// 시각용 투사체 한 발을 날린다. 벽이나 대상에 닿으면 멈추지만 명중 처리는 하지 않는다.
        /// 다른 플레이어가 쏜 탄을 내 화면에 보여 줄 때 쓴다.
        /// </summary>
        /// <param name="fireInfo">발사 정보</param>
        public void LaunchVisualOnly(FireInfo fireInfo)
        {
            LaunchProjectile(fireInfo, false);
        }

        /// <summary>
        /// 풀에서 투사체를 꺼내 날린다.
        /// </summary>
        /// <param name="fireInfo">발사 정보</param>
        /// <param name="handlesHit">맞았을 때 명중 처리를 할지 여부. 시각용이면 false</param>
        private void LaunchProjectile(FireInfo fireInfo, bool handlesHit)
        {
            if (_projectilePool == null || fireInfo.Weapon == null)
            {
                return;
            }

            Projectile projectile = _projectilePool.Get();
            if (projectile == null)
            {
                return;
            }

            // 시각용이면 명중 콜백을 넘기지 않아, 투사체가 닿아도 멈추기만 한다
            Action<Projectile, RaycastHit> onHit = handlesHit ? HandleHit : (Action<Projectile, RaycastHit>)null;
            projectile.Launch(fireInfo, _hitMask, onHit, _projectilePool.Release);
        }

        /// <summary>
        /// 판정용 투사체가 맞으면 현재 명중 처리에 넘긴다.
        /// </summary>
        /// <param name="projectile">명중한 투사체</param>
        /// <param name="hit">명중 정보</param>
        private void HandleHit(Projectile projectile, RaycastHit hit)
        {
            _hitHandler.HandleHit(projectile.FireInfo, hit);
        }
    }
}
