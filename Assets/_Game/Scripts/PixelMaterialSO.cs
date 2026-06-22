using UnityEngine;

[CreateAssetMenu(fileName = "PixelMaterialSO", menuName = "XGame/Pixel Material")]
public class PixelMaterialSO : ScriptableObject
{
    [SerializeField] private Material pixelMaterial;

    public Material PixelMaterial => pixelMaterial;
    public Material PixelMaterialNoOutline { get; private set; }
    public Material PixelMaterialUnlit { get; private set; }

    public void Initialized()
    {
        if (pixelMaterial == null)
        {
            PixelMaterialNoOutline = null;
            PixelMaterialUnlit = null;
            return;
        }

        PixelMaterialNoOutline = CreateMaterialWithShader(pixelMaterial, GetNoOutlineShader(pixelMaterial));
        PixelMaterialUnlit = CreateMaterialWithShader(pixelMaterial, Shader.Find("XGame/UnlitColorTexture"));
    }

    private static Material CreateMaterialWithShader(Material source, Shader shader)
    {
        var material = new Material(source);

        if (shader != null)
        {
            material.shader = shader;
        }

        return material;
    }

    private static Shader GetNoOutlineShader(Material material)
    {
        if (material.shader == null)
        {
            return null;
        }

        var shaderName = material.shader.name
            .Replace(" Outline", string.Empty)
            .Replace(" (Outline)", string.Empty);

        return Shader.Find(shaderName);
    }
}
