// 파일 경로: Assets/_Scripts/World/Occluder.cs
using ProjectD.Core.Define;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectD.World
{
    /// <summary>
    /// 내 캐릭터를 가릴 수 있는 물체(벽, 천장)에 붙이는 컴포넌트.
    /// OccluderProbe가 "가리고 있다"고 알려 주면 정해진 방식으로 안 보이게 하고, 더 이상 가리지 않으면 원래대로 되돌린다.
    /// 벽은 투명 머티리얼로 바꾸고, 천장은 그림자만 남긴다. 원래 상태는 이 컴포넌트가 저장했다가 복구한다.
    /// 자기와 자식의 렌더러를 모두 함께 바꾼다.
    /// </summary>
    [DisallowMultipleComponent]
    public class Occluder : MonoBehaviour
    {
        [Header("숨기는 방식")]
        [Tooltip("Transparent: 투명 머티리얼로 교체(벽). ShadowsOnly: 그림자만 남김(천장)")]
        [SerializeField] private OcclusionMode _mode = OcclusionMode.Transparent;

        [Tooltip("Transparent 방식에서 쓸 투명 머티리얼. 여러 벽이 하나를 같이 쓴다")]
        [SerializeField] private Material _transparentMaterial;

        private Renderer[] _renderers;

        // 렌더러마다 원래 머티리얼 배열과 그림자 설정을 저장해 두었다가 복구한다
        private Material[][] _originalMaterials;
        private ShadowCastingMode[] _originalShadowModes;

        // 렌더러마다 머티리얼 칸 수만큼 투명 머티리얼을 채운 배열. 숨길 때마다 새로 만들지 않도록 미리 만든다
        private Material[][] _transparentMaterialArrays;

        /// <summary>
        /// 지금 숨겨져 있는지 여부
        /// </summary>
        public bool IsOccluding { get; private set; }

        /// <summary>
        /// 자기와 자식의 렌더러를 모으고, 저장 공간과 투명 머티리얼 배열을 미리 준비한다.
        /// </summary>
        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _originalMaterials = new Material[_renderers.Length][];
            _originalShadowModes = new ShadowCastingMode[_renderers.Length];
            _transparentMaterialArrays = new Material[_renderers.Length][];

            for (int i = 0; i < _renderers.Length; i++)
            {
                _transparentMaterialArrays[i] = CreateFilledArray(_transparentMaterial, _renderers[i].sharedMaterials.Length);
            }

            WarnMissingSettings();
        }

        /// <summary>
        /// 꺼지거나 사라질 때 숨겨진 상태면 원래대로 되돌린다.
        /// </summary>
        private void OnDisable()
        {
            Reveal();
        }

        /// <summary>
        /// 정해진 방식으로 안 보이게 한다. 이미 숨겨져 있으면 아무것도 하지 않는다.
        /// </summary>
        public void Occlude()
        {
            if (IsOccluding)
            {
                return;
            }

            IsOccluding = true;

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_mode == OcclusionMode.Transparent)
                {
                    MakeTransparent(i);
                }
                else
                {
                    MakeShadowsOnly(i);
                }
            }
        }

        /// <summary>
        /// 저장해 둔 원래 상태로 되돌린다. 숨겨져 있지 않으면 아무것도 하지 않는다.
        /// </summary>
        public void Reveal()
        {
            if (!IsOccluding)
            {
                return;
            }

            IsOccluding = false;

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null)
                {
                    continue;
                }

                if (_mode == OcclusionMode.Transparent)
                {
                    // 투명 머티리얼이 없어 바꾸지 않았던 렌더러는 저장된 값이 없으므로 건너뛴다
                    if (_originalMaterials[i] != null)
                    {
                        _renderers[i].sharedMaterials = _originalMaterials[i];
                    }
                }
                else
                {
                    _renderers[i].shadowCastingMode = _originalShadowModes[i];
                }
            }
        }

        /// <summary>
        /// 렌더러 하나의 원래 머티리얼을 저장하고 투명 머티리얼로 바꾼다.
        /// materials가 아니라 sharedMaterials를 써서 머티리얼 복제본이 생기지 않게 한다.
        /// </summary>
        /// <param name="index">렌더러 번호</param>
        private void MakeTransparent(int index)
        {
            if (_transparentMaterial == null)
            {
                return;
            }

            _originalMaterials[index] = _renderers[index].sharedMaterials;
            _renderers[index].sharedMaterials = _transparentMaterialArrays[index];
        }

        /// <summary>
        /// 렌더러 하나의 원래 그림자 설정을 저장하고 그림자만 남긴다.
        /// 렌더러를 끄면 그림자까지 사라져 실내가 갑자기 밝아지므로, 보이지 않고 그림자만 드리우게 한다.
        /// </summary>
        /// <param name="index">렌더러 번호</param>
        private void MakeShadowsOnly(int index)
        {
            _originalShadowModes[index] = _renderers[index].shadowCastingMode;
            _renderers[index].shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }

        /// <summary>
        /// 같은 머티리얼로 채운 배열을 만든다.
        /// </summary>
        /// <param name="material">채울 머티리얼</param>
        /// <param name="length">배열 길이 (렌더러의 머티리얼 칸 수)</param>
        /// <returns>채운 배열</returns>
        private static Material[] CreateFilledArray(Material material, int length)
        {
            Material[] materials = new Material[length];

            for (int i = 0; i < length; i++)
            {
                materials[i] = material;
            }

            return materials;
        }

        /// <summary>
        /// 동작하지 않는 설정이면 경고를 출력한다.
        /// </summary>
        private void WarnMissingSettings()
        {
            if (_renderers.Length == 0)
            {
                Debug.LogWarning($"[{nameof(Occluder)}] 자기나 자식에 렌더러가 없습니다. 숨길 대상이 없습니다.", this);
            }

            if (_mode == OcclusionMode.Transparent && _transparentMaterial == null)
            {
                Debug.LogWarning($"[{nameof(Occluder)}] 투명 머티리얼이 비어 있습니다. 반투명이 동작하지 않습니다.", this);
            }
        }
    }
}
