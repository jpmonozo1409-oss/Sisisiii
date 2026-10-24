using System;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public int biome;
    public int money;
    public int keys;
    public int backgroundIndex;
    public int[] foods = new int[4];
    public int[] orbs = new int[7];
    public BiomeData[] biomes = new BiomeData[7];

    public static GameSaveData CreateNew()
    {
        var data = new GameSaveData();
        data.backgroundIndex = UnityEngine.Random.Range(0, 3);
        for (int i = 0; i < data.biomes.Length; i++)
        {
            data.biomes[i] = new BiomeData
            {
                unlocked = i == 0,
                foodTaken = new bool[4],
                wild = i == 0 ? new SlimeData { x = 4, y = 3, energy = 50 } : null,
                corral = null
            };
        }
        return data;
    }
}

[Serializable]
public class BiomeData
{
    public bool unlocked;
    public bool[] foodTaken = new bool[4];
    public SlimeData wild;
    public SlimeData corral;
}

[Serializable]
public class SlimeData
{
    public float x;
    public float y;
    public float energy;
    public float bob;
}

public static class GameConstants
{
    public const int Width = 10;
    public const int Height = 7;
    public static readonly Vector2Int StartBot = new Vector2Int(1, 5);
    public static readonly Vector2Int CorralCell = new Vector2Int(7, 4);
    public static readonly Vector2Int MarketCell = new Vector2Int(9, 1);
    public static readonly Vector2Int GateCell = new Vector2Int(1, 0);
    public static readonly Vector2Int[] FoodSpots =
    {
        new Vector2Int(2, 2),
        new Vector2Int(5, 5),
        new Vector2Int(8, 3),
        new Vector2Int(3, 5)
    };

    public static readonly string[] Biomes =
    {
        "Arrecife Seco", "Cantera Índigo", "Manta de Musgo", "Desierto Cristal",
        "Bosque Neón", "Volcán Violeta", "Cúpula Celeste"
    };

    public static readonly string[] Foods =
    {
        "Zanahoria Pogofa", "Fruta Cúbica", "Gallina Furia", "Núcleo Estelar"
    };

    public static readonly Color[] Sky =
    {
        new Color(.40f,.70f,1f), new Color(.20f,.30f,.50f), new Color(.10f,.40f,.20f),
        new Color(.80f,.50f,.20f), new Color(.10f,.60f,.55f), new Color(.35f,.10f,.45f),
        new Color(.20f,.25f,.60f)
    };

    public static readonly Color[] Ground =
    {
        new Color(.80f,.70f,.50f), new Color(.40f,.40f,.50f), new Color(.20f,.60f,.30f),
        new Color(.90f,.60f,.30f), new Color(.15f,.45f,.40f), new Color(.35f,.18f,.30f),
        new Color(.35f,.45f,.80f)
    };
}
