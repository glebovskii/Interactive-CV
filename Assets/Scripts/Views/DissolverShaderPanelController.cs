using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dissolver
{
    [RequireComponent(typeof(PanelRenderer))]
    public sealed class DissolverShaderPanelController : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;

        private readonly UICallbackBinder uiCallbacks = new();

        private PlayerDissolveController dissolveController;
        private VisualElement currentRoot;

        private void Awake()
        {
            if (panelRenderer == null)
                panelRenderer = GetComponent<PanelRenderer>();

            if (panelRenderer == null)
            {
                Debug.LogError($"{nameof(DissolverShaderPanelController)} requires a PanelRenderer.", this);
                return;
            }

            panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }

        public void SetPlayer(PlayerDissolveController controller)
        {
            dissolveController = controller;
            UnbindUI();

            if (currentRoot != null)
                BindUI(currentRoot);
        }

        private void OnDestroy()
        {
            UnbindUI();

            if (panelRenderer != null)
                panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
        {
            UnbindUI();
            currentRoot = root;
            BindUI(root);
        }

        private void BindUI(VisualElement root)
        {
            if (dissolveController == null || !dissolveController.HasStateAuthority)
                return;

            BindEdge(root, "slider-edge-width");
            BindDissolve(root, "slider-dissolve");
            BindAxis(root, "dropdown-dissolve-direction", PlayerDissolveController.AxisChoices, dissolveController.Axis);
            BindDirection(root, "dropdown-dissolve-start", PlayerDissolveController.StartDirectionChoices, dissolveController.Direction);
        }

        private void BindDissolve(VisualElement root, string controlName)
        {
            Slider slider = root.Q<Slider>(controlName);

            if (slider == null)
                return;

            slider.SetValueWithoutNotify(dissolveController.Dissolve);
            uiCallbacks.BindChange<float>(slider, value => dissolveController.Dissolve = value, sound => sound.PlaySliderChange());
        }

        private void BindEdge(VisualElement root, string controlName)
        {
            Slider slider = root.Q<Slider>(controlName);

            if (slider == null)
                return;

            slider.SetValueWithoutNotify(dissolveController.EdgeWidth);
            uiCallbacks.BindChange<float>(slider, value => dissolveController.EdgeWidth = value, sound => sound.PlaySliderChange());
        }

        private void BindAxis(VisualElement root, string controlName, List<string> choices, int initialIndex)
        {
            DropdownField dropdown = root.Q<DropdownField>(controlName);

            if (dropdown == null)
                return;

            dropdown.choices = new List<string>(choices);
            initialIndex = Mathf.Clamp(initialIndex, 0, dropdown.choices.Count - 1);
            dropdown.SetValueWithoutNotify(dropdown.choices[initialIndex]);

            uiCallbacks.BindChange<string>(dropdown, value =>
            {
                int selectedIndex = dropdown.choices.IndexOf(value);

                if (selectedIndex >= 0)
                    dissolveController.Axis = selectedIndex;
            }, sound => sound.PlayToggle());
        }

        private void BindDirection(VisualElement root, string controlName, List<string> choices, int initialIndex)
        {
            DropdownField dropdown = root.Q<DropdownField>(controlName);

            if (dropdown == null)
                return;

            dropdown.choices = new List<string>(choices);
            initialIndex = Mathf.Clamp(initialIndex, 0, dropdown.choices.Count - 1);
            dropdown.SetValueWithoutNotify(dropdown.choices[initialIndex]);

            uiCallbacks.BindChange<string>(dropdown, value =>
            {
                int selectedIndex = dropdown.choices.IndexOf(value);

                if (selectedIndex >= 0)
                    dissolveController.Direction = selectedIndex;
            }, sound => sound.PlayToggle());
        }

        private void UnbindUI()
        {
            uiCallbacks.Clear();
        }

        public void Hide()
        {
            UnbindUI();
        }
    }
}