using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PortfolioDocumentController))]
public sealed class PortfolioLinkController : MonoBehaviour
{
    [Serializable]
    private struct LinkBinding
    {
        public string elementName;
        public string url;

        public LinkBinding(string elementName, string url)
        {
            this.elementName = elementName;
            this.url = url;
        }
    }

    [SerializeField] private List<LinkBinding> links = new();

    private readonly Dictionary<Button, Action> registeredHandlers = new();
    private PortfolioDocumentController documentController;

    private void Awake()
    {
        documentController = GetComponent<PortfolioDocumentController>();
    }

    private void OnEnable()
    {
        documentController.DocumentBuilt += BindLinks;

        if (documentController.Root != null)
            BindLinks(documentController.Root);
    }

    private void OnDisable()
    {
        documentController.DocumentBuilt -= BindLinks;
        UnbindLinks();
    }

    private void BindLinks(VisualElement root)
    {
        UnbindLinks();

        foreach (LinkBinding link in links)
        {
            if (string.IsNullOrWhiteSpace(link.elementName) ||
                string.IsNullOrWhiteSpace(link.url))
            {
                continue;
            }

            Button button = root.Q<Button>(link.elementName);
            if (button == null)
            {
                Debug.LogWarning(
                    $"Portfolio link element '{link.elementName}' was not found.",
                    this);
                continue;
            }

            string capturedUrl = link.url;
            Action handler = () => Application.OpenURL(capturedUrl);

            button.clicked += handler;
            registeredHandlers.Add(button, handler);
        }
    }

    private void UnbindLinks()
    {
        foreach (KeyValuePair<Button, Action> pair in registeredHandlers)
        {
            if (pair.Key != null)
                pair.Key.clicked -= pair.Value;
        }

        registeredHandlers.Clear();
    }

#if UNITY_EDITOR
    private void Reset()
    {
        links = new List<LinkBinding>
        {
            new LinkBinding("github-link", "https://github.com/Glebovvski"),
            new LinkBinding("linkedin-link", "https://www.linkedin.com/in/hlib-zadachyn-23a0b9117"),
            new LinkBinding("asset-store-link", "https://assetstore.unity.com/publishers/105938"),
            new LinkBinding("itch-link", "https://maidnmate.itch.io/art-guard"),
            new LinkBinding("cv-link", "https://drive.google.com/file/d/11f9qXg29zkJ181I5gof-Iw1IEaeAaYR-/view"),
            new LinkBinding("corepunk-link", "https://corepunk.com/en-gb"),
            new LinkBinding("last-pirate-link", "https://apps.apple.com/ua/app/id1449724605"),
            new LinkBinding("optor-store-link", "https://play.google.com/store/apps/developer?id=Optor+Group"),
            new LinkBinding("water-sort-link", "https://play.google.com/store/apps/details?id=com.OptorGroup.ColorTheDrawingGame"),
            new LinkBinding("sudoku-link", "https://play.google.com/store/apps/details?id=com.OptorGroup.Sudoku"),
            new LinkBinding("war-strategy-link", "https://play.google.com/store/apps/details?id=com.OptorGroup.WarStrategyOfBattleClans"),
            new LinkBinding("poker-link", "https://play.google.com/store/apps/details?id=com.OptorGroup.TexasHoldemPoker")
        };
    }
#endif
}
