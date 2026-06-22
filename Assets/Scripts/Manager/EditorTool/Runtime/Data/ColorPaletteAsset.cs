using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewColorPalette", menuName = "Editor/Color Palette")]
public class ColorPaletteAsset : ScriptableObject
{
    public string paletteName;
    public List<Color> colors = new List<Color>();
}
