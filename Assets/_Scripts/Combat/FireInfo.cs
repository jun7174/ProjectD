// 파일 경로: Assets/_Scripts/Combat/FireInfo.cs
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 투사체 한 발을 만드는 데 필요한 발사 정보.
    /// 사격은 이 정보만으로 투사체를 만들기 때문에,이 값을 그대로 RPC로 보내다른 클라이언트에서도 같은 투사체를 재생한다.
    /// </summary>
    public readonly struct FireInfo
    {
        /// <summary>
        /// 발사 시작 위치 (월드 좌표)
        /// </summary>
        public Vector3 Origin { get; }

        /// <summary>
        /// 발사 방향 (길이 1, 탄퍼짐 적용 후)
        /// </summary>
        public Vector3 Direction { get; }

        /// <summary>
        /// 발사한 무기 데이터
        /// </summary>
        public WeaponData Weapon { get; }

        /// <summary>
        /// 발사 정보를 만든다.
        /// </summary>
        /// <param name="origin">발사 시작 위치</param>
        /// <param name="direction">발사 방향. 길이 1로 정규화해서 저장한다</param>
        /// <param name="weapon">발사한 무기 데이터</param>
        public FireInfo(Vector3 origin, Vector3 direction, WeaponData weapon)
        {
            Origin = origin;
            Direction = direction.normalized;
            Weapon = weapon;
        }
    }
}
