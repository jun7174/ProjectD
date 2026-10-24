// 파일 경로: Assets/_Scripts/Combat/ShotCameraShake.cs
using ProjectD.Data;
using Unity.Cinemachine;
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 발사할 때 Cinemachine Impulse로 화면을 짧게 흔든다. 흔들림 세기는 무기 데이터에서 가져온다.
    /// 화면 흔들림은 내 화면에만 필요한 연출이라 로컬 플레이어 전용이다.
    /// 카메라 쪽에는 Cinemachine Impulse Listener가 있어야 흔들린다.
    /// </summary>
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class ShotCameraShake : MonoBehaviour
    {
        [Tooltip("발사 이벤트를 받을 대상. 비워 두면 같은 오브젝트에서 찾는다")]
        [SerializeField] private PlayerShooter _shooter;

        private CinemachineImpulseSource _impulseSource;

        /// <summary>
        /// Impulse Source를 캐싱하고 참조를 확인한다.
        /// </summary>
        private void Awake()
        {
            _impulseSource = GetComponent<CinemachineImpulseSource>();

            if (_shooter == null)
            {
                _shooter = GetComponent<PlayerShooter>();
            }

            if (_shooter == null)
            {
                Debug.LogWarning($"[{nameof(ShotCameraShake)}] PlayerShooter를 찾지 못했습니다. 발사 화면 흔들림이 동작하지 않습니다.", this);
            }
        }

        /// <summary>
        /// 발사 이벤트를 구독한다.
        /// </summary>
        private void OnEnable()
        {
            if (_shooter != null)
            {
                _shooter.OnFired += HandleFired;
            }
        }

        /// <summary>
        /// 발사 이벤트 구독을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            if (_shooter != null)
            {
                _shooter.OnFired -= HandleFired;
            }
        }

        /// <summary>
        /// 발사한 무기의 흔들림 세기로 Impulse를 발생시킨다.
        /// </summary>
        /// <param name="weapon">발사한 무기</param>
        private void HandleFired(WeaponData weapon)
        {
            if (weapon == null || weapon.ShakeForce <= 0f)
            {
                return;
            }

            _impulseSource.GenerateImpulseWithForce(weapon.ShakeForce);
        }
    }
}
