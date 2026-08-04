using UnityEngine;
using UnityEngine.UIElements;

namespace Chroma
{
    [RequireComponent(typeof(PanelRenderer))]
    public sealed class SpectraShadowPanelController : MonoBehaviour
    {
        private static readonly int ShadowOffsetProperty = Shader.PropertyToID("_ShadowOffset");

        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private Material targetMaterial;

        private readonly UICallbackBinder uiCallbacks = new();

        private VisualElement currentRoot;
        private Vector4 shadowOffset;

        private void Awake()
        {
            if (panelRenderer == null)
                panelRenderer = GetComponent<PanelRenderer>();

            if (panelRenderer == null)
            {
                Debug.LogError($"{nameof(SpectraShadowPanelController)} requires a PanelRenderer.", this);
                return;
            }

            panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }

        private void OnDestroy()
        {
            UnbindUI();

            if (panelRenderer != null)
                panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        public void SetMaterial(Material material)
        {
            UnbindUI();
            targetMaterial = material;

            if (currentRoot != null)
                BindUI(currentRoot);
        }

        private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
        {
            UnbindUI();
            currentRoot = root;

            if (targetMaterial != null)
                BindUI(root);
        }

        private void BindUI(VisualElement root)
        {
            if (targetMaterial == null)
                return;

            bool hasOffset = targetMaterial.HasProperty(ShadowOffsetProperty);
            shadowOffset = hasOffset ? targetMaterial.GetVector(ShadowOffsetProperty) : Vector4.zero;

            BindOffsetSlider(root, "slider-shadow-offset-x", 0, hasOffset);
            BindOffsetSlider(root, "slider-shadow-offset-y", 1, hasOffset);
            BindOffsetSlider(root, "slider-shadow-offset-z", 2, hasOffset);
        }

        private void BindOffsetSlider(VisualElement root, string controlName, int componentIndex, bool propertyExists)
        {
            Slider slider = root.Q<Slider>(controlName);

            if (slider == null)
                return;

            slider.SetEnabled(propertyExists);

            if (!propertyExists)
            {
                slider.tooltip = "The assigned material does not contain _ShadowOffset.";
                return;
            }

            slider.SetValueWithoutNotify(GetVectorComponent(shadowOffset, componentIndex));

            uiCallbacks.BindChange<float>(slider, value =>
            {
                SetVectorComponent(ref shadowOffset, componentIndex, value);
                targetMaterial.SetVector(ShadowOffsetProperty, shadowOffset);
            }, sound => sound.PlaySliderChange());
        }

        private void UnbindUI()
        {
            uiCallbacks.Clear();
        }

        private static float GetVectorComponent(Vector4 value, int componentIndex)
        {
            return componentIndex switch
            {
                0 => value.x,
                1 => value.y,
                2 => value.z,
                _ => value.w
            };
        }

        private static void SetVectorComponent(ref Vector4 value, int componentIndex, float componentValue)
        {
            switch (componentIndex)
            {
                case 0:
                    value.x = componentValue;
                    break;
                case 1:
                    value.y = componentValue;
                    break;
                case 2:
                    value.z = componentValue;
                    break;
                default:
                    value.w = componentValue;
                    break;
            }
        }
    }
}