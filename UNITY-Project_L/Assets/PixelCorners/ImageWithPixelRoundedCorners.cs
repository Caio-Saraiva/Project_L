using UnityEngine;
using UnityEngine.UI;

namespace Nobi.UiRoundedCorners
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class ImageWithPixelRoundedCorners : MonoBehaviour
    {
        // IDs para os parâmetros do shader
        private static readonly int SHADER_PROP_ID = Shader.PropertyToID("_WidthHeightRadiusDensity");
        private static readonly int SHADER_PROP_OUTERUV = Shader.PropertyToID("_OuterUV");

        [Tooltip("Raio dos cantos em pixels")]
        public float radius = 40f;
        [Tooltip("Densidade dos 'pixels' nos cantos (quanto maior, menores os pixels)")]
        public float density = 10f;

        private Material material;
        private Vector4 outerUV = new Vector4(0, 0, 1, 1);

        [HideInInspector, SerializeField]
        private MaskableGraphic image;

        private void OnValidate()
        {
            Setup();
            UpdateMaterialProperties();
        }

        private void OnEnable()
        {
            Setup();
            UpdateMaterialProperties();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (enabled && material != null)
            {
                UpdateMaterialProperties();
            }
        }

        private void OnDestroy()
        {
            if (image != null)
            {
                image.material = null; // Restaura o material padrão
            }
            if (material != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(material);
#else
                Destroy(material);
#endif
            }
            material = null;
            image = null;
        }

        // Configura o material e obtém referências necessárias
        private void Setup()
        {
            if (material == null)
            {
                // Certifique-se de que o shader "UI/RoundedCorners/PixelRoundedCorners" foi criado (veja abaixo)
                material = new Material(Shader.Find("PixelRoundedCorners"));
            }
            if (image == null)
            {
                TryGetComponent(out image);
            }
            if (image != null)
            {
                image.material = material;
            }
            if (image is Image uiImage && uiImage.sprite != null)
            {
                outerUV = UnityEngine.Sprites.DataUtility.GetOuterUV(uiImage.sprite);
            }
        }

        // Atualiza os parâmetros enviados para o shader
        private void UpdateMaterialProperties()
        {
            Rect rect = ((RectTransform)transform).rect;
            // Parâmetros: (largura, altura, raio, densidade)
            material.SetVector(SHADER_PROP_ID, new Vector4(rect.width, rect.height, radius, density));
            material.SetVector(SHADER_PROP_OUTERUV, outerUV);
        }
    }
}
