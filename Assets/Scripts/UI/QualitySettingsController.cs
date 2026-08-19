using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class QualitySettingsController : MonoBehaviour
{
    private const int MobileIndex = 0;
    private const int PCIndex = 1;

    private readonly LocalizedString qualityText = new("ui", "menu.quality");
    private readonly LocalizedString mobileText = new("ui", "menu.quality_mobile");
    private readonly LocalizedString pcText = new("ui", "menu.quality_pc");

    private PanelRenderer panelRenderer;
    private Label qualityLabel;
    private DropdownField qualityDropdown;

    private string localizedQuality = "Quality";
    private string localizedMobile = "Mobile";
    private string localizedPC = "PC";

    private readonly UICallbackBinder uiCallbacks = new();


    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();

        qualityText.StringChanged += OnQualityTextChanged;
        mobileText.StringChanged += OnMobileTextChanged;
        pcText.StringChanged += OnPCTextChanged;

        panelRenderer.RegisterUIReloadCallback(OnUIReload);

        qualityText.RefreshString();
        mobileText.RefreshString();
        pcText.RefreshString();
    }

    private void OnDisable()
    {
        //qualityDropdown?.UnregisterValueChangedCallback(OnQualityChanged);
        uiCallbacks.Clear();
        panelRenderer?.UnregisterUIReloadCallback(OnUIReload);

        qualityText.StringChanged -= OnQualityTextChanged;
        mobileText.StringChanged -= OnMobileTextChanged;
        pcText.StringChanged -= OnPCTextChanged;
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        //qualityDropdown?.UnregisterValueChangedCallback(OnQualityChanged);
        uiCallbacks.Clear();

        qualityLabel = root.Q<Label>("quality-label");
        qualityDropdown = root.Q<DropdownField>("quality-dropdown");

        RefreshUI();
        //qualityDropdown?.RegisterValueChangedCallback(OnQualityChanged);
        uiCallbacks.BindChange<string>(qualityDropdown, OnQualityChanged, sound => sound.PlayToggle());
    }

    private void OnQualityChanged(string evt)
    {
        if (qualityDropdown == null)
            return;

        int selectedIndex = qualityDropdown.choices.IndexOf(evt);
        if (selectedIndex < 0 || selectedIndex >= qualityDropdown.choices.Count)
        {
            Debug.LogError($"Invalid quality dropdown index: {selectedIndex}");
            return;
        }

        PlayerInfoSave.SaveQualityIndex(qualityDropdown.index);
    }

    private void OnQualityTextChanged(string value)
    {
        localizedQuality = value;
        if (qualityLabel != null)
            qualityLabel.text = localizedQuality;
    }

    private void OnMobileTextChanged(string value)
    {
        localizedMobile = value;
        RefreshDropdown();
    }

    private void OnPCTextChanged(string value)
    {
        localizedPC = value;
        RefreshDropdown();
    }

    private void RefreshUI()
    {
        if (qualityLabel != null)
            qualityLabel.text = localizedQuality;

        RefreshDropdown();
    }

    private void RefreshDropdown()
    {
        if (qualityDropdown == null)
            return;

        int savedIndex = Mathf.Clamp(PlayerInfoSave.GetQualityIndex(), MobileIndex, PCIndex);
        qualityDropdown.choices = new List<string> { localizedMobile, localizedPC };
        qualityDropdown.SetValueWithoutNotify(qualityDropdown.choices[savedIndex]);
    }
}