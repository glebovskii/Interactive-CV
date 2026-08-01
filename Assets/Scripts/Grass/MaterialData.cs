using UnityEngine;
public struct MaterialData
{
    public Material Material;

    public MaterialData(Material material, int id, Texture texture)
    {
        Material = material;
        Material.SetTexture(id, texture);
    }
}
