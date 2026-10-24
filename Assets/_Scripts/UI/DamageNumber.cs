// 파일 경로: Assets/_Scripts/UI/DamageNumber.cs
using System;
using TMPro;
using UnityEngine;

namespace ProjectD.UI
{
    /// <summary>
    /// 맞은 위치에 떠오르며 사라지는 데미지 숫자 하나. 항상 카메라를 향하게 돌린다.
    /// DamageNumberSpawner의 풀에서 꺼내 쓰고, 다 사라지면 풀로 돌아간다.
    /// </summary>
    [RequireComponent(typeof(TextMeshPro))]
    public class DamageNumber : MonoBehaviour
    {
        [Tooltip("표시되는 시간 (초)")]
        [Min(0.01f)]
        [SerializeField] private float _lifetime = 0.8f;

        [Tooltip("위로 떠오르는 속도 (m/s)")]
        [SerializeField] private float _riseSpeed = 1.5f;

        private TextMeshPro _text;
        private Transform _cameraTransform;
        private Action<DamageNumber> _onFinished;
        private float _elapsedTime;

        /// <summary>
        /// 텍스트 컴포넌트를 캐싱한다.
        /// </summary>
        private void Awake()
        {
            _text = GetComponent<TextMeshPro>();
        }

        /// <summary>
        /// 데미지 숫자 표시를 시작한다.
        /// </summary>
        /// <param name="amount">표시할 데미지</param>
        /// <param name="position">표시를 시작할 위치 (월드 좌표)</param>
        /// <param name="cameraTransform">숫자가 바라볼 카메라. null이면 회전하지 않는다</param>
        /// <param name="onFinished">표시가 끝났을 때 호출할 함수. 보통 풀로 돌려보내는 함수</param>
        public void Play(float amount, Vector3 position, Transform cameraTransform, Action<DamageNumber> onFinished)
        {
            transform.position = position;
            _cameraTransform = cameraTransform;
            _onFinished = onFinished;
            _elapsedTime = 0f;

            _text.text = Mathf.RoundToInt(amount).ToString();
            _text.alpha = 1f;
        }

        /// <summary>
        /// 매 프레임 위로 이동하고, 카메라를 바라보고, 점점 투명해진다. 시간이 다 되면 종료한다.
        /// </summary>
        private void LateUpdate()
        {
            _elapsedTime += Time.deltaTime;
            transform.position += Vector3.up * (_riseSpeed * Time.deltaTime);

            if (_cameraTransform != null)
            {
                transform.rotation = _cameraTransform.rotation;
            }

            _text.alpha = 1f - Mathf.Clamp01(_elapsedTime / _lifetime);

            if (_elapsedTime >= _lifetime)
            {
                Finish();
            }
        }

        /// <summary>
        /// 종료 함수를 한 번만 호출한다.
        /// </summary>
        private void Finish()
        {
            Action<DamageNumber> onFinished = _onFinished;
            _onFinished = null;
            onFinished?.Invoke(this);
        }
    }
}
