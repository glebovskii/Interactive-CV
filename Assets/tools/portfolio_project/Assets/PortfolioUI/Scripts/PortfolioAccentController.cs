using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PortfolioDocumentController))]
public sealed class PortfolioAccentController : MonoBehaviour
{
    [SerializeField] private Color iconTint = new Color(0.23f, 0.8f, 0.75f, 1f);

    private PortfolioDocumentController documentController;
    private VisualElement currentRoot;

    private void Awake()
    {
        documentController = GetComponent<PortfolioDocumentController>();
    }

    private void OnEnable()
    {
        documentController.DocumentBuilt += OnDocumentBuilt;

        if (documentController.Root != null)
            OnDocumentBuilt(documentController.Root);
    }

    private void OnDisable()
    {
        documentController.DocumentBuilt -= OnDocumentBuilt;
        currentRoot = null;
    }

    public void SetIconTint(Color color)
    {
        iconTint = color;
        ApplyTint();
    }

    private void OnDocumentBuilt(VisualElement root)
    {
        currentRoot = root;
        ApplyTint();
    }

    private void ApplyTint()
    {
        if (currentRoot == null)
            return;

        currentRoot
            .Query<VisualElement>(className: "accent-tint")
            .ForEach(element =>
                element.style.unityBackgroundImageTintColor = iconTint);
    }
}
