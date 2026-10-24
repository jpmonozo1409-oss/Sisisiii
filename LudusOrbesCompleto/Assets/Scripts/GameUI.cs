using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    private BotInterpreter interpreter;
    private Texture2D whiteTex, corralTex, marketTex, gateTex, botTex, orbTex;
    private Texture2D[] backgroundTex = new Texture2D[3];
    private Texture2D[] foodTex = new Texture2D[3];
    private Texture2D[] slimeTex = new Texture2D[7];
    private GUIStyle label, title, button, codeStyle, outputStyle, variableStyle, badge;
    private Vector2 outputScroll, variableScroll;
    private string code = "";
    private const string BotCodeKey = "LudusOrbes_BotCode";
    private bool stylesReady;
    private float anim;

    private void Awake()
    {
        interpreter = gameObject.AddComponent<BotInterpreter>();
        CreateTextures();

        if (PlayerPrefs.HasKey(BotCodeKey))
            code = PlayerPrefs.GetString(BotCodeKey);
        else
            code = DefaultProgram();
    }


    private void Update()
    {
        anim += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Return) &&
            (Input.GetKey(KeyCode.LeftAlt) ||
             Input.GetKey(KeyCode.RightAlt)))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }



    private void InitStyles()
    {
        if (stylesReady) return;
        stylesReady = true;

        int baseFont = Mathf.Clamp(Screen.height / 45, 12, 18);

        label = new GUIStyle(GUI.skin.label)
        {
            fontSize = baseFont,
            wordWrap = true,
            richText = true
        };
        label.normal.textColor = Color.white;

        title = new GUIStyle(label)
        {
            fontSize = Mathf.Clamp(baseFont + 4, 16, 24),
            fontStyle = FontStyle.Bold
        };

        button = new GUIStyle(GUI.skin.button)
        {
            fontSize = Mathf.Clamp(baseFont - 2, 10, 14),
            wordWrap = true,
            alignment = TextAnchor.MiddleCenter
        };

        codeStyle = new GUIStyle(GUI.skin.textArea)
        {
            fontSize = Mathf.Clamp(baseFont - 1, 11, 15),
            wordWrap = true,
            richText = false
        };
        codeStyle.normal.background = Solid(new Color(.035f, .055f, .075f, 1));
        codeStyle.normal.textColor = new Color(.90f, .97f, 1f);
        codeStyle.padding = new RectOffset(8, 8, 8, 8);

        outputStyle = new GUIStyle(GUI.skin.textArea)
        {
            fontSize = Mathf.Clamp(baseFont - 2, 10, 13),
            wordWrap = true
        };
        outputStyle.normal.background = Solid(new Color(.035f, .07f, .045f, 1));
        outputStyle.normal.textColor = new Color(.85f, 1f, .88f);

        variableStyle = new GUIStyle(label)
        {
            fontSize = Mathf.Clamp(baseFont - 2, 10, 13)
        };
        variableStyle.normal.textColor = new Color(.65f, 1f, .75f);

        badge = new GUIStyle(label)
        {
            fontSize = Mathf.Clamp(baseFont - 2, 10, 13),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        badge.normal.textColor = Color.white;
    }


    private void OnGUI()
    {
        InitStyles();
        float leftW = Screen.width * .57f;
        DrawWorld(new Rect(0,0,leftW,Screen.height));
        DrawEditor(new Rect(leftW,0,Screen.width-leftW,Screen.height));
    }

    private void DrawWorld(Rect r)
    {
        GameManager gm = GameManager.Instance;

        // Fondo exterior: cambia según el mundo seleccionado.
        int bgIndex = Mathf.Clamp(gm.Save.backgroundIndex, 0, 2);
        Texture2D selectedBackground = backgroundTex[bgIndex];

        GUI.color = Color.white;

        if (selectedBackground != null)
        {
            GUI.DrawTexture(r, selectedBackground, ScaleMode.StretchToFill);
        }
        else
        {
            GUI.color = GameConstants.Sky[gm.Save.biome];
            GUI.DrawTexture(r, whiteTex);
            GUI.color = Color.white;
        }

        float header = 82;
        GUI.color = new Color(.025f,.10f,.11f,.96f);
        GUI.DrawTexture(new Rect(r.x,r.y,r.width,header),whiteTex);
        GUI.color = Color.white;
        GUI.Label(new Rect(r.x+12,r.y+6,r.width-24,28),"Rancho de Orbes — Programación",title);

        GUI.Label(new Rect(r.x + 12, r.y + 39, r.width - 24, 22),
            "Bioma: " + GameConstants.Biomes[gm.Save.biome] +
            "  |  Dinero: " + gm.Save.money +
            "  |  Llaves: " + gm.Save.keys, label);

        GUI.Label(new Rect(r.x + 12, r.y + 60, r.width - 24, 20),
            "Comida: " + gm.Save.foods[gm.CurrentFoodIndex] +
            "  |  Orbes: " + gm.Save.orbs[gm.Save.biome], label);


        float margin = 14;
        // Reservamos espacio inferior para sensores, instrucciones y leyenda visual.
        Rect gridArea = new Rect(r.x + margin, r.y + header + 8,
            r.width - margin * 2, r.height - header - margin * 2 - 165);
        float cell=Mathf.Min(gridArea.width/10f,gridArea.height/7f);
        float ox=gridArea.x+(gridArea.width-cell*10)*.5f;
        float oy=gridArea.y+(gridArea.height-cell*7)*.5f;

        Rect boardRect = new Rect(ox, oy, cell * 10, cell * 7);

        // Tablero beige sobre el fondo azul original.
        GUI.color = new Color(0.78f, 0.68f, 0.49f, 1f);
        GUI.DrawTexture(boardRect, whiteTex);
        GUI.color = Color.white;

        GUI.color=new Color(1,1,1,.18f);
        for(int x=0;x<=10;x++) GUI.DrawTexture(new Rect(ox+x*cell,oy,1,cell*7),whiteTex);
        for(int y=0;y<=7;y++) GUI.DrawTexture(new Rect(ox,oy+y*cell,cell*10,1),whiteTex);
        GUI.color=Color.white;

        Rect corr = Cell(GameConstants.CorralCell.x - 1, GameConstants.CorralCell.y - 1,
            ox, oy, cell, 2, 2);
        GUI.DrawTexture(corr, corralTex, ScaleMode.StretchToFill, true);
        GUI.Label(new Rect(corr.x + 5, corr.y + 4, corr.width - 10, 18), "CORRAL", badge);


        int orbCount = gm.Save.orbs[gm.Save.biome];

        if (orbCount > 0)
        {
            float orbSize = cell * 0.42f;

            Rect orbRect = new Rect(
                corr.x + corr.width - orbSize * 0.65f,
                corr.y - orbSize * 0.25f,
                orbSize,
                orbSize
            );

            GUI.DrawTexture(
                orbRect,
                orbTex,
                ScaleMode.ScaleToFit,
                true
            );

            GUI.Label(
                new Rect(
                    orbRect.x,
                    orbRect.y - 18,
                    orbSize + 20,
                    18
                ),
                "x" + orbCount,
                badge
            );
        }


        GUI.DrawTexture(Cell(GameConstants.MarketCell.x, GameConstants.MarketCell.y, ox, oy, cell),
            marketTex, ScaleMode.ScaleToFit, true);
        GUI.DrawTexture(Cell(GameConstants.GateCell.x, GameConstants.GateCell.y, ox, oy, cell),
            gateTex, ScaleMode.ScaleToFit, true);


        /*if (gm.Save.orbs[gm.Save.biome] > 0)
        {
            Rect orbCell = Cell(
                GameConstants.CorralCell.x,
                GameConstants.CorralCell.y,
                ox, oy, cell);

            float orbSize = cell * 0.38f;

            Rect orbRect = new Rect(
                orbCell.x + orbCell.width - orbSize * 0.8f,
                orbCell.y - orbSize * 0.15f,
                orbSize,
                orbSize);

            GUI.DrawTexture(
                orbRect,
                orbTex,
                ScaleMode.ScaleToFit,
                true);
        }*/


        var b = gm.CurrentBiome;
        for (int i = 0; i < GameConstants.FoodSpots.Length; i++)
        {
            if (!b.foodTaken[i])
            {
                Rect foodCell = Cell(
                    GameConstants.FoodSpots[i].x,
                    GameConstants.FoodSpots[i].y,
                    ox, oy, cell
                );

                GUI.DrawTexture(
                    foodCell,
                    foodTex[i % foodTex.Length],
                    ScaleMode.ScaleToFit,
                    true
                );
            }
        }

        if (b.wild != null)
        {
            DrawSlime(b.wild, ox, oy, cell, false, gm.Save.biome);
            Vector2Int wildCell = new Vector2Int(Mathf.RoundToInt(b.wild.x), Mathf.RoundToInt(b.wild.y));
            Rect wr = Cell(wildCell.x, wildCell.y, ox, oy, cell);
            GUI.Label(new Rect(wr.x, wr.y + cell * .72f, cell, cell * .22f), "SLIME", badge);
        }

        if (b.corral != null)
        {
            DrawSlime(b.corral, ox, oy, cell, true, gm.Save.biome);
            Rect er = new Rect(corr.x + corr.width * .18f, corr.y + corr.height * .78f,
                corr.width * .64f, Mathf.Max(4, cell * .07f));
            GUI.color = new Color(.05f, .08f, .05f, .9f);
            GUI.DrawTexture(er, whiteTex);
            GUI.color = b.corral.energy >= 100 ? new Color(.3f, 1f, .45f) : new Color(1f, .75f, .2f);
            GUI.DrawTexture(new Rect(er.x, er.y, er.width * Mathf.Clamp01(b.corral.energy / 100f), er.height), whiteTex);
            GUI.color = Color.white;
        }

        Vector2Int bot = gm.BotPosition;
        Rect br = Cell(bot.x, bot.y, ox, oy, cell);
        br = new Rect(br.x + cell * .14f, br.y + cell * .14f, cell * .72f, cell * .72f);
        GUI.DrawTexture(br, botTex, ScaleMode.ScaleToFit, true);

        Rect info = new Rect(r.x + 12, r.height - 152, r.width - 24, 138);
        GUI.color = new Color(.02f, .08f, .08f, .96f);
        GUI.DrawTexture(info, whiteTex);
        GUI.color = Color.white;
        GUI.Label(new Rect(info.x + 10, info.y + 5, info.width - 20, 20), "Sensores del bot", title);
        GUI.Label(new Rect(info.x + 10, info.y + 29, info.width - 20, 20),
            "Posición = (" + gm.BotPosition.x + "," + gm.BotPosition.y + ")   ·   Comida = " +
            gm.Save.foods[gm.CurrentFoodIndex] + "   ·   ✦ Orbes = " + gm.Save.orbs[gm.Save.biome] +
            "   ·   Energía del slime = " + Mathf.RoundToInt(gm.SlimeEnergy()) + "%", label);
        GUI.Label(new Rect(info.x + 10, info.y + 51, info.width - 20, 30),
            "CAPTURAR: llega a la misma casilla del slime y ejecuta capturar().  ALIMENTAR: ve al corral y ejecuta alimentar().",
            label);

        float legendY = info.y + 91;
        float itemW = (info.width - 20) / 7f;
        DrawLegendItem(info.x + 10 + itemW * 0, legendY, foodTex[0], "Comida");
        DrawLegendItem(info.x + 10 + itemW * 1, legendY, slimeTex[gm.Save.biome], "Slime");
        DrawLegendItem(info.x + 10 + itemW * 2, legendY, botTex, "Bot");
        DrawLegendItem(info.x + 10 + itemW * 3, legendY, corralTex, "Corral");
        DrawLegendItem(info.x + 10 + itemW * 4, legendY, orbTex, "Orbe");
        DrawLegendItem(info.x + 10 + itemW * 5, legendY, marketTex, "Mercado");
        DrawLegendItem(info.x + 10 + itemW * 6, legendY, gateTex, "Puerta");
    }

    private Rect Cell(int x, int y, float ox, float oy, float cell)
    {
        float visualY = GameConstants.Height - 1 - y;
        return new Rect(ox + x * cell, oy + visualY * cell, cell, cell);
    }

    private Rect Cell(int x, int y, float ox, float oy, float cell, float w, float h)
    {
        float visualY = GameConstants.Height - y - h;
        return new Rect(ox + x * cell, oy + visualY * cell, cell * w, cell * h);
    }

    private void DrawSlime(SlimeData slime, float ox, float oy, float cell, bool resident, int biome)
    {
        float bounce = Mathf.Sin(anim * 3f + slime.x) * .04f;
        float size = cell * (resident ? .62f : .76f);
        float visualY = GameConstants.Height - 1 - slime.y;
        Rect sr = new Rect(
            ox + slime.x * cell + (cell - size) * .5f,
            oy + visualY * cell + (cell - size) * .5f - cell * bounce,
            size, size * (1f - bounce));
        GUI.DrawTexture(sr, slimeTex[biome], ScaleMode.ScaleToFit, true);
    }

    private void DrawLegendItem(float x, float y, Texture2D icon, string text)
    {
        GUI.DrawTexture(new Rect(x, y, 18, 18), icon, ScaleMode.ScaleToFit, true);
        GUI.Label(new Rect(x + 22, y - 1, 90, 20), text, variableStyle);
    }

    private void DrawEditor(Rect r)
    {
        GUI.color=new Color(.08f,.10f,.13f,1); GUI.DrawTexture(r,whiteTex); GUI.color=Color.white;
        GUILayout.BeginArea(new Rect(r.x+9,r.y+8,r.width-18,r.height-16));

        // Botón para guardar y volver al menú principal.
        if (GUILayout.Button("VOLVER AL MENÚ", button,
    GUILayout.Height(32)))
        {
            PlayerPrefs.SetString(BotCodeKey, code);
            PlayerPrefs.Save();

            interpreter.Stop();

            GameManager.Instance.SaveGame();

            SceneManager.LoadScene("MenuPrincipal");
        }

        GUILayout.Space(8);
        GUILayout.Label("Editor del bot",title);
        GUILayout.Label("Lenguaje educativo en español · variables · sensores · if/else · mientras · repetir · funciones",label);
        GUILayout.BeginHorizontal();
        GUI.enabled=!interpreter.Running;
        if(GUILayout.Button("EJECUTAR",button,GUILayout.Height(32))) interpreter.Run(code);
        GUI.enabled=interpreter.Running;
        if(GUILayout.Button("DETENER",button,GUILayout.Height(32))) interpreter.Stop();
        GUI.enabled=true;
        if(GUILayout.Button("BÁSICO",button,GUILayout.Height(32))) code=BasicTutorial();
        if(GUILayout.Button("AVANZADO",button,GUILayout.Height(32))) code=AdvancedTutorial();
        if(GUILayout.Button("NUEVO MUNDO",button,GUILayout.Height(32))) { interpreter.Stop(); GameManager.Instance.NewGame(); code=DefaultProgram(); }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUI.color=interpreter.Running?new Color(.2f,.8f,.4f):new Color(.7f,.7f,.7f);
        GUILayout.Label(interpreter.Running?"● EJECUTANDO":"● LISTO",badge,GUILayout.Height(22)); GUI.color=Color.white;
        GUILayout.Label("Código",title);
        string previousCode = code;

        code = GUILayout.TextArea(
            code,
            codeStyle,
            GUILayout.ExpandWidth(true),
            GUILayout.Height(Mathf.Max(260, Screen.height * .37f))
        );

        if (code != previousCode)
        {
            PlayerPrefs.SetString(BotCodeKey, code);
        }

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.Width(r.width*.62f));
        GUILayout.Label("Salida",title);
        outputScroll=GUILayout.BeginScrollView(outputScroll,GUILayout.Height(Mathf.Max(125,Screen.height*.20f)));
        GUILayout.TextArea(GameManager.Instance.LastOutput,outputStyle,GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        GUILayout.Label("Variables",title);
        variableScroll=GUILayout.BeginScrollView(variableScroll,GUILayout.Height(Mathf.Max(125,Screen.height*.20f)));
        if(interpreter.Variables.Count==0) GUILayout.Label("Sin variables todavía.",variableStyle);
        else foreach(KeyValuePair<string,double> kv in interpreter.Variables) GUILayout.Label(kv.Key+" = "+kv.Value.ToString("0.##"),variableStyle);
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Label("Comandos y sensores",title);
        GUILayout.Label("mover_derecha() · mover_izquierda() · mover_arriba() · mover_abajo() · recoger() · capturar() · alimentar() · vender_orbes() · comprar_llave() · viajar() · imprimir(expresión)",label);
        GUILayout.Label("Sensores: x() y() · hay_comida() · hay_comida_aqui() · comida_x() comida_y() · hay_slime_salvaje() · slime_x() slime_y() · distancia_slime() · energia_slime() · comida() · orbes() · dinero() · llaves() · bioma() · corral_x() corral_y() · mercado_x() mercado_y() · puerta_x() puerta_y() · en_corral() en_mercado() en_puerta()",label);
        GUILayout.EndArea();
    }


    private void CreateTextures()
    {
        whiteTex = Solid(Color.white);

        corralTex = LoadIcon("Icons/corral",
            Box(64, new Color(.72f, .43f, .18f),
            new Color(.18f, .12f, .07f, .92f)));

        marketTex = LoadIcon("Icons/mercado", MarketIcon(64));
        gateTex = LoadIcon("Icons/puerta",
            DiamondIcon(64, new Color(.72f, .25f, 1f)));
        botTex = LoadIcon("Icons/bot", Bot(64));
        orbTex = LoadIcon("Icons/orbe",
            StarIcon(64, new Color(1f, .86f, .12f)));

        foodTex[0] = LoadIcon(
            "Icons/comida",
            FoodIcon(64, 0, new Color(1f, .45f, .05f)));

        foodTex[1] = LoadIcon(
            "Icons/comida1",
            FoodIcon(64, 1, new Color(.15f, .8f, 1f)));

        foodTex[2] = LoadIcon(
            "Icons/comida2",
            FoodIcon(64, 2, new Color(.95f, .2f, .15f)));

        slimeTex = new Texture2D[7];

        Texture2D slimeIcon = LoadIcon(
            "Icons/slime",
            Circle(64, new Color(.5f, 1f, .3f), true));

        for (int i = 0; i < slimeTex.Length; i++)
            slimeTex[i] = slimeIcon;

        backgroundTex[0] = Resources.Load<Texture2D>("Fondos/Fondo1");
        backgroundTex[1] = Resources.Load<Texture2D>("Fondos/Fondo2");
        backgroundTex[2] = Resources.Load<Texture2D>("Fondos/Fondo3");

        Debug.Log("Fondo1 cargado: " + (backgroundTex[0] != null));
        Debug.Log("Fondo2 cargado: " + (backgroundTex[1] != null));
        Debug.Log("Fondo3 cargado: " + (backgroundTex[2] != null));
    }

    private Texture2D LoadIcon(string path, Texture2D fallback)
    {
        Texture2D icon = Resources.Load<Texture2D>(path);

        if (icon == null)
        {
            Debug.LogWarning(
                "No se encontró el icono: Assets/Resources/" + path + ".png"
            );

            return fallback;
        }

        return icon;
    }

    private Texture2D FoodIcon(int size, int kind, Color color)
    {
        Texture2D t = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        float cx = size * .5f, cy = size * .48f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float nx = x / (float)size, ny = y / (float)size;
            bool inside = false;
            if (kind == 0)
            {
                // Manzana: cuerpo redondeado y hoja verde.
                inside = Vector2.Distance(new Vector2(x, y), new Vector2(cx - size * .12f, cy)) < size * .30f
                      || Vector2.Distance(new Vector2(x, y), new Vector2(cx + size * .12f, cy)) < size * .30f;
                if (nx > .48f && nx < .68f && ny > .73f && ny < .88f) inside = true;
                if (nx > .60f && nx < .83f && ny > .78f && ny < .91f
                    && Mathf.Abs((ny - .78f) / .13f - (nx - .60f) / .23f) < .24f) pixels[y * size + x] = new Color(.25f, .9f, .2f);
            }
            else if (kind == 1)
            {
                // Hexágono: segunda variedad de alimento (la puerta usa un diamante).
                float dx = Mathf.Abs(nx - .5f);
                float dy = Mathf.Abs(ny - .5f);
                inside = dy <= .34f && dx <= .42f && dx + dy * .45f <= .48f;
            }
            else
            {
                // Triángulo: tercera variedad de alimento.
                inside = ny >= .16f && ny <= .86f &&
                    nx >= .5f - (ny - .16f) * .58f &&
                    nx <= .5f + (ny - .16f) * .58f;
            }
            if (inside && pixels[y * size + x].a == 0) pixels[y * size + x] = color;
        }
        t.SetPixels(pixels);
        t.Apply();
        return t;
    }

    private Texture2D StarIcon(int size, Color color)
    {
        Texture2D t = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(size * .5f, size * .5f);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            Vector2 d = new Vector2(x, y) - center;
            float angle = Mathf.Atan2(d.y, d.x);
            float radius = d.magnitude;
            float boundary = size * (.34f + .13f * Mathf.Cos(angle * 5f));
            if (radius <= boundary) pixels[y * size + x] = color;
        }
        t.SetPixels(pixels);
        t.Apply();
        return t;
    }

    private Texture2D DiamondIcon(int size, Color color)
    {
        Texture2D t = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = Mathf.Abs(x - size * .5f) / (size * .42f);
            float dy = Mathf.Abs(y - size * .5f) / (size * .42f);
            if (dx + dy <= 1f) pixels[y * size + x] = color;
        }
        t.SetPixels(pixels);
        t.Apply();
        return t;
    }

    private Texture2D MarketIcon(int size)
    {
        // Mercado: moneda circular dorada con un símbolo $ pixelado.
        Texture2D t = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        Color border = new Color(.45f, .22f, .02f);
        Color gold = new Color(1f, .72f, .08f);
        Color symbol = new Color(.38f, .18f, .02f);
        float cx = size * .5f, cy = size * .5f, radius = size * .43f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
            if (d <= radius) pixels[y * size + x] = d >= radius * .82f ? border : gold;
        }

        // Símbolo $ simplificado para conservar legibilidad en tamaños pequeños.
        for (int y = (int)(size * .25f); y < (int)(size * .75f); y++)
        {
            int x = size / 2;
            pixels[y * size + x] = symbol;
        }
        for (int x = (int)(size * .34f); x < (int)(size * .66f); x++)
        {
            pixels[(int)(size * .67f) * size + x] = symbol;
            pixels[(int)(size * .50f) * size + x] = symbol;
            pixels[(int)(size * .33f) * size + x] = symbol;
        }
        t.SetPixels(pixels);
        t.Apply();
        return t;
    }

    private Texture2D Solid(Color c){var t=new Texture2D(1,1);t.SetPixel(0,0,c);t.Apply();return t;}
    private Texture2D Box(int s,Color border,Color fill){var t=new Texture2D(s,s);var a=new Color[s*s];for(int y=0;y<s;y++)for(int x=0;x<s;x++)a[y*s+x]=(x<4||x>=s-4||y<4||y>=s-4)?border:fill;t.SetPixels(a);t.Apply();return t;}
    private Texture2D Bot(int s){var t=new Texture2D(s,s);var a=new Color[s*s];for(int y=0;y<s;y++)for(int x=0;x<s;x++){bool b=x<3||x>=s-3||y<3||y>=s-3;bool v=y>s*.43f&&y<s*.68f&&x>s*.18f&&x<s*.82f;a[y*s+x]=v?new Color(.1f,.9f,1):b?new Color(.08f,.12f,.16f):new Color(.3f,.35f,.4f);}t.SetPixels(a);t.Apply();return t;}
    private Texture2D Circle(int s,Color c,bool eyes){var t=new Texture2D(s,s);var a=new Color[s*s];float cx=s*.5f,cy=s*.5f,r=s*.42f;for(int y=0;y<s;y++)for(int x=0;x<s;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(cx,cy));Color q=Color.clear;if(d<=r){q=c;if(eyes){float ex=r*.32f,ey=cy+r*.08f;if(Vector2.Distance(new Vector2(x,y),new Vector2(cx-ex,ey))<r*.18f||Vector2.Distance(new Vector2(x,y),new Vector2(cx+ex,ey))<r*.18f)q=Color.black;}}a[y*s+x]=q;}t.SetPixels(a);t.Apply();return t;}

    private string DefaultProgram()=>@"# Primer reto: recoge la comida más cercana\nfuncion ir_a_comida():\n    mientras hay_comida():\n        objetivo_x = comida_x()\n        objetivo_y = comida_y()\n        mientras x() < objetivo_x:\n            mover_derecha()\n        fin\n        mientras x() > objetivo_x:\n            mover_izquierda()\n        fin\n        mientras y() < objetivo_y:\n            mover_arriba()\n        fin\n        mientras y() > objetivo_y:\n            mover_abajo()\n        fin\n        recoger()\n    fin\nfin\nir_a_comida()";

    private string BasicTutorial()=>@"# TUTORIAL BÁSICO\nmover_derecha()\nmover_arriba()\nmover_arriba()\nrecoger()\n";
    private string AdvancedTutorial()=>@"# TUTORIAL COMPLETO\nfuncion ir_a_comida():\n    mientras hay_comida():\n        objetivo_x = comida_x()\n        objetivo_y = comida_y()\n        mientras x() < objetivo_x:\n            mover_derecha()\n        fin\n        mientras x() > objetivo_x:\n            mover_izquierda()\n        fin\n        mientras y() < objetivo_y:\n            mover_arriba()\n        fin\n        mientras y() > objetivo_y:\n            mover_abajo()\n        fin\n        recoger()\n    fin\nfin\n\nfuncion ir_al_slime():\n    mientras hay_slime_salvaje():\n        sx = slime_x()\n        sy = slime_y()\n        mientras x() < sx:\n            mover_derecha()\n        fin\n        mientras x() > sx:\n            mover_izquierda()\n        fin\n        mientras y() < sy:\n            mover_arriba()\n        fin\n        mientras y() > sy:\n            mover_abajo()\n        fin\n        capturar()\n    fin\nfin\n\nir_a_comida()\nir_al_slime()\n\n# Después de capturar, lleva el bot al corral y alimenta.\nmientras x() < corral_x():\n    mover_derecha()\nfin\nmientras x() > corral_x():\n    mover_izquierda()\nfin\nmientras y() < corral_y():\n    mover_arriba()\nfin\nmientras y() > corral_y():\n    mover_abajo()\nfin\nsi energia_slime() < 100:\n    alimentar()\nsi_no:\n    imprimir(energia_slime())\nfin";
}
