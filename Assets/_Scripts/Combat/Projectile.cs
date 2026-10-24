// 파일 경로: Assets/_Scripts/Combat/Projectile.cs
using System;
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 날아가는 투사체 한 발. 매 프레임 이전 위치에서 다음 위치까지를 레이캐스트로 검사해서,
    /// 빠른 탄도 얇은 벽을 뚫지 않게 한다. 명중이나 사거리 초과 시 풀로 돌아간다.
    /// 명중 후 데미지를 줄지는 투사체가 아니라 발사한 쪽이 정한다.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Tooltip("궤적 표시용 트레일. 없으면 비워 둔다")]
        [SerializeField] private TrailRenderer _trailRenderer;

        private FireInfo _fireInfo;
        private LayerMask _hitMask;
        private float _traveledDistance;
        private bool _isFlying;
        private Action<Projectile, RaycastHit> _onHit;
        private Action<Projectile> _onFinished;

        /// <summary>
        /// 이 투사체를 만든 발사 정보
        /// </summary>
        public FireInfo FireInfo => _fireInfo;

        /// <summary>
        /// 트레일 참조가 비어 있으면 같은 오브젝트에서 찾는다.
        /// </summary>
        private void Awake()
        {
            if (_trailRenderer == null)
            {
                _trailRenderer = GetComponent<TrailRenderer>();
            }
        }

        /// <summary>
        /// 발사 정보에 따라 투사체를 날린다.
        /// </summary>
        /// <param name="fireInfo">시작 위치, 방향, 무기 정보</param>
        /// <param name="hitMask">명중 검사를 할 레이어</param>
        /// <param name="onHit">무언가에 맞았을 때 호출할 함수. 시각용 투사체는 null</param>
        /// <param name="onFinished">비행이 끝났을 때 호출할 함수. 보통 풀로 돌려보내는 함수</param>
        public void Launch(FireInfo fireInfo, LayerMask hitMask, Action<Projectile, RaycastHit> onHit, Action<Projectile> onFinished)
        {
            _fireInfo = fireInfo;
            _hitMask = hitMask;
            _onHit = onHit;
            _onFinished = onFinished;
            _traveledDistance = 0f;
            _isFlying = true;

            transform.SetPositionAndRotation(fireInfo.Origin, Quaternion.LookRotation(fireInfo.Direction));

            // 풀에서 재사용할 때 이전 위치에서 새 위치까지 트레일이 이어지지 않도록 지운다
            if (_trailRenderer != null)
            {
                _trailRenderer.Clear();
            }
        }

        /// <summary>
        /// 매 프레임 이동할 구간을 검사하고, 명중하지 않았으면 이동한다.
        /// </summary>
        private void Update()
        {
            if (!_isFlying)
            {
                return;
            }

            float remainingDistance = _fireInfo.Weapon.MaxRange - _traveledDistance;
            float stepDistance = Mathf.Min(_fireInfo.Weapon.ProjectileSpeed * Time.deltaTime, remainingDistance);

            if (Physics.Raycast(transform.position, _fireInfo.Direction, out RaycastHit hit, stepDistance, _hitMask, QueryTriggerInteraction.Ignore))
            {
                transform.position = hit.point;
                _onHit?.Invoke(this, hit);
                Finish();
                return;
            }

            transform.position += _fireInfo.Direction * stepDistance;
            _traveledDistance += stepDistance;

            if (_traveledDistance >= _fireInfo.Weapon.MaxRange)
            {
                Finish();
            }
        }

        /// <summary>
        /// 비행을 끝내고 종료 함수를 한 번만 호출한다.
        /// </summary>
        private void Finish()
        {
            if (!_isFlying)
            {
                return;
            }

            _isFlying = false;
            _onFinished?.Invoke(this);
        }
    }
}
