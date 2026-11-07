// 파일 경로: Assets/_Scripts/AI/IEnemyState.cs
using ProjectD.Core.Define;

namespace ProjectD.AI
{
    /// <summary>
    /// 적 상태 머신의 상태 하나 (상태 클래스 패턴).
    /// 상태마다 클래스를 나눠, 상태별 코드가 한곳에 모이고 다른 상태와 섞이지 않게 한다.
    /// 상태를 바꾸는 판단은 상태 안에서 하고, 실제 전환은 EnemyBrain의 GoTo 함수로 요청한다.
    /// </summary>
    public interface IEnemyState
    {
        /// <summary>
        /// 이 상태의 번호. 네트워크 동기화와 디버그 표시에 쓴다
        /// </summary>
        EnemyState Id { get; }

        /// <summary>
        /// 이 상태로 들어올 때 한 번 호출된다.
        /// </summary>
        void Enter();

        /// <summary>
        /// 이 상태인 동안 매 프레임 호출된다.
        /// </summary>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        void Tick(float deltaTime);

        /// <summary>
        /// 이 상태에서 나갈 때 한 번 호출된다.
        /// </summary>
        void Exit();
    }
}
