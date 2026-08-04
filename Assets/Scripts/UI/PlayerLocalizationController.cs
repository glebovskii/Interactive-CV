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
    private readonly UICallbackBinder uiCallbacks = new();

    private PanelRenderer panelRenderer;
    private DropdownField localeField;
    private Coroutine initializationCoroutine;

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        if (initializationCoroutine != null)
        {
            StopCoroutine(initializationCoroutine);
            initializationCoroutine = null;
        }

        CleanupUI();
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        if (initializationCoroutine != null)
            StopCoroutine(initializationCoroutine);

        CleanupUI();

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
        availableLocales.AddRange(LocalizationSettings.AvailableLocales.Locales);

        var localeNames = new List<string>(availableLocales.Count);

        foreach (Locale locale in availableLocales)
            localeNames.Add(locale.LocaleName);

        field.choices = localeNames;

        int selectedIndex = availableLocales.IndexOf(LocalizationSettings.SelectedLocale);

        if (selectedIndex >= 0)
            field.SetValueWithoutNotify(localeNames[selectedIndex]);
        else if (localeNames.Count > 0)
            field.SetValueWithoutNotify(localeNames[0]);

        uiCallbacks.BindChange<string>(field, OnLanguageChanged, sound => sound.PlayToggle());
        initializationCoroutine = null;
    }

    private void OnLanguageChanged(string localeName)
    {
        int selectedIndex = localeField.choices.IndexOf(localeName);

        if (selectedIndex < 0 || selectedIndex >= availableLocales.Count)
        {
            Debug.LogError($"Invalid locale dropdown index: {selectedIndex}");
            return;
        }

        LocalizationSettings.SelectedLocale = availableLocales[selectedIndex];
    }

    private void CleanupUI()
    {
        uiCallbacks.Clear();
        localeField = null;
    }
}