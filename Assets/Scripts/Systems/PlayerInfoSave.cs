using UnityEngine;

public static class PlayerInfoSave
{
    private const string keyName = "Name";
    private const string keyColor_r = "Color_r";
    private const string keyColor_g = "Color_g";
    private const string keyColor_b = "Color_b";

    public static void SaveName(string newName)
    {
        PlayerPrefs.SetString(keyName, newName);
    }

    public static void SaveColor(Color newColor)
    {
        PlayerPrefs.SetFloat(keyColor_r, newColor.r);
        PlayerPrefs.SetFloat(keyColor_g, newColor.g);
        PlayerPrefs.SetFloat(keyColor_b, newColor.b);
    }

    public static string GetName()
    {
        return PlayerPrefs.GetString(keyName, "Player");
    }

    public static Color GetColor()
    {
        float r = PlayerPrefs.GetFloat(keyColor_r, 1);
        float g = PlayerPrefs.GetFloat(keyColor_g, 1);
        float b = PlayerPrefs.GetFloat(keyColor_b, 1);

        return new Color(r, g, b, 1);
    }
}
