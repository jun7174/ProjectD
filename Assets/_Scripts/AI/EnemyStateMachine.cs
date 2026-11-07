// 파일 경로: Assets/_Scripts/AI/EnemyStateMachine.cs
using System;

namespace ProjectD.AI
{
    /// <summary>
    /// 현재 상태 하나를 들고, 상태 전환(Exit → Enter)과 매 프레임 실행만 담당한다.
    /// 어떤 상태가 있는지, 언제 바꾸는지는 모른다. MonoBehaviour가 아닌 일반 클래스라 EnemyBrain이 만들어 쓴다.
    /// </summary>
    public class EnemyStateMachine
    {
        /// <summary>
        /// 현재 상태. 처음 상태를 정하기 전에는 null
        /// </summary>
        public IEnemyState CurrentState { get; private set; }

        /// <summary>
        /// 상태가 바뀔 때 발생한다. 인자는 새 상태다.
        /// </summary>
        public event Action<IEnemyState> OnStateChanged;

        /// <summary>
        /// 상태를 바꾼다. 지금 상태의 Exit, 새 상태의 Enter 순서로 호출한다.
        /// 같은 상태로 바꾸면 그 상태를 처음부터 다시 시작한다 (예: 조사 중에 새 소리를 들었을 때).
        /// </summary>
        /// <param name="nextState">새 상태</param>
        public void ChangeState(IEnemyState nextState)
        {
            if (nextState == null)
            {
                return;
            }

            CurrentState?.Exit();
            CurrentState = nextState;
            CurrentState.Enter();

            OnStateChanged?.Invoke(CurrentState);
        }

        /// <summary>
        /// 현재 상태를 한 프레임 실행한다.
        /// </summary>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        public void Tick(float deltaTime)
        {
            CurrentState?.Tick(deltaTime);
        }
    }
}
