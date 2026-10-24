using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class BotInterpreter : MonoBehaviour
{
    private enum NodeType { Command, Assignment, If, While, Repeat, FunctionCall }
    private class Node
    {
        public NodeType type;
        public string text;
        public string expression;
        public string name;
        public List<Node> body = new List<Node>();
        public List<Node> elseBody = new List<Node>();
    }

    private readonly Dictionary<string, List<Node>> functions = new Dictionary<string, List<Node>>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, double> variables = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
    private int safetyCounter;
    private const int MaxOperations = 2000;

    public IReadOnlyDictionary<string, double> Variables => variables;
    public bool Running { get; private set; }

    public void Run(string source)
    {
        if (Running) return;
        StartCoroutine(RunRoutine(source));
    }

    public void Stop()
    {
        StopAllCoroutines();
        Running = false;
        if (GameManager.Instance != null) GameManager.Instance.IsBusy = false;
    }

    private IEnumerator RunRoutine(string source)
    {
        Running = true;
        variables.Clear();
        functions.Clear();
        safetyCounter = 0;
        GameManager.Instance.Log("--- Ejecutando programa ---");

        List<string> lines = CleanLines(source);
        List<Node> main = ParseProgram(lines);
        yield return ExecuteBlock(main, 0);

        if (Running) GameManager.Instance.Log("--- Programa terminado ---");
        Running = false;
    }

    private List<string> CleanLines(string source)
    {
        var result = new List<string>();
        string[] raw = source.Replace("\r", "").Split('\n');
        foreach (string original in raw)
        {
            string line = original.Trim();
            if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) continue;
            result.Add(line);
        }
        return result;
    }

    private List<Node> ParseProgram(List<string> lines)
    {
        int index = 0;
        return ParseBlock(lines, ref index, false, false);
    }

    private List<Node> ParseBlock(List<string> lines, ref int index, bool stopAtFin, bool stopAtElse)
    {
        var result = new List<Node>();
        while (index < lines.Count)
        {
            string line = lines[index];
            if (line.Equals("fin", StringComparison.OrdinalIgnoreCase))
            {
                if (stopAtFin) { index++; return result; }
                index++; continue;
            }
            if (line.Equals("si_no:", StringComparison.OrdinalIgnoreCase) || line.Equals("si_no", StringComparison.OrdinalIgnoreCase))
            {
                if (stopAtElse) return result;
                index++; continue;
            }

            Match fn = Regex.Match(line, @"^funcion\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(\s*\)\s*:\s*$", RegexOptions.IgnoreCase);
            if (fn.Success)
            {
                index++;
                functions[fn.Groups[1].Value] = ParseBlock(lines, ref index, true, false);
                continue;
            }

            Match iff = Regex.Match(line, @"^si\s+(.+):$", RegexOptions.IgnoreCase);
            if (iff.Success)
            {
                index++;
                var node = new Node { type = NodeType.If, expression = iff.Groups[1].Value };
                node.body = ParseBlock(lines, ref index, true, true);
                if (index < lines.Count && (lines[index].Equals("si_no:", StringComparison.OrdinalIgnoreCase) || lines[index].Equals("si_no", StringComparison.OrdinalIgnoreCase)))
                {
                    index++;
                    node.elseBody = ParseBlock(lines, ref index, true, false);
                }
                result.Add(node);
                continue;
            }

            Match wh = Regex.Match(line, @"^mientras\s+(.+):$", RegexOptions.IgnoreCase);
            if (wh.Success)
            {
                index++;
                var node = new Node { type = NodeType.While, expression = wh.Groups[1].Value };
                node.body = ParseBlock(lines, ref index, true, false);
                result.Add(node);
                continue;
            }

            Match rep = Regex.Match(line, @"^repetir\s+(.+):$", RegexOptions.IgnoreCase);
            if (rep.Success)
            {
                index++;
                var node = new Node { type = NodeType.Repeat, expression = rep.Groups[1].Value };
                node.body = ParseBlock(lines, ref index, true, false);
                result.Add(node);
                continue;
            }

            Match assign = Regex.Match(line, @"^([A-Za-z_][A-Za-z0-9_]*)\s*=\s*(.+)$");
            if (assign.Success)
            {
                result.Add(new Node { type = NodeType.Assignment, name = assign.Groups[1].Value, expression = assign.Groups[2].Value });
                index++;
                continue;
            }

            Match call = Regex.Match(line, @"^([A-Za-z_][A-Za-z0-9_]*)\s*\(.*\)\s*$");
            if (call.Success)
            {
                result.Add(new Node { type = NodeType.FunctionCall, name = call.Groups[1].Value, text = line });
                index++;
                continue;
            }

            result.Add(new Node { type = NodeType.Command, text = line });
            index++;
        }
        return result;
    }

    private IEnumerator ExecuteBlock(List<Node> nodes, int depth)
    {
        if (depth > 40) { GameManager.Instance.Log("Error: demasiadas llamadas de función anidadas."); yield break; }
        foreach (Node node in nodes)
        {
            if (!Running) yield break;
            safetyCounter++;
            if (safetyCounter > MaxOperations)
            {
                GameManager.Instance.Log("Ejecución detenida por límite de seguridad (posible bucle infinito).");
                yield break;
            }

            switch (node.type)
            {
                case NodeType.Command:
                    yield return ExecuteCommand(node.text);
                    break;
                case NodeType.Assignment:
                    variables[node.name] = Evaluate(node.expression);
                    break;
                case NodeType.FunctionCall:
                    // Las funciones integradas del juego se ejecutan como comandos.
                    // Las funciones creadas por el usuario siguen pasando por ExecuteFunction.
                    if (IsBuiltinCommand(node.name))
                        yield return ExecuteCommand(node.text);
                    else
                        yield return ExecuteFunction(node.name, depth + 1);
                    break;
                case NodeType.If:
                    yield return ExecuteBlock(Evaluate(node.expression) != 0 ? node.body : node.elseBody, depth);
                    break;
                case NodeType.While:
                    int loops = 0;
                    while (Evaluate(node.expression) != 0 && Running)
                    {
                        loops++;
                        if (loops > 500) { GameManager.Instance.Log("Bucle detenido: máximo 500 iteraciones."); break; }
                        yield return ExecuteBlock(node.body, depth);
                        yield return null;
                    }
                    break;
                case NodeType.Repeat:
                    int count = Mathf.Clamp((int)Math.Round(Evaluate(node.expression)), 0, 500);
                    for (int i = 0; i < count && Running; i++) yield return ExecuteBlock(node.body, depth);
                    break;
            }
        }
    }

    private bool IsBuiltinCommand(string name)
    {
        switch (name.Trim().ToLowerInvariant())
        {
            case "mover_derecha":
            case "mover_izquierda":
            case "mover_arriba":
            case "mover_abajo":
            case "recoger":
            case "capturar":
            case "alimentar":
            case "vender_orbes":
            case "comprar_llave":
            case "viajar":
            case "reiniciar_comida":
            case "imprimir":
                return true;
            default:
                return false;
        }
    }

    private IEnumerator ExecuteFunction(string name, int depth)
    {
        if (!functions.ContainsKey(name))
        {
            GameManager.Instance.Log("Error: función no definida: " + name);
            yield break;
        }
        yield return ExecuteBlock(functions[name], depth);
    }

    private IEnumerator ExecuteCommand(string line)
    {
        string c = line.Trim().ToLowerInvariant();
        switch (c)
        {
            case "mover_derecha()": yield return GameManager.Instance.MoveBot(Vector2Int.right); break;
            case "mover_izquierda()": yield return GameManager.Instance.MoveBot(Vector2Int.left); break;
            case "mover_arriba()": yield return GameManager.Instance.MoveBot(Vector2Int.up); break;
            case "mover_abajo()": yield return GameManager.Instance.MoveBot(Vector2Int.down); break;
            case "recoger()": GameManager.Instance.PickupFood(); break;
            case "capturar()": GameManager.Instance.Capture(); break;
            case "alimentar()": GameManager.Instance.Feed(); break;
            case "vender_orbes()": GameManager.Instance.SellOrbs(); break;
            case "comprar_llave()": GameManager.Instance.BuyKey(); break;
            case "viajar()": GameManager.Instance.NextBiome(); break;
            case "reiniciar_comida()": GameManager.Instance.ResetCurrentBiomeFood(); break;
            default:
                if (c.StartsWith("imprimir(") && c.EndsWith(")"))
                {
                    string expr = line.Substring(line.IndexOf('(') + 1, line.LastIndexOf(')') - line.IndexOf('(') - 1);
                    GameManager.Instance.Log("Salida: " + Format(Evaluate(expr)));
                }
                else GameManager.Instance.Log("Comando no reconocido: " + line);
                break;
        }
        yield return new WaitForSeconds(.08f);
    }

    private double Evaluate(string expression)
    {
        expression = expression.Trim();
        if (expression.EndsWith(";")) expression = expression.Substring(0, expression.Length - 1);
        try
        {
            var parser = new ExpressionParser(expression, GetValue);
            return parser.Parse();
        }
        catch (Exception e)
        {
            GameManager.Instance.Log("Error en expresión '" + expression + "': " + e.Message);
            return 0;
        }
    }

    private double GetValue(string name)
    {
        if (variables.TryGetValue(name, out double value)) return value;
        string n = name.ToLowerInvariant();
        var gm = GameManager.Instance;
        if (n == "x") return gm.BotPosition.x;
        if (n == "y") return gm.BotPosition.y;
        if (n == "comida") return gm.Save.foods[gm.CurrentFoodIndex];
        if (n == "orbes") return gm.Save.orbs[gm.Save.biome];
        if (n == "dinero") return gm.Save.money;
        if (n == "llaves") return gm.Save.keys;
        if (n == "bioma") return gm.Save.biome + 1;
        if (n == "comida_x") return FindFood().x;
        if (n == "comida_y") return FindFood().y;
        if (n == "slime_x") return gm.CurrentBiome.wild != null ? gm.CurrentBiome.wild.x : -1;
        if (n == "slime_y") return gm.CurrentBiome.wild != null ? gm.CurrentBiome.wild.y : -1;
        if (n == "distancia_slime") return gm.DistanceToSlime();
        if (n == "energia_slime") return gm.SlimeEnergy();
        if (n == "corral_x") return GameConstants.CorralCell.x;
        if (n == "corral_y") return GameConstants.CorralCell.y;
        if (n == "mercado_x") return GameConstants.MarketCell.x;
        if (n == "mercado_y") return GameConstants.MarketCell.y;
        if (n == "puerta_x") return GameConstants.GateCell.x;
        if (n == "puerta_y") return GameConstants.GateCell.y;
        if (n == "hay_comida") return HasFood() ? 1 : 0;
        if (n == "hay_comida_aqui") return HasFoodHere() ? 1 : 0;
        if (n == "hay_slime_salvaje") return gm.CurrentBiome.wild != null ? 1 : 0;
        if (n == "slime_en_corral") return gm.CurrentBiome.corral != null ? 1 : 0;
        if (n == "en_corral") return gm.BotPosition == GameConstants.CorralCell ? 1 : 0;
        if (n == "en_mercado") return gm.BotPosition == GameConstants.MarketCell ? 1 : 0;
        if (n == "en_puerta") return gm.BotPosition == GameConstants.GateCell ? 1 : 0;
        throw new Exception("variable o sensor desconocido: " + name);
    }

    private bool HasFood()
    {
        var taken = GameManager.Instance.CurrentBiome.foodTaken;
        for (int i = 0; i < taken.Length; i++) if (!taken[i]) return true;
        return false;
    }

    private bool HasFoodHere()
    {
        for (int i = 0; i < GameConstants.FoodSpots.Length; i++)
            if (!GameManager.Instance.CurrentBiome.foodTaken[i] && GameManager.Instance.BotPosition == GameConstants.FoodSpots[i]) return true;
        return false;
    }

    private Vector2Int FindFood()
    {
        Vector2Int bot = GameManager.Instance.BotPosition;
        Vector2Int nearest = bot;
        float best = float.MaxValue;
        for (int i = 0; i < GameConstants.FoodSpots.Length; i++)
        {
            if (GameManager.Instance.CurrentBiome.foodTaken[i]) continue;
            float d = Vector2Int.Distance(bot, GameConstants.FoodSpots[i]);
            if (d < best) { best = d; nearest = GameConstants.FoodSpots[i]; }
        }
        return nearest;
    }

    private string Format(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    private class ExpressionParser
    {
        private readonly string s;
        private readonly Func<string, double> resolver;
        private int pos;
        public ExpressionParser(string text, Func<string, double> resolver) { s = text; this.resolver = resolver; }
        public double Parse() { double v = ParseOr(); Skip(); if (pos < s.Length) throw new Exception("token inesperado: " + s[pos]); return v; }
        private double ParseOr()
        {
            double v = ParseAnd();
            while (true) { Skip(); if (MatchWord("o")) v = (v != 0 || ParseAnd() != 0) ? 1 : 0; else break; }
            return v;
        }
        private double ParseAnd()
        {
            double v = ParseNot();
            while (true) { Skip(); if (MatchWord("y")) v = (v != 0 && ParseNot() != 0) ? 1 : 0; else break; }
            return v;
        }
        private double ParseNot()
        {
            Skip(); if (MatchWord("no") || MatchWord("not") || Match("!")) return ParseNot() == 0 ? 1 : 0;
            return ParseComparison();
        }
        private double ParseComparison()
        {
            double left = ParseAdd(); Skip();
            string op = ReadOperator();
            if (op == null) return left;
            double right = ParseAdd();
            switch (op) { case "<": return left < right ? 1 : 0; case ">": return left > right ? 1 : 0; case "<=": return left <= right ? 1 : 0; case ">=": return left >= right ? 1 : 0; case "==": return Math.Abs(left-right) < .0001 ? 1 : 0; case "!=": return Math.Abs(left-right) >= .0001 ? 1 : 0; }
            return 0;
        }
        private double ParseAdd()
        {
            double v = ParseMul();
            while (true) { Skip(); if (Match("+")) v += ParseMul(); else if (Match("-")) v -= ParseMul(); else break; }
            return v;
        }
        private double ParseMul()
        {
            double v = ParseUnary();
            while (true) { Skip(); if (Match("*")) v *= ParseUnary(); else if (Match("/")) { double d=ParseUnary(); v = Math.Abs(d)<.000001 ? 0 : v/d; } else break; }
            return v;
        }
        private double ParseUnary() { Skip(); if (Match("-")) return -ParseUnary(); return ParsePrimary(); }
        private double ParsePrimary()
        {
            Skip();
            if (Match("(")) { double v=ParseOr(); Expect(")"); return v; }
            if (pos < s.Length && (char.IsDigit(s[pos]) || s[pos]=='.')) return ReadNumber();
            string name = ReadName();
            if (string.IsNullOrEmpty(name)) throw new Exception("se esperaba número, variable o sensor");
            Skip();
            if (Match("(")) { Expect(")"); return resolver(name); }
            return resolver(name);
        }
        private double ReadNumber() { int start=pos; while(pos<s.Length && (char.IsDigit(s[pos]) || s[pos]=='.')) pos++; return double.Parse(s.Substring(start,pos-start), CultureInfo.InvariantCulture); }
        private string ReadName() { Skip(); int start=pos; while(pos<s.Length && (char.IsLetterOrDigit(s[pos]) || s[pos]=='_')) pos++; return s.Substring(start,pos-start); }
        private string ReadOperator() { foreach(string op in new[]{"<=",">=","==","!=","<",">"}) { if (Match(op)) return op; } return null; }
        private bool Match(string token) { Skip(); if (s.Substring(pos).StartsWith(token, StringComparison.Ordinal)) { pos += token.Length; return true; } return false; }
        private bool MatchWord(string word) { Skip(); int end = pos + word.Length; if (end <= s.Length && string.Compare(s,pos,word,0,word.Length,StringComparison.OrdinalIgnoreCase)==0 && (end==s.Length || !char.IsLetterOrDigit(s[end]) && s[end]!='_')) { pos=end; return true; } return false; }
        private void Expect(string token) { if (!Match(token)) throw new Exception("falta '"+token+"'"); }
        private void Skip() { while(pos<s.Length && char.IsWhiteSpace(s[pos])) pos++; }
    }
}
