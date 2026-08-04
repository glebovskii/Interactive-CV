using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class PlayerLocalizationController : MonoBehaviour
{
    private readonly List<Locale> availableLocales = new();

    private PanelRenderer panelRenderer;
    private DropdownField localeField;
    private Coroutine initializationCoroutine;

    private UISoundController soundController;

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
        ServiceLocator.TryGet(out soundController);
    }

    private void OnDisable()
    {
        if (panelRenderer != null)
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        if (initializationCoroutine != null)
        {
            StopCoroutine(initializationCoroutine);
            initializationCoroutine = null;
        }

        UnregisterDropdown();
    }

    private void OnUIReload(
        PanelRenderer renderer,
        VisualElement root,
        int version)
    {
        if (initializationCoroutine != null)
            StopCoroutine(initializationCoroutine);

        UnregisterDropdown();

        localeField = root.Q<DropdownField>("DropdownField");

        if (localeField == null)
        {
            Debug.LogError("DropdownField named 'DropdownField' was not found.");
            return;
        }

        initializationCoroutine = StartCoroutine(InitializeLocaleDropdown(localeField));
    }

    private IEnumerator InitializeLocaleDropdown(DropdownField field)
    {
        yield return LocalizationSettings.InitializationOperation;

        if (!isActiveAndEnabled || field != localeField)
            yield break;

        availableLocales.Clear();
        availableLocales.AddRange(
            LocalizationSettings.AvailableLocales.Locales);

        var localeNames = new List<string>(availableLocales.Count);

        foreach (Locale locale in availableLocales)
        {
            localeNames.Add(locale.LocaleName);
        }

        field.choices = localeNames;

        Locale selectedLocale = LocalizationSettings.SelectedLocale;
        int selectedIndex = availableLocales.IndexOf(selectedLocale);

        if (selectedIndex >= 0)
        {
            field.SetValueWithoutNotify(localeNames[selectedIndex]);
        }
        else if (localeNames.Count > 0)
        {
            field.SetValueWithoutNotify(localeNames[0]);
        }

        field.RegisterCallback<ChangeEvent<string>>(OnLanguageChanged);

        initializationCoroutine = null;
    }

    private void OnLanguageChanged(ChangeEvent<string> evt)
    {
        if (localeField == null)
            return;

        if(soundController == null)
            ServiceLocator.TryGet(out soundController);
        soundController?.PlayToggle();

        int selectedIndex = localeField.index;

        if (selectedIndex < 0 ||
            selectedIndex >= availableLocales.Count)
        {
            Debug.LogError(
                $"Invalid locale dropdown index: {selectedIndex}");

            return;
        }

        Locale selectedLocale = availableLocales[selectedIndex];

        LocalizationSettings.SelectedLocale = selectedLocale;
    }

    private void UnregisterDropdown()
    {
        if (localeField == null)
            return;

        localeField.UnregisterCallback<ChangeEvent<string>>(OnLanguageChanged);

        localeField = null;
    }
}