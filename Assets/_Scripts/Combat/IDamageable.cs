// 파일 경로: Assets/_Scripts/Combat/IDamageable.cs
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 데미지를 받을 수 있는 대상. 투사체는 맞은 대상이 무엇인지 몰라도 이 인터페이스로 데미지를 전달한다.
    /// 더미 적, 3주차 적, 플레이어가 각각 구현한다.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// 데미지를 받는다.
        /// </summary>
        /// <param name="amount">데미지 양</param>
        /// <param name="hitPoint">맞은 위치 (월드 좌표)</param>
        void TakeDamage(float amount, Vector3 hitPoint);
    }
}
