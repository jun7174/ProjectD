// 파일 경로: Assets/_Scripts/Combat/ProjectileLauncher.cs
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 발사 정보를 받아 풀에서 투사체를 꺼내 날리고, 명중하면 데미지를 전달한다.
    /// 다른 클라이언트에서 받은 발사 정보로 시각용 투사체만 날릴 때도 이 컴포넌트를 쓴다.
    /// 그때는 데미지 판정을 하지 않도록 Deals Damage를 끈다.
    /// </summary>
    public class ProjectileLauncher : MonoBehaviour
    {
        [Tooltip("투사체를 꺼낼 풀. 비워 두면 씬에서 찾는다")]
        [SerializeField] private ProjectilePool _projectilePool;

        [Tooltip("투사체가 명중 검사를 할 레이어 (벽, 바닥, 적 등)")]
        [SerializeField] private LayerMask _hitMask;

        [Tooltip("명중 시 데미지를 줄지 여부. 판정 권한이 없는 시각용 발사는 끈다")]
        [SerializeField] private bool _dealsDamage = true;

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
        /// 발사 정보로 투사체 한 발을 날린다.
        /// </summary>
        /// <param name="fireInfo">발사 정보</param>
        public void Launch(FireInfo fireInfo)
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

            projectile.Launch(fireInfo, _hitMask, _dealsDamage ? HandleHit : null, _projectilePool.Release);
        }

        /// <summary>
        /// 투사체가 맞은 대상이 데미지를 받을 수 있으면 무기 데미지를 전달한다.
        /// </summary>
        /// <param name="projectile">명중한 투사체</param>
        /// <param name="hit">명중 정보</param>
        private void HandleHit(Projectile projectile, RaycastHit hit)
        {
            // 콜라이더가 자식에 붙어 있는 경우도 있어 부모 방향으로 찾는다
            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                return;
            }

            damageable.TakeDamage(projectile.FireInfo.Weapon.Damage, hit.point);
        }
    }
}
