// 파일 경로: Assets/_Scripts/Player/InteractionInputReader.cs
using UnityEngine;

namespace ProjectD.Player
{
    /// <summary>
    /// 상호작용 키(F) 입력을 읽어 PlayerInteractor에 전달한다 (입력 담당).
    /// 채팅 중에는 PlayerInputGate가 이 컴포넌트를 꺼서 F가 글자로만 쓰이게 한다.
    /// 플레이어 프리팹 루트에 붙이고 PlayerNetworkSetup의 Local Only 목록에 넣는다.
    /// </summary>
    [DefaultExecutionOrder(-10)]
    public class InteractionInputReader : MonoBehaviour
    {
        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private PlayerInteractor _interactor;

        private PlayerControls _controls;

        /// <summary>
        /// 입력 클래스를 만들고 참조를 찾는다.
        /// </summary>
        private void Awake()
        {
            _controls = new PlayerControls();

            if (_interactor == null && !TryGetComponent(out _interactor))
            {
                Debug.LogWarning($"[{nameof(InteractionInputReader)}] PlayerInteractor를 찾지 못했습니다. 상호작용 입력이 전달되지 않습니다.", this);
            }
        }

        /// <summary>
        /// 입력을 켠다.
        /// </summary>
        private void OnEnable()
        {
            _controls.Player.Enable();
        }

        /// <summary>
        /// 입력을 끄고, 누르고 있던 상태를 풀어 준다.
        /// </summary>
        private void OnDisable()
        {
            _controls.Player.Disable();

            if (_interactor != null)
            {
                _interactor.SetInteractHeld(false);
            }
        }

        /// <summary>
        /// 입력 클래스를 정리한다.
        /// </summary>
        private void OnDestroy()
        {
            _controls.Dispose();
        }

        /// <summary>
        /// F를 누르고 있는지 매 프레임 전달한다.
        /// </summary>
        private void Update()
        {
            if (_interactor != null)
            {
                _interactor.SetInteractHeld(_controls.Player.Interact.IsPressed());
            }
        }
    }
}
