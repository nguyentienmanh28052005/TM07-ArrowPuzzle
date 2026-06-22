using master;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : Singleton<ColorManager>
{
    [SerializeField] List<PixelMaterialSO> pixelMaterialSOs = new List<PixelMaterialSO>();
    [SerializeField] Material surprisePixelMaterial;
    [SerializeField] Material surpriseShooterMaterial;
    [SerializeField] Material iceShooterMaterial;
    public Shader pixelShader;
    public Material SurprisePixelMaterial => surprisePixelMaterial;
    public Material SurpriseShooterMaterial => surpriseShooterMaterial;
    public Material IceShooterMaterial => iceShooterMaterial;
    public Material SurpriseShooterMaterialNoOutline { get; set; }
    public Material SurprisePixelMaterialNoOutline { get; set; }
    public Material SurprisePixelMaterialUnlit { get; set; }
    public Material IceShooterMaterialNoOutline { get; set; }


    private void Start()
    {
        foreach (var pixelMaterial in pixelMaterialSOs)
        {
            if (pixelMaterial == null)
            {
                continue;
            }

            pixelMaterial.Initialized();
        }
        if (SurpriseShooterMaterialNoOutline == null)
        {
            SurpriseShooterMaterialNoOutline = new Material(surpriseShooterMaterial);
            SurpriseShooterMaterialNoOutline.shader = Shader.Find(surpriseShooterMaterial.shader.name.Replace(" Outline", "").Replace(" (Outline)", ""));
        }
        if (SurprisePixelMaterialNoOutline == null)
        {
            SurprisePixelMaterialNoOutline = new Material(surprisePixelMaterial);
            SurprisePixelMaterialNoOutline.shader = Shader.Find(surprisePixelMaterial.shader.name.Replace(" Outline", "").Replace(" (Outline)", ""));
        }
        
        if (SurprisePixelMaterialUnlit == null)
        {
            SurprisePixelMaterialUnlit = new Material(surprisePixelMaterial);
            SurprisePixelMaterialUnlit.shader = Shader.Find("XGame/UnlitColorTexture");
        }
        if (IceShooterMaterialNoOutline == null)
        {
            IceShooterMaterialNoOutline = new Material(iceShooterMaterial);
            IceShooterMaterialNoOutline.shader = Shader.Find(iceShooterMaterial.shader.name.Replace(" Outline", "").Replace(" (Outline)", ""));
        }
    }

    public PixelMaterialSO GetPixelMaterialColorSO(int idColor)
    {
        if (pixelMaterialSOs == null || pixelMaterialSOs.Count == 0)
        {
            return null;
        }

        return pixelMaterialSOs[Mathf.Clamp(idColor, 0, pixelMaterialSOs.Count - 1)];
    }
}
