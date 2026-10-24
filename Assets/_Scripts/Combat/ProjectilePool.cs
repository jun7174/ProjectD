// 파일 경로: Assets/_Scripts/Combat/ProjectilePool.cs
using UnityEngine;
using UnityEngine.Pool;

namespace ProjectD.Combat
{
    /// <summary>
    /// 투사체를 미리 만들어 두고 재사용하는 풀.
    /// 연사할 때 생성과 삭제가 반복되지 않도록 Unity 내장 ObjectPool을 사용한다.
    /// 씬에 하나 두고 모든 발사 담당이 공유한다.
    /// </summary>
    public class ProjectilePool : MonoBehaviour
    {
        [Tooltip("풀에서 만들 투사체 프리팹")]
        [SerializeField] private Projectile _projectilePrefab;

        [Tooltip("처음에 준비해 둘 개수")]
        [Min(0)]
        [SerializeField] private int _defaultCapacity = 64;

        [Tooltip("풀에 보관할 최대 개수. 넘치는 투사체는 반환될 때 삭제된다")]
        [Min(1)]
        [SerializeField] private int _maxSize = 256;

        private ObjectPool<Projectile> _pool;

        /// <summary>
        /// 프리팹을 확인하고 풀을 만든다.
        /// </summary>
        private void Awake()
        {
            if (_projectilePrefab == null)
            {
                Debug.LogWarning($"[{nameof(ProjectilePool)}] 투사체 프리팹이 비어 있습니다. 인스펙터에서 연결하세요.", this);
                return;
            }

            _pool = new ObjectPool<Projectile>(
                CreateProjectile,
                OnGetProjectile,
                OnReleaseProjectile,
                OnDestroyProjectile,
                true,
                _defaultCapacity,
                _maxSize);
        }

        /// <summary>
        /// 풀에서 투사체를 하나 꺼낸다.
        /// </summary>
        /// <returns>사용할 투사체. 풀이 준비되지 않았으면 null</returns>
        public Projectile Get()
        {
            return _pool?.Get();
        }

        /// <summary>
        /// 다 쓴 투사체를 풀로 돌려보낸다.
        /// </summary>
        /// <param name="projectile">돌려보낼 투사체</param>
        public void Release(Projectile projectile)
        {
            _pool?.Release(projectile);
        }

        /// <summary>
        /// 풀이 비었을 때 새 투사체를 만든다. 만든 투사체는 정리를 위해 풀 오브젝트의 자식으로 둔다.
        /// </summary>
        /// <returns>새로 만든 투사체</returns>
        private Projectile CreateProjectile()
        {
            return Instantiate(_projectilePrefab, transform);
        }

        /// <summary>
        /// 풀에서 꺼낼 때 투사체를 켠다.
        /// </summary>
        /// <param name="projectile">꺼낸 투사체</param>
        private void OnGetProjectile(Projectile projectile)
        {
            projectile.gameObject.SetActive(true);
        }

        /// <summary>
        /// 풀로 돌아올 때 투사체를 끈다.
        /// </summary>
        /// <param name="projectile">돌아온 투사체</param>
        private void OnReleaseProjectile(Projectile projectile)
        {
            projectile.gameObject.SetActive(false);
        }

        /// <summary>
        /// 풀이 가득 찬 상태에서 돌아온 투사체를 삭제한다.
        /// </summary>
        /// <param name="projectile">삭제할 투사체</param>
        private void OnDestroyProjectile(Projectile projectile)
        {
            Destroy(projectile.gameObject);
        }
    }
}
