#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoutingTile))]
public class RoutingTileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 1. Draw all standard fields
        DrawDefaultInspector();

        RoutingTile tile = (RoutingTile)target;

        EditorGUILayout.Space(12);

        // 2. Prominent Button to trigger regeneration on demand
        GUI.backgroundColor = new Color(0.35f, 0.75f, 1f, 1f);
        if (GUILayout.Button("🔄 Regenerate Tile Icon", GUILayout.Height(32)))
        {
            tile.RegeneratePreviewSprite();
            
            // Mark dirty and refresh assets so Tile Palette picks it up immediately
            EditorUtility.SetDirty(tile);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        GUI.backgroundColor = Color.white;
    }
}
#endif