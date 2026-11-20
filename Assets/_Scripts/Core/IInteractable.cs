// 파일 경로: Assets/_Scripts/Core/IInteractable.cs
using UnityEngine;

namespace ProjectD.Core
{
    /// <summary>
    /// F키로 상호작용할 수 있는 대상(상자, 땅의 아이템, 이후 부활 등).
    /// 내 캐릭터의 PlayerInteractor가 주변에서 우선순위가 가장 높고 가장 가까운 대상 하나를 골라 실행한다.
    /// 무엇을 할지는 각 대상이 정하므로, 새 상호작용을 추가해도 PlayerInteractor는 바뀌지 않는다.
    /// Interactable 레이어의 콜라이더가 이 컴포넌트와 같은 오브젝트나 자식에 있어야 찾을 수 있다.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// 우선순위. 클수록 먼저 고른다 (부활 2, 상자 1, 아이템 0)
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// F를 누르고 있어야 하는 시간 (초). 0이면 누르는 즉시 실행한다
        /// </summary>
        float HoldDuration { get; }

        /// <summary>
        /// 거리 비교에 쓰는 위치
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// 화면에 띄울 안내 문구 (예: "열기: 상자")
        /// </summary>
        string Prompt { get; }

        /// <summary>
        /// 지금 상호작용할 수 있는지 여부
        /// </summary>
        bool CanInteract { get; }

        /// <summary>
        /// 상호작용을 실행한다.
        /// </summary>
        /// <param name="interactor">상호작용한 플레이어 캐릭터</param>
        void Interact(GameObject interactor);
    }
}
