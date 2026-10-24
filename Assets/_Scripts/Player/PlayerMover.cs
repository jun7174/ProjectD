// 파일 경로: Assets/_Scripts/Player/PlayerMover.cs
using UnityEngine;

namespace ProjectD.Player
{
    /// <summary>
    /// CharacterController로 플레이어의 걷기, 달리기, 중력을 처리한다.
    /// 입력을 직접 읽지 않고, 외부에서 전달받은 이동 입력과 달리기 요청으로만 움직인다.
    /// 정밀 조준 중에는 감속하고, 회피 중에는 수평 이동을 PlayerDodger에 맡긴다.
    /// 로컬 플레이어는 PlayerInputReader가,원격 플레이어는 네트워크 담당이 값을 전달한다.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMover : MonoBehaviour
    {
        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [Tooltip("달리기에 사용할 스태미나")]
        [SerializeField] private PlayerStamina _stamina;

        [Tooltip("정밀 조준 상태를 읽을 대상")]
        [SerializeField] private PlayerAimer _aimer;

        [Tooltip("회피 상태를 읽을 대상")]
        [SerializeField] private PlayerDodger _dodger;

        [Header("이동 속도")]
        [Tooltip("걷기 속도 (m/s)")]
        [SerializeField] private float _walkSpeed = 3.5f;

        [Tooltip("달리기 속도 (m/s)")]
        [SerializeField] private float _sprintSpeed = 6f;

        [Tooltip("정밀 조준 중 이동 속도 배율")]
        [Range(0f, 1f)]
        [SerializeField] private float _preciseAimSpeedMultiplier = 0.6f;

        [Header("달리기")]
        [Tooltip("달리는 동안 초당 소모하는 스태미나")]
        [SerializeField] private float _sprintStaminaCostPerSecond = 15f;

        [Header("중력")]
        [Tooltip("중력 가속도 (m/s^2). 아래 방향이므로 음수로 입력")]
        [SerializeField] private float _gravity = -20f;

        [Tooltip("바닥에 서 있을 때 캐릭터를 바닥에 붙여 두기 위해 유지하는 아래 방향 속도")]
        [SerializeField] private float _groundedVerticalVelocity = -2f;

        // 이 값보다 작은 입력은 이동 입력이 없는 것으로 본다
        private const float MoveInputDeadZone = 0.01f;

        private CharacterController _characterController;
        private Vector2 _moveInput;
        private bool _isSprintRequested;
        private bool _isSprintBlockedUntilRelease;
        private bool _isSprintSuppressed;
        private float _verticalVelocity;

        /// <summary>
        /// 현재 달리고 있는지 여부. 애니메이션, 탄퍼짐 계산 등에서 읽는다.
        /// </summary>
        public bool IsSprinting { get; private set; }

        /// <summary>
        /// 이동 입력으로 움직이고 있는지 여부. 회피 중에는 false다. 탄퍼짐 계산 등에서 읽는다.
        /// </summary>
        public bool IsMoving => !IsDodging && _moveInput.sqrMagnitude > MoveInputDeadZone;

        /// <summary>
        /// 이동 입력 방향 (월드 기준, 길이 0~1). 애니메이션의 앞뒤 방향 계산 등에서 읽는다.
        /// </summary>
        public Vector3 MoveInputDirection => new Vector3(_moveInput.x, 0f, _moveInput.y);

        /// <summary>
        /// 현재 정밀 조준 중인지 여부
        /// </summary>
        private bool IsAimingPrecise => _aimer != null && _aimer.IsAimingPrecise;

        /// <summary>
        /// 현재 회피 중인지 여부
        /// </summary>
        private bool IsDodging => _dodger != null && _dodger.IsDodging;

        /// <summary>
        /// 필요한 컴포넌트를 캐싱하고 필수 참조를 확인한다.
        /// </summary>
        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            FindMissingReferences();
            WarnMissingReferences();
        }

        /// <summary>
        /// 이동 입력을 설정한다. 대각선 입력이 1보다 커지지 않도록 길이를 제한한다.
        /// </summary>
        /// <param name="moveInput">x는 좌우, y는 앞뒤 입력 (-1 ~ 1)</param>
        public void SetMoveInput(Vector2 moveInput)
        {
            _moveInput = Vector2.ClampMagnitude(moveInput, 1f);
        }

        /// <summary>
        /// 달리기 요청 여부를 설정한다. 달리기 키를 떼면 스태미나 고갈로 걸린 달리기 잠금도 풀린다.
        /// </summary>
        /// <param name="isRequested">달리기 키를 누르고 있으면 true</param>
        public void SetSprintRequested(bool isRequested)
        {
            _isSprintRequested = isRequested;

            if (!isRequested)
            {
                _isSprintBlockedUntilRelease = false;
            }
        }

        /// <summary>
        /// 외부 행동 때문에 달리기를 잠시 막을지 설정한다. 사격 키를 누르는 동안 막는 데 사용한다.
        /// 막는 동안에도 달리기 키를 계속 누르고 있으면, 해제된 뒤 다시 달린다.
        /// </summary>
        /// <param name="isSuppressed">달리기를 막으려면 true</param>
        public void SetSprintSuppressed(bool isSuppressed)
        {
            _isSprintSuppressed = isSuppressed;
        }

        /// <summary>
        /// 매 프레임 달리기 상태, 수평 속도, 수직 속도를 계산해 캐릭터를 이동시킨다.
        /// </summary>
        private void Update()
        {
            IsSprinting = UpdateSprintState();

            Vector3 velocity = CalculateHorizontalVelocity();
            UpdateVerticalVelocity();
            velocity.y = _verticalVelocity;

            _characterController.Move(velocity * Time.deltaTime);
        }

        /// <summary>
        /// 달리기 조건을 확인하고, 달리는 중이면 스태미나를 소모한다.
        /// 정밀 조준 중, 회피 중, 외부에서 달리기를 막은 동안에는 달릴 수 없다.
        /// </summary>
        /// <returns>이번 프레임에 달리는 상태이면 true</returns>
        private bool UpdateSprintState()
        {
            bool hasMoveInput = _moveInput.sqrMagnitude > MoveInputDeadZone;
            bool canSprint = _isSprintRequested
                && hasMoveInput
                && !_isSprintBlockedUntilRelease
                && !_isSprintSuppressed
                && !IsAimingPrecise
                && !IsDodging
                && _stamina != null
                && _stamina.HasStamina;

            if (!canSprint)
            {
                return false;
            }

            _stamina.Drain(_sprintStaminaCostPerSecond * Time.deltaTime);

            // 스태미나가 바닥나면 달리기 키를 뗐다가 다시 누를 때까지 달리기를 막는다.
            // 막지 않으면 회복과 소모가 반복되면서 달리기가 켜졌다 꺼졌다 한다.
            if (!_stamina.HasStamina)
            {
                _isSprintBlockedUntilRelease = true;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 이동 입력과 현재 상태로 수평 이동 속도를 계산한다. 회피 중에는 0을 반환한다.
        /// </summary>
        /// <returns>월드 기준 수평 이동 속도 (m/s)</returns>
        private Vector3 CalculateHorizontalVelocity()
        {
            // 회피 중의 수평 이동은 PlayerDodger가 처리한다
            if (IsDodging)
            {
                return Vector3.zero;
            }

            float speed = IsSprinting ? _sprintSpeed : _walkSpeed;

            if (IsAimingPrecise)
            {
                speed *= _preciseAimSpeedMultiplier;
            }

            // 카메라가 정면 시점(좌우 각도 0도)으로 고정되어 있어 입력의 y가 월드 앞쪽(+Z)과 같다
            Vector3 direction = new Vector3(_moveInput.x, 0f, _moveInput.y);
            return direction * speed;
        }

        /// <summary>
        /// 바닥에 있으면 바닥에 붙여 두고, 공중에 있으면 중력을 누적한다.
        /// </summary>
        private void UpdateVerticalVelocity()
        {
            if (_characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = _groundedVerticalVelocity;
                return;
            }

            _verticalVelocity += _gravity * Time.deltaTime;
        }

        /// <summary>
        /// 인스펙터에서 비어 있는 참조를 같은 오브젝트에서 찾는다.
        /// </summary>
        private void FindMissingReferences()
        {
            if (_stamina == null)
            {
                _stamina = GetComponent<PlayerStamina>();
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
            if (_stamina == null)
            {
                Debug.LogWarning($"[{nameof(PlayerMover)}] PlayerStamina를 찾지 못했습니다. 스태미나가 없으면 달리기를 할 수 없습니다.", this);
            }

            if (_aimer == null)
            {
                Debug.LogWarning($"[{nameof(PlayerMover)}] PlayerAimer를 찾지 못했습니다. 정밀 조준 감속이 동작하지 않습니다.", this);
            }

            if (_dodger == null)
            {
                Debug.LogWarning($"[{nameof(PlayerMover)}] PlayerDodger를 찾지 못했습니다. 회피 중 이동 제한이 동작하지 않습니다.", this);
            }
        }
    }
}
