// 파일 경로: Assets/_Scripts/Combat/WeaponInputReader.cs
using ProjectD.Player;
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 로컬 플레이어의 사격, 재장전, 무기 교체 입력을 읽어 전투 담당 컴포넌트에 전달한다.
    /// Player 모듈이 Combat 모듈을 모르게 하려고, 전투 입력은 PlayerInputReader와 분리해서 이 클래스가 읽는다.
    /// 사격 키를 누르는 동안에는 달리기를 막는다.
    /// 원격 플레이어에서는 이 컴포넌트를 비활성화한다.
    /// </summary>
    // 같은 프레임의 입력이 동작 처리보다 먼저 전달되도록 기본 스크립트보다 먼저 실행한다
    [DefaultExecutionOrder(-10)]
    public class WeaponInputReader : MonoBehaviour
    {
        [Header("입력을 전달할 대상 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private PlayerShooter _shooter;
        [SerializeField] private PlayerReloader _reloader;
        [SerializeField] private PlayerWeaponHolder _weaponHolder;
        [SerializeField] private PlayerMover _mover;

        private PlayerControls _controls;

        /// <summary>
        /// 입력 액션 클래스를 생성하고 참조를 확인한다.
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
        /// 컴포넌트가 꺼지면 입력을 멈추고, 방아쇠와 달리기 제한을 풀어 둔다.
        /// </summary>
        private void OnDisable()
        {
            _controls.Player.Disable();

            if (_shooter != null)
            {
                _shooter.SetTriggerHeld(false);
            }

            if (_mover != null)
            {
                _mover.SetSprintSuppressed(false);
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
        /// 매 프레임 사격, 재장전, 무기 교체 입력을 읽어 전달한다.
        /// </summary>
        private void Update()
        {
            SendFireInput();
            SendReloadInput();
            SendWeaponSwitchInput();
        }

        /// <summary>
        /// 방아쇠 상태를 PlayerShooter에 전달하고, 사격 키를 누르는 동안 달리기를 막는다.
        /// </summary>
        private void SendFireInput()
        {
            bool isFireHeld = _controls.Player.Fire.IsPressed();

            if (_shooter != null)
            {
                _shooter.SetTriggerHeld(isFireHeld);
            }

            if (_mover != null)
            {
                _mover.SetSprintSuppressed(isFireHeld);
            }
        }

        /// <summary>
        /// 이번 프레임에 재장전 키를 눌렀으면 재장전을 요청한다.
        /// </summary>
        private void SendReloadInput()
        {
            if (_reloader != null && _controls.Player.Reload.WasPressedThisFrame())
            {
                _reloader.TryStartReload();
            }
        }

        /// <summary>
        /// 무기 교체 키 입력을 전달한다.테스트용으로 1번은 다음 무기, 2번은 이전 무기로 바꾼다.
        /// </summary>
        private void SendWeaponSwitchInput()
        {
            if (_weaponHolder == null)
            {
                return;
            }

            if (_controls.Player.Weapon1.WasPressedThisFrame())
            {
                _weaponHolder.SelectNextWeapon();
            }

            if (_controls.Player.Weapon2.WasPressedThisFrame())
            {
                _weaponHolder.SelectPreviousWeapon();
            }
        }

        /// <summary>
        /// 인스펙터에서 비어 있는 참조를 같은 오브젝트에서 찾는다.
        /// </summary>
        private void FindMissingReferences()
        {
            if (_shooter == null)
            {
                _shooter = GetComponent<PlayerShooter>();
            }

            if (_reloader == null)
            {
                _reloader = GetComponent<PlayerReloader>();
            }

            if (_weaponHolder == null)
            {
                _weaponHolder = GetComponent<PlayerWeaponHolder>();
            }

            if (_mover == null)
            {
                _mover = GetComponent<PlayerMover>();
            }
        }

        /// <summary>
        /// 찾지 못한 참조가 있으면 어떤 입력이 동작하지 않는지 경고를 출력한다.
        /// </summary>
        private void WarnMissingReferences()
        {
            if (_shooter == null)
            {
                Debug.LogWarning($"[{nameof(WeaponInputReader)}] PlayerShooter를 찾지 못했습니다. 사격 입력이 전달되지 않습니다.", this);
            }

            if (_reloader == null)
            {
                Debug.LogWarning($"[{nameof(WeaponInputReader)}] PlayerReloader를 찾지 못했습니다. 재장전 입력이 전달되지 않습니다.", this);
            }

            if (_weaponHolder == null)
            {
                Debug.LogWarning($"[{nameof(WeaponInputReader)}] PlayerWeaponHolder를 찾지 못했습니다. 무기 교체 입력이 전달되지 않습니다.", this);
            }

            if (_mover == null)
            {
                Debug.LogWarning($"[{nameof(WeaponInputReader)}] PlayerMover를 찾지 못했습니다. 사격 중 달리기 제한이 동작하지 않습니다.", this);
            }
        }
    }
}
