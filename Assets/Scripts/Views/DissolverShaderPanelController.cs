using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

namespace Dissolver
{
    [RequireComponent(typeof(PanelRenderer))]
    public sealed class DissolverShaderPanelController : MonoBehaviour
    {

        [SerializeField] private PanelRenderer panelRenderer;

        private readonly List<Action> unbindActions = new();

        private PlayerDissolveController dissolveController;

        public void SetPlayer(PlayerDissolveController controller)
        {
            dissolveController = controller;

            if (panelRenderer == null)
                panelRenderer = GetComponent<PanelRenderer>();

            if (panelRenderer == null)
            {
                Debug.LogError($"{nameof(DissolverShaderPanelController)} requires a PanelRenderer.", this);

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

        private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
        {
            UnbindUI();

            BindEdge(root, "slider-edge-width");

            BindDissolve(root, "slider-dissolve");

            BindAxis(root, "dropdown-dissolve-direction", PlayerDissolveController.AxisChoices, dissolveController.Axis);

            BindDirection(root, "dropdown-dissolve-start", StartDirectionChoices, dissolveController.Direction);
        }

        private void BindDissolve(VisualElement root, string controlName)
        {
            Slider slider = root.Q<Slider>(controlName);

            if (slider == null)
                return;

            slider.SetValueWithoutNotify(dissolveController.Dissolve);

            EventCallback<ChangeEvent<float>> callback = evt => dissolveController.Dissolve = evt.newValue;

            slider.RegisterValueChangedCallback(callback);

            unbindActions.Add(() => slider.UnregisterValueChangedCallback(callback));
        }

        private void BindEdge(VisualElement root, string controlName)
        {
            Slider slider = root.Q<Slider>(controlName);

            if (slider == null)
                return;

            slider.SetValueWithoutNotify(dissolveController.EdgeWidth);

            EventCallback<ChangeEvent<float>> callback = evt => dissolveController.EdgeWidth = evt.newValue;

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
            };

            dropdown.RegisterValueChangedCallback(callback);

            unbindActions.Add(() => dropdown.UnregisterValueChangedCallback(callback));
        }

        private int GetSelectedIndex(int propertyId, string[] keywords)
        {
            for (int index = 0; index < keywords.Length; index++)
            {
                if (targetMaterial.IsKeywordEnabled(keywords[index]))
                    return index;
            }

            if (targetMaterial.HasProperty(propertyId))
            {
                return Mathf.RoundToInt(targetMaterial.GetFloat(propertyId));
            }

            return 0;
        }

        private void SetStartDirection(int selectedIndex)
        {
            selectedIndex = Mathf.Clamp(selectedIndex, 0, StartDirectionKeywords.Length - 1);

            if (targetMaterial.HasProperty(StartDirectionProperty))
            {
                targetMaterial.SetFloat(StartDirectionProperty, selectedIndex);
            }

            SetExclusiveKeyword(StartDirectionKeywords, selectedIndex);
        }

        private void SetExclusiveKeyword(string[] keywords, int enabledIndex)
        {
            for (int index = 0; index < keywords.Length; index++)
            {
                if (index == enabledIndex)
                    targetMaterial.EnableKeyword(keywords[index]);
                else
                    targetMaterial.DisableKeyword(keywords[index]);
            }
        }

        private void UnbindUI()
        {
            for (int index = unbindActions.Count - 1; index >= 0; index--)
            {
                unbindActions[index]?.Invoke();
            }

            unbindActions.Clear();
        }
    }
}
