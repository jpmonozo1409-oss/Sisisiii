using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameSaveData Save { get; private set; }
    public Vector2Int BotPosition { get; private set; }
    public bool IsBusy { get; set; }
    public string LastOutput { get; private set; } = "Listo. Escribe un programa y pulsa EJECUTAR.";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        NewGame();
    }


    public void NewGame()
    {
        Save = GameSaveData.CreateNew();

        // Posición aleatoria del bot
        BotPosition = new Vector2Int(
            Random.Range(0, GameConstants.Width),
            Random.Range(0, GameConstants.Height)
        );

        // Evitar que el bot aparezca en lugares especiales
        while (BotPosition == GameConstants.CorralCell ||
               BotPosition == GameConstants.MarketCell ||
               BotPosition == GameConstants.GateCell)
        {
            BotPosition = new Vector2Int(
                Random.Range(0, GameConstants.Width),
                Random.Range(0, GameConstants.Height)
            );
        }

        // Posición aleatoria del slime salvaje
        Vector2Int slimePosition;

        do
        {
            slimePosition = new Vector2Int(
                Random.Range(0, GameConstants.Width),
                Random.Range(0, GameConstants.Height)
            );
        }
        while (slimePosition == BotPosition ||
               slimePosition == GameConstants.CorralCell ||
               slimePosition == GameConstants.MarketCell ||
               slimePosition == GameConstants.GateCell);

        Save.biomes[0].wild.x = slimePosition.x;
        Save.biomes[0].wild.y = slimePosition.y;

        IsBusy = false;
        LastOutput = "Mundo nuevo creado. Bioma: " + GameConstants.Biomes[0];
    }


    private const string SaveKey = "LudusOrbes_Save_v1";

    public void SaveGame()
    {
        if (Save == null) return;

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Save));

        PlayerPrefs.SetInt("LudusOrbes_BotX", BotPosition.x);
        PlayerPrefs.SetInt("LudusOrbes_BotY", BotPosition.y);

        PlayerPrefs.Save();
    }

    public bool LoadGame()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return false;
        try
        {
            Save = JsonUtility.FromJson<GameSaveData>(PlayerPrefs.GetString(SaveKey));
            if (Save == null || Save.biomes == null || Save.biomes.Length != 7) return false;
            for (int i = 0; i < Save.biomes.Length; i++)
            {
                bool[] previousFood = Save.biomes[i].foodTaken;

                if (previousFood == null || previousFood.Length != 4)
                {
                    bool[] updatedFood = new bool[4];

                    if (previousFood != null)
                    {
                        for (int j = 0; j < Mathf.Min(previousFood.Length, 4); j++)
                        {
                            updatedFood[j] = previousFood[j];
                        }
                    }

                    Save.biomes[i].foodTaken = updatedFood;
                }
            }
            int botX = PlayerPrefs.GetInt(
                "LudusOrbes_BotX",
                GameConstants.StartBot.x
            );

            int botY = PlayerPrefs.GetInt(
                "LudusOrbes_BotY",
                GameConstants.StartBot.y
            );

            BotPosition = new Vector2Int(botX, botY);
            LastOutput = "Partida guardada cargada.";
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        NewGame();
        SaveGame();
        Log("Partida reiniciada y guardada.");
    }

    public BiomeData CurrentBiome => Save.biomes[Save.biome];
    public int CurrentFoodIndex => Mathf.Clamp(Save.biome, 0, 2);
    public string CurrentFoodName => GameConstants.Foods[CurrentFoodIndex];

    public void Log(string message)
    {
        LastOutput += "\n" + message;
        if (LastOutput.Length > 6000) LastOutput = LastOutput.Substring(LastOutput.Length - 6000);
    }

    public bool TryMove(Vector2Int delta)
    {
        Vector2Int next = BotPosition + delta;
        if (next.x < 0 || next.x >= GameConstants.Width || next.y < 0 || next.y >= GameConstants.Height)
        {
            Log("Movimiento bloqueado: fuera del mapa.");
            return false;
        }
        BotPosition = next;
        SaveGame();
        return true;
    }

    public IEnumerator MoveBot(Vector2Int delta)
    {
        if (IsBusy == false) IsBusy = true;
        Vector2Int start = BotPosition;
        Vector2Int next = start + delta;
        if (next.x < 0 || next.x >= GameConstants.Width || next.y < 0 || next.y >= GameConstants.Height)
        {
            Log("Movimiento bloqueado: fuera del mapa.");
            yield return new WaitForSeconds(.12f);
            yield break;
        }
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            yield return null;
        }
        BotPosition = next;
        SaveGame();
    }

    public bool PickupFood()
    {
        var b = CurrentBiome;
        for (int i = 0; i < GameConstants.FoodSpots.Length; i++)
        {
            if (!b.foodTaken[i] && BotPosition == GameConstants.FoodSpots[i])
            {
                b.foodTaken[i] = true;
                Save.foods[CurrentFoodIndex]++;
                SaveGame();
                Log("Recogido: " + CurrentFoodName + ". Inventario: " + Save.foods[CurrentFoodIndex]);
                return true;
            }
        }
        Log("No hay alimento aquí.");
        return false;
    }

    public bool Capture()
    {
        var b = CurrentBiome;
        if (b.corral != null) { Log("El corral ya tiene un slime."); return false; }
        if (b.wild == null) { Log("No hay slime salvaje."); return false; }
        Vector2Int wildCell = new Vector2Int(Mathf.RoundToInt(b.wild.x), Mathf.RoundToInt(b.wild.y));
        if (BotPosition != wildCell) { Log("Debes estar en la misma celda que el slime."); return false; }
        b.corral = b.wild;
        b.corral.x = GameConstants.CorralCell.x;
        b.corral.y = GameConstants.CorralCell.y;
        b.wild = null;
        SaveGame();
        Log("Slime capturado exitosamente.");
        return true;
    }


    public bool Feed()
    {
        var b = CurrentBiome;

        if (b.corral == null)
        {
            Log("No hay slime en el corral.");
            return false;
        }

        if (BotPosition != GameConstants.CorralCell)
        {
            Log("Debes estar en el corral para alimentar.");
            return false;
        }

        if (Save.foods[CurrentFoodIndex] <= 0)
        {
            Log("No tienes " + CurrentFoodName + ".");
            return false;
        }

        Save.foods[CurrentFoodIndex]--;

        b.corral.energy = Mathf.Min(100, b.corral.energy + 25);

        Log("Slime alimentado. Energía: " +
            Mathf.RoundToInt(b.corral.energy) + "%");

        if (b.corral.energy >= 100)
        {
            Save.orbs[Save.biome]++;
            b.corral.energy = 0;

            Log("¡Generó un orbe! Total del bioma: " +
                Save.orbs[Save.biome]);
        }

        SaveGame();
        return true;
    }


    public bool SellOrbs()
    {
        if (BotPosition != GameConstants.MarketCell) { Log("Debes estar en el mercado."); return false; }
        int gain = 0;
        for (int i = 0; i < Save.orbs.Length; i++)
        {
            gain += Save.orbs[i] * ((i + 1) * 10);
            Save.orbs[i] = 0;
        }
        Save.money += gain;
        SaveGame();
        Log(gain > 0 ? "Venta realizada: +" + gain + " Brillólares." : "No tienes orbes para vender.");
        return gain > 0;
    }

    public bool BuyKey()
    {
        if (BotPosition != GameConstants.MarketCell) { Log("Debes estar en el mercado."); return false; }
        int cost = 4 + Save.biome;
        if (Save.orbs[Save.biome] < cost) { Log("Necesitas " + cost + " orbes del bioma actual."); return false; }
        Save.orbs[Save.biome] -= cost;
        Save.keys++;
        SaveGame();
        Log("Llave comprada. Llaves: " + Save.keys);
        return true;
    }

    public bool NextBiome()
    {
        if (BotPosition != GameConstants.GateCell) { Log("Debes estar en la puerta."); return false; }
        int next = Save.biome + 1;
        if (next >= GameConstants.Biomes.Length) { Log("Ya estás en el último bioma."); return false; }
        if (!Save.biomes[next].unlocked)
        {
            if (Save.keys < 1) { Log("Necesitas una llave para abrir el siguiente bioma."); return false; }
            Save.keys--;
            Save.biomes[next].unlocked = true;
            if (Save.biomes[next].wild == null)
                Save.biomes[next].wild = new SlimeData { x = 4, y = 3, energy = 50 };
            Log("Bioma desbloqueado: " + GameConstants.Biomes[next]);
        }
        Save.biome = next;
        BotPosition = GameConstants.StartBot;
        SaveGame();
        Log("Viajaste a " + GameConstants.Biomes[next] + ".");
        return true;
    }

    public void ResetCurrentBiomeFood()
    {
        CurrentBiome.foodTaken = new bool[4];
        SaveGame();
    }

    public float SlimeEnergy()
    {
        return CurrentBiome.corral != null ? CurrentBiome.corral.energy : (CurrentBiome.wild != null ? CurrentBiome.wild.energy : 0);
    }

    public float DistanceToSlime()
    {
        if (CurrentBiome.wild == null) return 999;
        return Vector2.Distance(BotPosition, new Vector2(CurrentBiome.wild.x, CurrentBiome.wild.y));
    }
}
