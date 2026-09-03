using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Prefab,
    PlayerSpawn,
    Enemy
}

[System.Serializable]
public class TileMapping
{
    [SerializeField] private string tag;
    [SerializeField] private Color color = Color.white;
    [SerializeField] private TileType type = TileType.Prefab;

    [Header("Prefab Settings")]
    [Tooltip("Used when Type is set to Prefab.")]
    [SerializeField] private GameObject prefab;

    public string Tag => tag;
    public Color Color => color;
    public TileType Type => type;
    public GameObject Prefab => prefab;
}

[CreateAssetMenu(fileName = "NewTilePalette", menuName = "Level Generation/Tile Palette")]
public class TilePalette : ScriptableObject
{
    public List<TileMapping> mappings = new List<TileMapping>();
}