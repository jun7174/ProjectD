// 파일 경로: Assets/_Scripts/Player/PlayerInputReader.cs
using UnityEngine;

namespace ProjectD.Player
{
    /// <summary>
    /// 로컬 플레이어의 키보드와 마우스 입력을 읽어 동작 담당 컴포넌트에 전달한다.
    /// 마우스 화면 좌표를 월드 조준점으로 바꾸는 일도 로컬 플레이어에서만 필요하므로 여기서 처리한다.
    /// 입력 읽기만 담당하며 실제 동작은 PlayerMover, PlayerAimer, PlayerDodger가 처리한다.
    /// 원격 플레이어에서는 이 컴포넌트를 비활성화하고, 네트워크 담당이 대신 값을 전달한다.
    /// </summary>
    // 같은 프레임의 입력이 동작 처리보다 먼저 전달되도록 기본 스크립트보다 먼저 실행한다
    [DefaultExecutionOrder(-10)]
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("입력을 전달할 대상 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private PlayerMover _mover;
        [SerializeField] private PlayerAimer _aimer;
        [SerializeField] private PlayerDodger _dodger;

        [Header("조준")]
        [Tooltip("마우스 위치를 월드로 투영할 카메라. 비워 두면 Main Camera를 사용한다")]
        [SerializeField] private Camera _camera;

        [Tooltip("마우스 위치를 투영할 수평면의 높이. 캐릭터 기준점에서 위쪽으로 떨어진 거리 (m)")]
        [SerializeField] private float _aimPlaneHeightOffset = 0f;

        private PlayerControls _controls;

        /// <summary>
        /// 입력 액션 클래스를 생성하고 필수 참조를 확인한다.
        /// </summary>
        private void Awake()
        {
            _controls = new PlayerControls();
            FindMissingReferences();
            WarnMissingReferences();
        }

        /// <summary>
        /// 컴포넌트가 켜지면 Player 액션 맵의 입력을 받기 시작한다.
        /// </summary>
        private void OnEnable()
        {
            _controls.Player.Enable();
        }

        /// <summary>
        /// 컴포넌트가 꺼지면 입력을 멈추고, 마지막 입력값이 남아 동작이 계속되지 않도록 정지 상태를 전달한다.
        /// </summary>
        private void OnDisable()
        {
            _controls.Player.Disable();

            if (_mover != null)
            {
                _mover.SetMoveInput(Vector2.zero);
                _mover.SetSprintRequested(false);
            }

            if (_aimer != null)
            {
                _aimer.SetPreciseAimRequested(false);
            }
        }

        /// <summary>
        /// 입력 액션 클래스가 사용하던 자원을 해제한다.
        /// </summary>
        private void OnDestroy()
        {
            _controls.Dispose();
        }

        /// <summary>
        /// 매 프레임 이동, 조준, 회피 입력을 읽어 각 동작 담당에 전달한다.
        /// </summary>
        private void Update()
        {
            Vector2 moveInput = _controls.Player.Move.ReadValue<Vector2>();

            SendMoveInput(moveInput);
            SendAimInput();
            SendDodgeInput(moveInput);
        }

        /// <summary>
        /// 이동 입력과 달리기 요청을 PlayerMover에 전달한다.
        /// </summary>
        /// <param name="moveInput">이번 프레임의 이동 입력</param>
        private void SendMoveInput(Vector2 moveInput)
        {
            if (_mover == null)
            {
                return;
            }

            _mover.SetMoveInput(moveInput);
            _mover.SetSprintRequested(_controls.Player.Sprint.IsPressed());
        }

        /// <summary>
        /// 정밀 조준 요청과 마우스 조준점을 PlayerAimer에 전달한다.
        /// </summary>
        private void SendAimInput()
        {
            if (_aimer == null)
            {
                return;
            }

            _aimer.SetPreciseAimRequested(_controls.Player.AimPrecise.IsPressed());

            if (TryGetMouseAimPoint(out Vector3 aimPoint))
            {
                _aimer.SetAimPoint(aimPoint);
            }
        }

        /// <summary>
        /// 이번 프레임에 회피 키를 눌렀으면 PlayerDodger에 회피를 요청한다.
        /// </summary>
        /// <param name="moveInput">회피 방향을 정하기 위한 이번 프레임의 이동 입력</param>
        private void SendDodgeInput(Vector2 moveInput)
        {
            if (_dodger == null)
            {
                return;
            }

            if (_controls.Player.Dodge.WasPressedThisFrame())
            {
                _dodger.TryStartDodge(moveInput);
            }
        }

        /// <summary>
        /// 마우스 화면 좌표를 캐릭터 높이의 수평면에 투영해서 월드 조준점을 구한다.
        /// </summary>
        /// <param name="aimPoint">구한 월드 조준점</param>
        /// <returns>조준점을 구했으면 true</returns>
        private bool TryGetMouseAimPoint(out Vector3 aimPoint)
        {
            aimPoint = default;

            if (_camera == null)
            {
                return false;
            }

            Vector2 screenPosition = _controls.Player.AimPosition.ReadValue<Vector2>();
            Ray ray = _camera.ScreenPointToRay(screenPosition);
            Plane aimPlane = new Plane(Vector3.up, transform.position + Vector3.up * _aimPlaneHeightOffset);

            // 카메라가 위에서 내려다보므로 정상적인 경우 항상 교차한다
            if (!aimPlane.Raycast(ray, out float distance))
            {
                return false;
            }

            aimPoint = ray.GetPoint(distance);
            return true;
        }

        /// <summary>
        /// 인스펙터에서 비어 있는 참조를 같은 오브젝트와 Main Camera에서 찾는다.
        /// </summary>
        private void FindMissingReferences()
        {
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

            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        /// <summary>
        /// 찾지 못한 참조가 있으면 어떤 입력이 동작하지 않는지 경고를 출력한다.
        /// </summary>
        private void WarnMissingReferences()
        {
            if (_mover == null)
            {
                Debug.LogWarning($"[{nameof(PlayerInputReader)}] PlayerMover를 찾지 못했습니다. 이동 입력이 전달되지 않습니다.", this);
            }

            if (_aimer == null)
            {
                Debug.LogWarning($"[{nameof(PlayerInputReader)}] PlayerAimer를 찾지 못했습니다. 조준 입력이 전달되지 않습니다.", this);
            }

            if (_dodger == null)
            {
                Debug.LogWarning($"[{nameof(PlayerInputReader)}] PlayerDodger를 찾지 못했습니다. 회피 입력이 전달되지 않습니다.", this);
            }

            if (_camera == null)
            {
                Debug.LogWarning($"[{nameof(PlayerInputReader)}] 카메라를 찾지 못했습니다. Main Camera 태그를 확인하거나 인스펙터에서 연결하세요.", this);
            }
        }
    }
}
