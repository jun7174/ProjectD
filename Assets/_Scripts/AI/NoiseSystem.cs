// 파일 경로: Assets/_Scripts/AI/NoiseSystem.cs
using UnityEngine;

namespace ProjectD.AI
{
    /// <summary>
    /// 소음을 반경 안의 듣는 대상에게 전달한다
    /// </summary>
    public static class NoiseSystem
    {
        // 한 번의 소음이 닿을 수 있는 최대 대상 수.
        private const int MaxListeners = 32;

        private static readonly Collider[] _overlapBuffer = new Collider[MaxListeners];

        /// <summary>
        /// 위치를 중심으로 반경 안에 있는 듣는 대상에게 소음을 알린다.
        /// </summary>
        /// <param name="position">소음이 난 위치</param>
        /// <param name="radius">소음 반경 (m)</param>
        /// <param name="listenerMask">듣는 대상의 레이어 (Enemy)</param>
        public static void Emit(Vector3 position, float radius, LayerMask listenerMask)
        {
            if (radius <= 0f)
            {
                return;
            }

            int count = Physics.OverlapSphereNonAlloc(position, radius, _overlapBuffer, listenerMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                if (_overlapBuffer[i].TryGetComponent(out INoiseListener listener))
                {
                    listener.OnNoiseHeard(position);
                }
            }
        }
    }
}
