using UnityEngine;
using UnityEngine.Tilemaps;
using Gameplay.Base;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum TileRole
{
    Road,
    Spawn,
    Exit
}

[CreateAssetMenu(fileName = "NewRoutingTile", menuName = "Tower Defense/Tiles/Routing Tile")]
public class RoutingTile : TileBase
{
    [Header("Tile Role & ID")]
    public TileRole role = TileRole.Road;

    [Tooltip("Unique ID for this Spawn or Exit point (e.g. 1, 2, 6, 99)")]
    public int pointId = 1;

    [Tooltip("If this is a Spawn tile, which Exit ID should enemies navigate to?")]
    public int destinationExitId = 1;

    [Header("Exit Gameplay Data")]
    [Tooltip("Health points for this exit. Only used if role == TileRole.Exit")]
    [Min(1)] public int exitHealth = 20;

    [Header("Visuals & Colors")]
    public Color tileColor = Color.white;
    public GameObject prefab;

    [SerializeField, HideInInspector] private Sprite previewSprite;
    private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");
    private static MaterialPropertyBlock propertyBlock;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.gameObject = prefab;
        tileData.color = Color.white;
        tileData.flags = TileFlags.LockTransform | TileFlags.LockColor;
        tileData.colliderType = Tile.ColliderType.None;

        if (previewSprite == null)
        {
            RegeneratePreviewSprite();
        }

        tileData.sprite = previewSprite;
    }

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        if (go != null)
        {
            // 1. Color Tinting Juice
            if (role != TileRole.Road)
            {
                var renderer = go.GetComponentInChildren<MeshRenderer>();
                if (renderer != null)
                {
                    if (propertyBlock == null)
                    {
                        propertyBlock = new MaterialPropertyBlock();
                    }

                    renderer.GetPropertyBlock(propertyBlock);
                    propertyBlock.SetColor(BaseColorPropertyId, tileColor);
                    renderer.SetPropertyBlock(propertyBlock);
                }
            }

            // 2. Configure Exit Health Dynamically from this ScriptableObject
            if (role == TileRole.Exit)
            {
                var baseCore = go.GetComponent<BaseCore>();
                if (baseCore == null)
                {
                    baseCore = go.AddComponent<BaseCore>();
                }
                baseCore.Initialize(pointId, exitHealth);
            }
        }

        return base.StartUp(position, tilemap, go);
    }

    [ContextMenu("Regenerate Tile Icon")]
    public void RegeneratePreviewSprite()
    {
        previewSprite = GeneratePaletteBadge();
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RegeneratePreviewSprite();
    }
#endif

    private Sprite GeneratePaletteBadge()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isBorder = (x == 0 || x == size - 1 || y == 0 || y == size - 1);
                pixels[y * size + x] = isBorder ? new Color(0.12f, 0.12f, 0.12f, 1f) : tileColor;
            }
        }

        string badgeText = role switch
        {
            TileRole.Spawn => $"S{pointId}",
            TileRole.Exit => $"E{pointId}",
            _ => "RD"
        };

        int charWidth = 4;
        int spacing = 1;
        int totalWidth = (badgeText.Length * charWidth) + ((badgeText.Length - 1) * spacing);
        int startX = Mathf.Max(2, (size - totalWidth) / 2);
        int startY = 19;

        Color ink = new Color(0.08f, 0.08f, 0.08f, 0.95f);

        for (int i = 0; i < badgeText.Length && i < 4; i++)
        {
            DrawGlyph(pixels, size, badgeText[i], startX + i * (charWidth + spacing), startY, ink);
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private void DrawGlyph(Color[] pixels, int size, char c, int ox, int oy, Color color)
    {
        int[] rows = c switch
        {
            '0' => new[] { 0b0110, 0b1001, 0b1001, 0b1001, 0b0110 },
            '1' => new[] { 0b0100, 0b1100, 0b0100, 0b0100, 0b1110 },
            '2' => new[] { 0b1110, 0b0001, 0b0110, 0b1000, 0b1111 },
            '3' => new[] { 0b1110, 0b0001, 0b0110, 0b0001, 0b1110 },
            '4' => new[] { 0b1001, 0b1001, 0b1111, 0b0001, 0b0001 },
            '5' => new[] { 0b1111, 0b1000, 0b1110, 0b0001, 0b1110 },
            '6' => new[] { 0b0111, 0b1000, 0b1110, 0b1001, 0b0110 },
            '7' => new[] { 0b1111, 0b0001, 0b0010, 0b0100, 0b0100 },
            '8' => new[] { 0b0110, 0b1001, 0b0110, 0b1001, 0b0110 },
            '9' => new[] { 0b0110, 0b1001, 0b0111, 0b0001, 0b1110 },
            'S' => new[] { 0b0111, 0b1000, 0b0110, 0b0001, 0b1110 },
            'E' => new[] { 0b1111, 0b1000, 0b1110, 0b1000, 0b1111 },
            'R' => new[] { 0b1110, 0b1001, 0b1110, 0b1010, 0b1001 },
            'D' => new[] { 0b1110, 0b1001, 0b1001, 0b1001, 0b1110 },
            _   => new[] { 0b1111, 0b1001, 0b1001, 0b1001, 0b1111 }
        };

        for (int r = 0; r < 5; r++)
        {
            int bitRow = rows[r];
            int py = oy - (r * 2);
            for (int col = 0; col < 4; col++)
            {
                if ((bitRow & (1 << (3 - col))) != 0)
                {
                    int px = ox + col;
                    if (px >= 0 && px < size && py >= 0 && py < size)
                    {
                        pixels[py * size + px] = color;
                    }
                }
            }
        }
    }
}