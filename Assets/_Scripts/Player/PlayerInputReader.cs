// 파일 경로: Assets/_Scripts/Player/PlayerInputReader.cs
using UnityEngine;

namespace ProjectD.Player
{
    /// <summary>
    /// 로컬 플레이어의 키보드와 마우스 입력을 읽어 동작 담당 컴포넌트에 전달한다.
    /// 입력 읽기만 담당하며 실제 이동은 PlayerMover가 처리한다.
    /// 2주차부터 원격 플레이어에서는 이 컴포넌트를 비활성화하고, 네트워크 담당이 대신 값을 전달한다.
    /// </summary>
    // 같은 프레임의 입력이 이동 처리보다 먼저 전달되도록 기본 스크립트보다 먼저 실행한다
    [DefaultExecutionOrder(-10)]
    public class PlayerInputReader : MonoBehaviour
    {
        [Tooltip("이동 입력을 전달할 대상. 비워 두면 같은 오브젝트에서 찾는다")]
        [SerializeField] private PlayerMover _mover;

        private PlayerControls _controls;

        /// <summary>
        /// 입력 액션 클래스를 생성하고 필수 참조를 확인한다.
        /// </summary>
        private void Awake()
        {
            _controls = new PlayerControls();

            if (_mover == null)
            {
                _mover = GetComponent<PlayerMover>();
            }

            if (_mover == null)
            {
                Debug.LogWarning($"[{nameof(PlayerInputReader)}] PlayerMover를 찾지 못했습니다. 같은 오브젝트에 PlayerMover를 추가하거나 인스펙터에서 연결하세요.", this);
            }
        }

        /// <summary>
        /// 컴포넌트가 켜지면 Player 액션 맵의 입력을 받기 시작한다.
        /// </summary>
        private void OnEnable()
        {
            _controls.Player.Enable();
        }

        /// <summary>
        /// 컴포넌트가 꺼지면 입력을 멈추고, 마지막 입력값이 남아 캐릭터가 계속 움직이지 않도록 정지 상태를 전달한다.
        /// </summary>
        private void OnDisable()
        {
            _controls.Player.Disable();

            if (_mover != null)
            {
                _mover.SetMoveInput(Vector2.zero);
                _mover.SetSprintRequested(false);
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
        /// 매 프레임 이동과 달리기 입력을 읽어 PlayerMover에 전달한다.
        /// </summary>
        private void Update()
        {
            if (_mover == null)
            {
                return;
            }

            _mover.SetMoveInput(_controls.Player.Move.ReadValue<Vector2>());
            _mover.SetSprintRequested(_controls.Player.Sprint.IsPressed());
        }
    }
}
