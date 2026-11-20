// 파일 경로: Assets/_Scripts/Core/InteractionHighlight.cs
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.Core
{
    /// <summary>
    /// 상호작용 대상으로 골라졌을 때 눈에 띄게 표시한다.
    /// 표시용 오브젝트(화살표, 빛 등)를 연결하면 그것을 켜고 끄고, 없으면 렌더러 색을 바꾼다.
    /// 에셋마다 셰이더의 색 속성 이름이 다를 수 있어, 색이 안 바뀌면 표시용 오브젝트를 연결한다.
    /// 상자, 적 시체, 땅의 아이템처럼 IInteractable이 붙은 오브젝트에 함께 붙인다.
    /// </summary>
    public class InteractionHighlight : MonoBehaviour
    {
        [Tooltip("골라졌을 때 켤 오브젝트. 연결하면 색 바꾸기 대신 이것을 쓴다")]
        [SerializeField] private GameObject _highlightObject;

        [Tooltip("색을 바꿀 렌더러. 비워 두면 자신과 자식의 렌더러를 모두 쓴다")]
        [SerializeField] private Renderer[] _renderers;

        [Tooltip("골라졌을 때 색")]
        [SerializeField] private Color _highlightColor = new Color(1f, 0.85f, 0.3f);

        private MaterialPropertyBlock _propertyBlock;

        /// <summary>
        /// 표시용 오브젝트를 끄고, 렌더러 목록이 비어 있으면 모은다.
        /// </summary>
        private void Awake()
        {
            if (_highlightObject != null)
            {
                _highlightObject.SetActive(false);
                return;
            }

            if (_renderers == null || _renderers.Length == 0)
            {
                _renderers = GetComponentsInChildren<Renderer>();
            }

            _propertyBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        /// 강조 표시를 켜거나 끈다.
        /// </summary>
        /// <param name="isHighlighted">켜려면 true</param>
        public void SetHighlighted(bool isHighlighted)
        {
            if (_highlightObject != null)
            {
                _highlightObject.SetActive(isHighlighted);
                return;
            }

            foreach (Renderer targetRenderer in _renderers)
            {
                if (targetRenderer == null)
                {
                    continue;
                }

                if (isHighlighted)
                {
                    _propertyBlock.SetColor(ShaderPropertyIds.BaseColor, _highlightColor);
                    targetRenderer.SetPropertyBlock(_propertyBlock);
                }
                else
                {
                    targetRenderer.SetPropertyBlock(null);
                }
            }
        }
    }
}
