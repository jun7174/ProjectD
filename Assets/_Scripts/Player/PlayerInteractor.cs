// 파일 경로: Assets/_Scripts/Player/PlayerInteractor.cs
using System;
using System.Collections.Generic;
using ProjectD.Core;
using UnityEngine;

namespace ProjectD.Player
{
    /// <summary>
    /// 주변의 상호작용 대상 중 하나를 골라 강조하고, F 입력에 맞춰 실행한다.
    /// 일정 간격으로 반경 안의 Interactable 레이어를 찾아, 우선순위가 높은 대상을, 같으면 가까운 대상을 고른다.
    /// 누르고 있어야 하는 시간이 0인 대상(줍기)은 누르는 즉시, 아니면(상자 열기) 그 시간만큼 누르고 있어야 실행된다.
    /// 누르는 중 대상이 바뀌거나 회피하면 처음부터 다시 눌러야 한다.
    /// 플레이어 프리팹 루트에 붙이고 PlayerNetworkSetup의 Local Only 목록에 넣는다.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [Tooltip("대상을 찾는 반경 (m)")]
        [Min(0.1f)]
        [SerializeField] private float _radius = 2f;

        [Tooltip("대상을 다시 찾는 간격 (초)")]
        [Min(0.02f)]
        [SerializeField] private float _scanInterval = 0.1f;

        [Tooltip("상호작용 대상의 레이어 (Interactable)")]
        [SerializeField] private LayerMask _interactableMask;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private PlayerDodger _dodger;

        // 반경 안에 동시에 들어올 수 있는 대상 콜라이더 수. 넘는 대상은 이번 검사에서 무시한다
        private const int MaxCandidates = 16;

        private readonly Collider[] _overlapBuffer = new Collider[MaxCandidates];

        // 콜라이더마다 IInteractable을 한 번만 찾고 기억한다
        private readonly Dictionary<Collider, IInteractable> _interactableByCollider = new Dictionary<Collider, IInteractable>();

        private IInteractable _currentTarget;
        private float _nextScanTime;
        private bool _isHeld;
        private bool _waitForRelease;
        private float _holdTime;

        /// <summary>
        /// 지금 고른 대상. 없거나 사라졌으면 null
        /// </summary>
        public IInteractable CurrentTarget => IsAlive(_currentTarget) ? _currentTarget : null;

        /// <summary>
        /// 길게 누르는 진행도 (0~1). 즉시 실행 대상이나 누르지 않을 때는 0
        /// </summary>
        public float HoldProgress
        {
            get
            {
                IInteractable target = CurrentTarget;
                if (target == null || target.HoldDuration <= 0f || _waitForRelease)
                {
                    return 0f;
                }

                return Mathf.Clamp01(_holdTime / target.HoldDuration);
            }
        }

        /// <summary>
        /// 고른 대상이 바뀌면 발생한다. 인자는 새 대상이고, 없으면 null이다.
        /// </summary>
        public event Action<IInteractable> OnTargetChanged;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_dodger == null && !TryGetComponent(out _dodger))
            {
                Debug.LogWarning($"[{nameof(PlayerInteractor)}] PlayerDodger를 찾지 못했습니다. 회피해도 길게 누르기가 취소되지 않습니다.", this);
            }

            if (_interactableMask.value == 0)
            {
                Debug.LogWarning($"[{nameof(PlayerInteractor)}] Interactable Mask가 비어 있습니다. 상호작용 대상을 찾지 못합니다.", this);
            }
        }

        /// <summary>
        /// 꺼질 때 고른 대상의 강조를 끄고 비운다.
        /// </summary>
        private void OnDisable()
        {
            SetTarget(null);
            _isHeld = false;
        }

        /// <summary>
        /// F를 누르고 있는지 받는다. 입력 담당(InteractionInputReader)이 매 프레임 호출한다.
        /// </summary>
        /// <param name="isHeld">누르고 있으면 true</param>
        public void SetInteractHeld(bool isHeld)
        {
            _isHeld = isHeld;
        }

        /// <summary>
        /// 일정 간격으로 대상을 다시 고르고, 매 프레임 누르기 진행을 처리한다.
        /// </summary>
        private void Update()
        {
            if (Time.time >= _nextScanTime)
            {
                _nextScanTime = Time.time + _scanInterval;
                SetTarget(FindBestTarget());
            }

            UpdateHold(Time.deltaTime);
        }

        /// <summary>
        /// 누르고 있는 시간을 재고, 대상의 시간이 되면 실행한다. 한 번 실행하면 손을 뗄 때까지 다시 실행하지 않는다.
        /// </summary>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        private void UpdateHold(float deltaTime)
        {
            if (!_isHeld)
            {
                _waitForRelease = false;
                _holdTime = 0f;
                return;
            }

            IInteractable target = CurrentTarget;
            if (_waitForRelease || target == null || !target.CanInteract)
            {
                return;
            }

            if (_dodger != null && _dodger.IsDodging)
            {
                _holdTime = 0f;
                _waitForRelease = true;
                return;
            }

            _holdTime += deltaTime;
            if (_holdTime >= target.HoldDuration)
            {
                _waitForRelease = true;
                _holdTime = 0f;
                target.Interact(gameObject);
            }
        }

        /// <summary>
        /// 반경 안의 대상 중 상호작용할 수 있고, 우선순위가 가장 높고, 같으면 가장 가까운 대상을 고른다.
        /// </summary>
        /// <returns>고른 대상. 없으면 null</returns>
        private IInteractable FindBestTarget()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, _radius, _overlapBuffer, _interactableMask, QueryTriggerInteraction.Collide);

            IInteractable best = null;
            float bestSqrDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                IInteractable candidate = GetInteractable(_overlapBuffer[i]);
                if (!IsAlive(candidate) || !candidate.CanInteract)
                {
                    continue;
                }

                float sqrDistance = (candidate.Position - transform.position).sqrMagnitude;
                bool isHigherPriority = best == null || candidate.Priority > best.Priority;
                bool isSamePriorityCloser = best != null && candidate.Priority == best.Priority && sqrDistance < bestSqrDistance;

                if (isHigherPriority || isSamePriorityCloser)
                {
                    best = candidate;
                    bestSqrDistance = sqrDistance;
                }
            }

            return best;
        }

        /// <summary>
        /// 고른 대상을 바꾼다. 이전 대상의 강조를 끄고 새 대상의 강조를 켜고, 누르기를 처음부터 다시 하게 한다.
        /// </summary>
        /// <param name="target">새 대상. 없으면 null</param>
        private void SetTarget(IInteractable target)
        {
            if (target == _currentTarget)
            {
                return;
            }

            SetHighlighted(_currentTarget, false);
            _currentTarget = target;
            SetHighlighted(_currentTarget, true);

            _holdTime = 0f;
            _waitForRelease = _isHeld;
            OnTargetChanged?.Invoke(CurrentTarget);
        }

        /// <summary>
        /// 대상에 강조 표시 컴포넌트가 있으면 켜거나 끈다.
        /// </summary>
        /// <param name="target">대상</param>
        /// <param name="isHighlighted">켜려면 true</param>
        private static void SetHighlighted(IInteractable target, bool isHighlighted)
        {
            if (IsAlive(target) && ((Component)target).TryGetComponent(out InteractionHighlight highlight))
            {
                highlight.SetHighlighted(isHighlighted);
            }
        }

        /// <summary>
        /// 콜라이더에 붙은 IInteractable을 찾는다. 콜라이더마다 처음 한 번만 찾고 기억한다.
        /// </summary>
        /// <param name="targetCollider">검사에 걸린 콜라이더</param>
        /// <returns>IInteractable. 없으면 null</returns>
        private IInteractable GetInteractable(Collider targetCollider)
        {
            if (!_interactableByCollider.TryGetValue(targetCollider, out IInteractable interactable))
            {
                interactable = targetCollider.GetComponentInParent<IInteractable>();
                _interactableByCollider[targetCollider] = interactable;
            }

            return interactable;
        }

        /// <summary>
        /// 대상이 아직 있는지 확인한다. 파괴된 오브젝트는 인터페이스로 null 비교가 되지 않아 Unity 오브젝트로 바꿔 확인한다.
        /// </summary>
        /// <param name="target">확인할 대상</param>
        /// <returns>있으면 true</returns>
        private static bool IsAlive(IInteractable target)
        {
            return target is UnityEngine.Object unityObject && unityObject != null;
        }
    }
}
