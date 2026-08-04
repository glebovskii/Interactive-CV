using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dissolver
{
    [RequireComponent(typeof(PanelRenderer))]
    public sealed class DissolverShaderPanelController : MonoBehaviour
    {

        [SerializeField] private PanelRenderer panelRenderer;

        private readonly List<Action> unbindActions = new();

        private PlayerDissolveController dissolveController;

        private UISoundController soundController;

        private void Awake()
        {
            if (panelRenderer == null)
                panelRenderer = GetComponent<PanelRenderer>();

            ServiceLocator.TryGet(out soundController);
        }

        public void SetPlayer(PlayerDissolveController controller)
        {
            dissolveController = controller;

            UnbindUI();
            panelRenderer.RegisterUIReloadCallback(OnUIReload);
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

            EventCallback<ChangeEvent<float>> callback = evt =>
            {
                dissolveController.Dissolve = evt.newValue;
                soundController?.PlaySliderChange();
            };

            slider.RegisterValueChangedCallback(callback);

            unbindActions.Add(() => slider.UnregisterValueChangedCallback(callback));
        }

        private void BindEdge(VisualElement root, string controlName)
        {
            Slider slider = root.Q<Slider>(controlName);

            if (slider == null)
                return;

            slider.SetValueWithoutNotify(dissolveController.EdgeWidth);

            EventCallback<ChangeEvent<float>> callback = evt =>
            {
                dissolveController.EdgeWidth = evt.newValue;
                soundController?.PlaySliderChange();
            };

            slider.RegisterValueChangedCallback(callback);

            unbindActions.Add(() => slider.UnregisterValueChangedCallback(callback));
        }

        private void BindAxis(VisualElement root, string controlName, List<string> choices, int initialIndex)
        {
            DropdownField dropdown = root.Q<DropdownField>(controlName);

            if (dropdown == null)
                return;

            dropdown.choices = new List<string>(choices);

            initialIndex = Mathf.Clamp(initialIndex, 0, dropdown.choices.Count - 1);

            dropdown.SetValueWithoutNotify(dropdown.choices[initialIndex]);

            EventCallback<ChangeEvent<string>> callback = evt =>
            {
                int selectedIndex = dropdown.choices.IndexOf(evt.newValue);

                if (selectedIndex >= 0)
                    dissolveController.Axis = selectedIndex;

                soundController?.PlayToggle();
            };

            dropdown.RegisterValueChangedCallback(callback);

            unbindActions.Add(() => dropdown.UnregisterValueChangedCallback(callback));
        }

        private void BindDirection(VisualElement root, string controlName, List<string> choices, int initialIndex)
        {
            DropdownField dropdown = root.Q<DropdownField>(controlName);

            if (dropdown == null)
                return;

            dropdown.choices = new List<string>(choices);

            initialIndex = Mathf.Clamp(initialIndex, 0, dropdown.choices.Count - 1);

            dropdown.SetValueWithoutNotify(dropdown.choices[initialIndex]);

            EventCallback<ChangeEvent<string>> callback = evt =>
            {
                int selectedIndex = dropdown.choices.IndexOf(evt.newValue);

                if (selectedIndex >= 0)
                    dissolveController.Direction = selectedIndex;

                soundController?.PlayToggle();
            };

            dropdown.RegisterValueChangedCallback(callback);

            unbindActions.Add(() => dropdown.UnregisterValueChangedCallback(callback));
        }

        private void UnbindUI()
        {
            for (int index = unbindActions.Count - 1; index >= 0; index--)
            {
                unbindActions[index]?.Invoke();
            }

            unbindActions.Clear();
        }

        public void Hide()
        {
            UnbindUI();
        }
    }
}
