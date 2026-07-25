using UnityEngine;

public class CharacterPreviewPresenter : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer mesh;
    [SerializeField] private Animator animator;

    private int triggerId = Animator.StringToHash("ColorChange");

    private Material material;
    private ColorPickerController colorPickerController;

    public void Init()
    {
        material = mesh.sharedMaterial;
        material.color = PlayerInfoSave.GetColor();
        if (ServiceLocator.TryGet<ColorPickerController>(out colorPickerController))
        {
            colorPickerController.ColorChanged += OnColorChanged;
            colorPickerController.ColorPicked += OnColorSelected;
        }
    }

    private void OnColorSelected()
    {
        var currentState = animator.GetCurrentAnimatorStateInfo(0);
        if (currentState.IsName("Base.LookAround"))
        {
            animator.ResetTrigger(triggerId);
            return;
        }
        animator.SetTrigger(triggerId);
        PlayerInfoSave.SaveColor(material.color);
    }

    private void OnColorChanged(Color color)
    {
        if (material == null)
        {
            Debug.LogError("NO MATERILA FOUND ON CHARATCER PREVIEW");
            return;
        }

        material.color = color;
    }

    private void OnDestroy()
    {
        if (colorPickerController != null)
        {
            colorPickerController.ColorChanged -= OnColorChanged;
            colorPickerController.ColorPicked -= OnColorSelected;
        }
    }
}
