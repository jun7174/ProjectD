// 파일 경로: Assets/_Scripts/AI/EnemyStatus.cs
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.AI
{
    /// <summary>
    /// 다른 화면에도 보여 줘야 하는 적의 상태 값(상태 머신 상태, 경계 게이지, 공격 예고 여부)을 담는다.
    /// 마스터에서는 AI가 값을 쓰고, 다른 클라이언트에서는 네트워크 동기화(EnemySync)가 받은 값을 쓴다.
    /// 디버그 표시와 예고 동작 표시는 어느 쪽이든 이 값만 읽으면 된다.
    /// 적 프리팹 루트에 붙이고, 모든 클라이언트에서 켜 둔다.
    /// </summary>
    public class EnemyStatus : MonoBehaviour
    {
        /// <summary>
        /// 현재 상태 머신 상태
        /// </summary>
        public EnemyState CurrentState { get; private set; } = EnemyState.Patrol;

        /// <summary>
        /// 경계 게이지 (0~1). 1이면 추적 중
        /// </summary>
        public float AlertLevel { get; private set; }

        /// <summary>
        /// 공격 예고 동작 중인지 여부
        /// </summary>
        public bool IsTelegraphing { get; private set; }

        /// <summary>
        /// 현재 상태를 바꾼다.
        /// </summary>
        /// <param name="state">새 상태</param>
        public void SetState(EnemyState state)
        {
            CurrentState = state;
        }

        /// <summary>
        /// 경계 게이지를 바꾼다. 0~1 사이로 제한한다.
        /// </summary>
        /// <param name="alertLevel">새 경계 게이지 값</param>
        public void SetAlertLevel(float alertLevel)
        {
            AlertLevel = Mathf.Clamp01(alertLevel);
        }

        /// <summary>
        /// 공격 예고 동작 여부를 바꾼다.
        /// </summary>
        /// <param name="isTelegraphing">예고 중이면 true</param>
        public void SetTelegraphing(bool isTelegraphing)
        {
            IsTelegraphing = isTelegraphing;
        }
    }
}
