// 파일 경로: Assets/_Scripts/AI/INoiseListener.cs
using UnityEngine;

namespace ProjectD.AI
{
    /// <summary>
    /// 소음을 들을 수 있는 대상. NoiseSystem이 소음 반경 안에서 찾아 호출한다.
    /// 소음을 듣는 쪽(적)과 내는 쪽(플레이어)이 서로를 모르게 하기 위한 인터페이스다.
    /// 콜라이더와 같은 오브젝트에 붙어 있어야 NoiseSystem이 찾을 수 있다.
    /// </summary>
    public interface INoiseListener
    {
        /// <summary>
        /// 소음을 들었을 때 호출된다.
        /// </summary>
        /// <param name="noisePosition">소음이 난 위치</param>
        void OnNoiseHeard(Vector3 noisePosition);
    }
}
