using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class FastGridBackground : MonoBehaviour
{
    [Header("Grid Size (Kích thước lưới)")]
    public int width = 50;
    public int height = 50;

    [Header("Visual Settings")]
    public Color color1 = new Color(0.15f, 0.15f, 0.15f, 1f);
    public Color color2 = new Color(0.20f, 0.20f, 0.20f, 1f);
    public int sortingOrder = -20;

    [Header("Alignment (Khóa Tọa Độ)")]
    [Tooltip("Tự động ép lệch lưới đi -0.5 để tâm ô vuông khớp với tọa độ Rắn")]
    public bool autoAlignToGrid = true;

    private SpriteRenderer sr;
    private Texture2D tex;

    private void OnEnable()
    {
        GenerateGrid();
    }

    private void OnValidate()
    {
        GenerateGrid();
    }

    private void Update()
    {
        // Liên tục khóa tọa độ trong Editor, cấm người dùng kéo lệch lưới
        if (autoAlignToGrid && !Application.isPlaying)
        {
            EnforceGridAlignment();
        }
    }

    private void EnforceGridAlignment()
    {
        // Ép X và Y về đúng -0.5, giữ nguyên Z để lưới vẫn nằm dưới Rắn
        transform.position = new Vector3(-0.5f, -0.5f, transform.position.z);
    }

    [ContextMenu("Force Regenerate")]
    public void GenerateGrid()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null) return; 

        if (tex == null)
        {
            tex = new Texture2D(2, 2);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.hideFlags = HideFlags.HideAndDontSave;
        }

        tex.SetPixel(0, 0, color1);
        tex.SetPixel(1, 1, color1);
        tex.SetPixel(1, 0, color2);
        tex.SetPixel(0, 1, color2);
        tex.Apply();

        // Đưa Pivot về lại 0.5 để lưới tỏa ra đều 4 hướng (Tiled mode hoạt động ổn định nhất)
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.FullRect);
        sprite.hideFlags = HideFlags.HideAndDontSave;

        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(width, height);
        sr.sortingOrder = sortingOrder;

        if (autoAlignToGrid) EnforceGridAlignment();
    }
}