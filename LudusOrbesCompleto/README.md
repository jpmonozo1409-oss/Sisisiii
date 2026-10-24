# Ludus de Orbes — versión completa Unity

Proyecto educativo inspirado en el demo `Ludus limi-demo`.

## Versión
- Unity 2022.3.62f1 LTS
- C#
- UGUI/IMGUI nativo de Unity
- Sin assets externos obligatorios

## Qué incluye
- Tablero 10×7.
- Bot programable.
- Editor de código en español.
- Intérprete real para comandos, variables, funciones, if/else, mientras, repetir y expresiones.
- Sensores del mundo.
- Recolección de comida.
- Captura y alimentación de slime.
- Generación y venta de orbes.
- Compra de llaves.
- 7 biomas.
- Guardado con PlayerPrefs/JSON.
- Tutorial básico y avanzado.
- Texturas procedurales generadas por código.

## Abrir
1. Instalar Unity Hub.
2. Instalar Unity 2022.3.62f1.
3. Abrir Unity Hub > Add > seleccionar esta carpeta.
4. Abrir `Assets/Scenes/Main.unity`.
5. Pulsar Play.

## Archivos principales
- `Assets/Scripts/GameData.cs`: estructuras y constantes del mundo.
- `Assets/Scripts/GameManager.cs`: reglas del juego, progreso, inventario y guardado.
- `Assets/Scripts/BotInterpreter.cs`: parser e intérprete del lenguaje.
- `Assets/Scripts/GameUI.cs`: tablero, editor, salida y panel de variables.
- `Assets/Scripts/Bootstrap.cs`: arranque automático.
- `Assets/Scenes/Main.unity`: escena inicial.

## Flujo de juego
Recolectar comida → localizar slime → capturar → ir al corral → alimentar → producir orbes → ir al mercado → comprar llave → ir a la puerta → viajar al siguiente bioma.
