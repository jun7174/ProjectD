// 파일 경로: Assets/_Scripts/Player/PlayerCameraTargetOffset.cs
using UnityEngine;

namespace ProjectD.Player
{
    /// <summary>
    /// 정밀 조준 중에 카메라 타깃을 조준점 방향으로 밀어서, 카메라가 마우스 쪽을 더 멀리 보여주게 한다.
    /// 플레이어의 자식 오브젝트인 카메라 타깃에 붙인다. 로컬 플레이어 전용이다.
    /// </summary>
    // 이동과 조준 계산이 끝난 뒤, 카메라가 갱신되기 전에 위치를 정하기 위해 기본 스크립트보다 늦게 실행한다
    [DefaultExecutionOrder(10)]
    public class PlayerCameraTargetOffset : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("조준점과 정밀 조준 상태를 읽을 대상. 비워 두면 부모 오브젝트에서 찾는다")]
        [SerializeField] private PlayerAimer _aimer;

        [Header("오프셋")]
        [Tooltip("조준점까지 거리 중 카메라 타깃을 얼마만큼 밀지 비율. 1에 가까우면 카메라가 계속 밀려나므로 0.9 이하로 둔다")]
        [Range(0f, 0.9f)]
        [SerializeField] private float _offsetRatio = 0.5f;

        [Tooltip("카메라 타깃이 플레이어에서 최대로 멀어질 수 있는 거리 (m)")]
        [SerializeField] private float _maxOffsetDistance = 4f;

        [Tooltip("오프셋이 목표값까지 따라가는 데 걸리는 대략적인 시간 (초)")]
        [SerializeField] private float _smoothTime = 0.15f;

        private float _heightOffset;
        private Vector3 _currentOffset;
        private Vector3 _offsetVelocity;

        /// <summary>
        /// 참조를 확인하고, 처음 배치된 높이를 기준 높이로 저장한다.
        /// </summary>
        private void Awake()
        {
            if (_aimer == null)
            {
                _aimer = GetComponentInParent<PlayerAimer>();
            }

            if (_aimer == null)
            {
                Debug.LogWarning($"[{nameof(PlayerCameraTargetOffset)}] 부모에서 PlayerAimer를 찾지 못했습니다. 카메라 타깃이 움직이지 않습니다.", this);
            }

            // 플레이어는 Y축으로만 회전하므로 로컬 Y 위치가 곧 플레이어 기준 높이다
            _heightOffset = transform.localPosition.y;
        }

        /// <summary>
        /// 매 프레임 목표 오프셋을 계산하고, 부드럽게 따라가며 카메라 타깃 위치를 정한다.
        /// </summary>
        private void Update()
        {
            if (_aimer == null)
            {
                return;
            }

            Vector3 targetOffset = CalculateTargetOffset();
            _currentOffset = Vector3.SmoothDamp(_currentOffset, targetOffset, ref _offsetVelocity, _smoothTime);

            // 플레이어가 조준 방향으로 회전해도 오프셋이 같이 돌지 않도록 월드 좌표로 위치를 정한다
            Vector3 playerPosition = _aimer.transform.position;
            transform.position = playerPosition + Vector3.up * _heightOffset + _currentOffset;
        }

        /// <summary>
        /// 정밀 조준 중이면 조준점 방향으로 밀어낼 오프셋을, 아니면 0을 계산한다.
        /// </summary>
        /// <returns>플레이어 위치 기준 수평 오프셋</returns>
        private Vector3 CalculateTargetOffset()
        {
            if (!_aimer.IsAimingPrecise)
            {
                return Vector3.zero;
            }

            Vector3 toAimPoint = _aimer.AimPoint - _aimer.transform.position;
            toAimPoint.y = 0f;

            return Vector3.ClampMagnitude(toAimPoint * _offsetRatio, _maxOffsetDistance);
        }
    }
}
