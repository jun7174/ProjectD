// 파일 경로: Assets/_Scripts/World/OccluderProbe.cs
using System.Collections.Generic;
using UnityEngine;

namespace ProjectD.World
{
    /// <summary>
    /// 내 캐릭터를 가리는 물체(Occluder)를 찾아 숨기고, 더 이상 가리지 않으면 되돌린다.
    /// 일정 간격으로 두 가지를 검사한다.
    /// 1. 카메라에서 캐릭터까지 굵은 레이(SphereCast): 앞을 가리는 벽
    /// 2. 캐릭터 머리 위로 레이(Raycast): 덮고 있는 천장
    /// 두 검사에서 찾은 Occluder를 같은 방식으로 다루고, 숨기는 방법은 각 Occluder가 정한다.
    /// 플레이어 프리팹 루트에 붙이고, 내 캐릭터에서만 동작하도록 PlayerNetworkSetup의 Local Only 목록에 넣는다.
    /// </summary>
    public class OccluderProbe : MonoBehaviour
    {
        [Header("검사 주기")]
        [Tooltip("검사 간격 (초)")]
        [Min(0.02f)]
        [SerializeField] private float _checkInterval = 0.1f;

        [Header("벽 검사 (카메라에서 캐릭터까지)")]
        [Tooltip("비워 두면 Main Camera를 쓴다")]
        [SerializeField] private Camera _camera;

        [Tooltip("검사 목표 높이 (m). 캐릭터 발밑 기준, 가슴 정도")]
        [Min(0f)]
        [SerializeField] private float _bodyHeight = 1.2f;

        [Tooltip("굵은 레이의 반지름 (m). 캐릭터가 벽 모서리에 반쯤 가려져도 찾도록 굵게 쏜다")]
        [Min(0.01f)]
        [SerializeField] private float _castRadius = 0.4f;

        [Tooltip("벽 레이어 (Environment)")]
        [SerializeField] private LayerMask _wallMask;

        [Header("천장 검사 (머리 위)")]
        [Tooltip("레이 시작 높이 (m). 캐릭터 발밑 기준, 머리보다 조금 아래")]
        [Min(0f)]
        [SerializeField] private float _roofCheckStartHeight = 1.5f;

        [Tooltip("위로 검사할 거리 (m)")]
        [Min(0.1f)]
        [SerializeField] private float _roofCheckDistance = 10f;

        [Tooltip("천장 레이어 (Roof)")]
        [SerializeField] private LayerMask _roofMask;

        [Header("복구")]
        [Tooltip("검사에서 빠진 뒤 이 시간(초) 동안 계속 빠져 있어야 되돌린다. 문턱에서 깜빡이는 것을 막는다")]
        [Min(0f)]
        [SerializeField] private float _revealDelay = 0.2f;

        // 한 번의 벽 검사에서 맞을 수 있는 최대 개수. 넘으면 일부를 놓치지만 쿼터뷰 시점에서는 충분하다
        private const int MaxWallHits = 16;

        private readonly RaycastHit[] _wallHits = new RaycastHit[MaxWallHits];
        private readonly HashSet<Occluder> _foundThisCheck = new HashSet<Occluder>();

        // 지금 숨겨 둔 Occluder와 마지막으로 검사에서 찾은 시각
        private readonly Dictionary<Occluder, float> _occludedLastSeenTime = new Dictionary<Occluder, float>();
        private readonly List<Occluder> _toReveal = new List<Occluder>();

        // 콜라이더마다 붙어 있는 Occluder를 한 번만 찾고 기억한다. Occluder가 없는 콜라이더는 null로 기억한다
        private readonly Dictionary<Collider, Occluder> _occluderByCollider = new Dictionary<Collider, Occluder>();

        private float _nextCheckTime;

        /// <summary>
        /// 카메라 참조를 찾고 설정을 확인한다.
        /// </summary>
        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_camera == null)
            {
                Debug.LogWarning($"[{nameof(OccluderProbe)}] 카메라를 찾지 못했습니다. 벽 반투명이 동작하지 않습니다.", this);
            }

            if (_wallMask.value == 0 || _roofMask.value == 0)
            {
                Debug.LogWarning($"[{nameof(OccluderProbe)}] Wall Mask 또는 Roof Mask가 비어 있습니다. 인스펙터에서 레이어를 지정해 주세요.", this);
            }
        }

        /// <summary>
        /// 검사 간격마다 가리는 물체를 다시 찾는다.
        /// </summary>
        private void Update()
        {
            if (Time.time < _nextCheckTime)
            {
                return;
            }

            _nextCheckTime = Time.time + _checkInterval;
            CheckOccluders();
        }

        /// <summary>
        /// 꺼지거나 사라질 때(레이드 나가기, 씬 전환) 숨겨 둔 것을 모두 되돌린다.
        /// </summary>
        private void OnDisable()
        {
            RevealAll();
        }

        /// <summary>
        /// 벽과 천장을 검사해서 새로 찾은 것은 숨기고, 일정 시간 계속 빠진 것은 되돌린다.
        /// </summary>
        private void CheckOccluders()
        {
            _foundThisCheck.Clear();
            FindBlockingWalls();
            FindCoveringRoof();

            float now = Time.time;

            foreach (Occluder occluder in _foundThisCheck)
            {
                occluder.Occlude();
                _occludedLastSeenTime[occluder] = now;
            }

            RevealExpired(now);
        }

        /// <summary>
        /// 카메라에서 캐릭터 가슴 높이까지 굵은 레이를 쏴서 그 사이에 있는 Occluder를 찾는다.
        /// 거리를 캐릭터까지로 제한해서 캐릭터 뒤쪽 벽은 찾지 않는다.
        /// </summary>
        private void FindBlockingWalls()
        {
            if (_camera == null)
            {
                return;
            }

            Vector3 origin = _camera.transform.position;
            Vector3 target = transform.position + Vector3.up * _bodyHeight;
            Vector3 toTarget = target - origin;
            float distance = toTarget.magnitude;

            int hitCount = Physics.SphereCastNonAlloc(origin, _castRadius, toTarget / distance, _wallHits, distance, _wallMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                AddIfOccluder(_wallHits[i].collider);
            }
        }

        /// <summary>
        /// 캐릭터 머리 위로 레이를 쏴서 덮고 있는 천장의 Occluder를 찾는다.
        /// </summary>
        private void FindCoveringRoof()
        {
            Vector3 origin = transform.position + Vector3.up * _roofCheckStartHeight;

            if (Physics.Raycast(origin, Vector3.up, out RaycastHit hit, _roofCheckDistance, _roofMask, QueryTriggerInteraction.Ignore))
            {
                AddIfOccluder(hit.collider);
            }
        }

        /// <summary>
        /// 콜라이더에 Occluder가 있으면 이번 검사 결과에 넣는다.
        /// 콜라이더마다 처음 한 번만 부모 방향으로 찾고, 결과는 기억해 두어 다음 검사부터는 찾지 않는다.
        /// </summary>
        /// <param name="hitCollider">검사에 맞은 콜라이더</param>
        private void AddIfOccluder(Collider hitCollider)
        {
            if (!_occluderByCollider.TryGetValue(hitCollider, out Occluder occluder))
            {
                occluder = hitCollider.GetComponentInParent<Occluder>();
                _occluderByCollider[hitCollider] = occluder;
            }

            if (occluder != null)
            {
                _foundThisCheck.Add(occluder);
            }
        }

        /// <summary>
        /// 이번 검사에서 빠졌고, 마지막으로 찾은 지 복구 지연 시간이 지난 Occluder를 되돌린다.
        /// </summary>
        /// <param name="now">현재 시각</param>
        private void RevealExpired(float now)
        {
            _toReveal.Clear();

            foreach (KeyValuePair<Occluder, float> pair in _occludedLastSeenTime)
            {
                bool isGone = pair.Key == null;
                bool isExpired = !_foundThisCheck.Contains(pair.Key) && now - pair.Value >= _revealDelay;

                if (isGone || isExpired)
                {
                    _toReveal.Add(pair.Key);
                }
            }

            // 사전을 돌면서 바로 지울 수 없어서, 지울 목록을 모은 뒤 따로 지운다
            foreach (Occluder occluder in _toReveal)
            {
                if (occluder != null)
                {
                    occluder.Reveal();
                }

                _occludedLastSeenTime.Remove(occluder);
            }
        }

        /// <summary>
        /// 숨겨 둔 Occluder를 모두 되돌린다.
        /// </summary>
        private void RevealAll()
        {
            foreach (Occluder occluder in _occludedLastSeenTime.Keys)
            {
                if (occluder != null)
                {
                    occluder.Reveal();
                }
            }

            _occludedLastSeenTime.Clear();
        }
    }
}
