// 파일 경로: Assets/_Scripts/Player/PlayerAnimationController.cs
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.Player
{
    /// <summary>
    /// 플레이어 상태를 보고 지금 재생할 애니메이션 상태 하나를 우선순위로 정해 Animator에 넣는다.
    /// Animator에 값을 쓰는 유일한 컴포넌트이며, 한 번에 하나의 클립만 재생되도록 State 번호 하나로 제어한다.
    /// 이동, 회피, 정밀 조준은 직접 읽고, 사격과 재장전 정보는 Combat 모듈이 SetShooting, SetReloading으로 전달한다.
    /// Player 루트에 붙이고, Animator는 자식 모델에서 찾는다.
    /// </summary>
    // 이동, 조준, 회피 계산이 끝난 뒤 같은 프레임 안에 상태를 정하도록 기본 스크립트보다 늦게 실행한다
    [DefaultExecutionOrder(10)]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("참조 (비워 두면 자동으로 찾는다)")]
        [Tooltip("자식 모델의 Animator. 비워 두면 자식에서 찾는다")]
        [SerializeField] private Animator _animator;

        [Tooltip("비워 두면 같은 오브젝트에서 찾는다")]
        [SerializeField] private PlayerMover _mover;

        [Tooltip("비워 두면 같은 오브젝트에서 찾는다")]
        [SerializeField] private PlayerAimer _aimer;

        [Tooltip("비워 두면 같은 오브젝트에서 찾는다")]
        [SerializeField] private PlayerDodger _dodger;

        [Header("뒷걸음 판정")]
        [Tooltip("이동 방향과 바라보는 방향의 내적이 이 값보다 작으면 뒤로 이동으로 보고 이동 클립을 역재생한다. -1은 정반대, 0은 정확히 옆")]
        [Range(-1f, 0f)]
        [SerializeField] private float _backwardDotThreshold = -0.3f;

        private bool _isShooting;
        private bool _isReloading;

        /// <summary>
        /// 이번 프레임에 정한 애니메이션 상태. 디버그 확인용이다.
        /// </summary>
        public PlayerAnimState CurrentState { get; private set; }

        /// <summary>
        /// 현재 회피 중인지 여부
        /// </summary>
        private bool IsDodging => _dodger != null && _dodger.IsDodging;

        /// <summary>
        /// 현재 이동 입력으로 움직이는 중인지 여부
        /// </summary>
        private bool IsMoving => _mover != null && _mover.IsMoving;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            FindMissingReferences();
            WarnMissingReferences();
        }

        /// <summary>
        /// 회피 시작 이벤트를 구독한다.
        /// </summary>
        private void OnEnable()
        {
            if (_dodger != null)
            {
                _dodger.OnDodgeStarted += HandleDodgeStarted;
            }
        }

        /// <summary>
        /// 회피 시작 이벤트 구독을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            if (_dodger != null)
            {
                _dodger.OnDodgeStarted -= HandleDodgeStarted;
            }
        }

        /// <summary>
        /// 사격 자세 여부를 설정한다. Combat 모듈이 호출한다.
        /// </summary>
        /// <param name="isShooting">사격 중이면 true</param>
        public void SetShooting(bool isShooting)
        {
            _isShooting = isShooting;
        }

        /// <summary>
        /// 재장전 중 여부를 설정한다. Combat 모듈이 호출한다.
        /// </summary>
        /// <param name="isReloading">재장전 중이면 true</param>
        public void SetReloading(bool isReloading)
        {
            _isReloading = isReloading;
        }

        /// <summary>
        /// 매 프레임 재생할 상태와 이동 클립 방향을 정해서 Animator에 넣는다.
        /// </summary>
        private void Update()
        {
            if (_animator == null)
            {
                return;
            }

            CurrentState = DecideState();
            _animator.SetInteger(AnimatorParams.State, (int)CurrentState);
            _animator.SetFloat(AnimatorParams.MoveDirection, CalculateMoveDirection());
        }

        /// <summary>
        /// 우선순위에 따라 재생할 상태 하나를 정한다.
        /// 1. 회피 중  2. 멈춘 채 재장전 중  3. 사격 중이거나 정밀 조준 중  4. 그 외 이동 상태
        /// </summary>
        /// <returns>재생할 애니메이션 상태</returns>
        private PlayerAnimState DecideState()
        {
            if (IsDodging)
            {
                return PlayerAnimState.Dodge;
            }

            bool isMoving = IsMoving;

            // 재장전 클립은 전신 동작이라 이동 중에 재생하면 미끄러져 보이므로 멈춰 있을 때만 재생한다
            if (_isReloading && !isMoving)
            {
                return PlayerAnimState.Reload;
            }

            bool isInShootStance = _isShooting || (_aimer != null && _aimer.IsAimingPrecise);
            if (isInShootStance)
            {
                return isMoving ? PlayerAnimState.WalkShoot : PlayerAnimState.IdleShoot;
            }

            if (!isMoving)
            {
                return PlayerAnimState.Idle;
            }

            return _mover.IsSprinting ? PlayerAnimState.Run : PlayerAnimState.Walk;
        }

        /// <summary>
        /// 이동 방향이 바라보는 방향의 반대쪽이면 -1, 앞쪽이나 옆쪽이면 1을 반환한다.
        /// 이동 상태들은 이 값을 재생 속도 배율로 써서 뒤로 이동할 때 거꾸로 재생된다.
        /// </summary>
        /// <returns>앞 1, 뒤 -1</returns>
        private float CalculateMoveDirection()
        {
            if (!IsMoving)
            {
                return 1f;
            }

            Vector3 moveDirection = _mover.MoveInputDirection.normalized;
            Vector3 facingDirection = transform.forward;
            facingDirection.y = 0f;
            facingDirection.Normalize();

            float dot = Vector3.Dot(moveDirection, facingDirection);
            return dot < _backwardDotThreshold ? -1f : 1f;
        }

        /// <summary>
        /// 회피가 시작되면 회피 클립을 처음부터 다시 재생한다.
        /// 연속으로 회피하면 State 값이 회피로 유지되어 전환이 일어나지 않기 때문에, 이 경우만 상태를 직접 재생한다.
        /// </summary>
        private void HandleDodgeStarted()
        {
            if (_animator == null)
            {
                return;
            }

            CurrentState = PlayerAnimState.Dodge;
            _animator.SetInteger(AnimatorParams.State, (int)PlayerAnimState.Dodge);
            _animator.Play(AnimatorStates.Dodge, 0, 0f);
        }

        /// <summary>
        /// 인스펙터에서 비어 있는 참조를 찾는다. Animator는 자식 모델에서, 나머지는 같은 오브젝트에서 찾는다.
        /// </summary>
        private void FindMissingReferences()
        {
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }

            if (_mover == null)
            {
                _mover = GetComponent<PlayerMover>();
            }

            if (_aimer == null)
            {
                _aimer = GetComponent<PlayerAimer>();
            }

            if (_dodger == null)
            {
                _dodger = GetComponent<PlayerDodger>();
            }
        }

        /// <summary>
        /// 찾지 못한 참조가 있으면 어떤 기능이 동작하지 않는지 경고를 출력한다.
        /// </summary>
        private void WarnMissingReferences()
        {
            if (_animator == null)
            {
                Debug.LogWarning($"[{nameof(PlayerAnimationController)}] 자식에서 Animator를 찾지 못했습니다. 애니메이션이 재생되지 않습니다.", this);
            }

            if (_mover == null)
            {
                Debug.LogWarning($"[{nameof(PlayerAnimationController)}] PlayerMover를 찾지 못했습니다. 이동 애니메이션이 동작하지 않습니다.", this);
            }

            if (_aimer == null)
            {
                Debug.LogWarning($"[{nameof(PlayerAnimationController)}] PlayerAimer를 찾지 못했습니다. 정밀 조준 자세가 동작하지 않습니다.", this);
            }

            if (_dodger == null)
            {
                Debug.LogWarning($"[{nameof(PlayerAnimationController)}] PlayerDodger를 찾지 못했습니다. 회피 애니메이션이 동작하지 않습니다.", this);
            }
        }
    }
}
