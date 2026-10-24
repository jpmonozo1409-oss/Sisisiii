# GUÍA COMPLETA — CREAR LUDUS DE ORBES DESDE CERO EN UNITY

## 1. Qué vamos a construir

La versión completa conserva la idea del demo: un tablero de 10×7 en el que un robot recibe instrucciones escritas por el estudiante. El jugador aprende programación resolviendo objetivos dentro de un pequeño rancho.

El ciclo principal es:

1. El bot empieza en `(1,5)`.
2. Busca comida mediante sensores.
3. Recoge los alimentos.
4. Localiza al slime salvaje.
5. Se mueve hasta el slime y lo captura.
6. Regresa al corral.
7. Alimenta al slime.
8. Cuando la energía llega a 100%, produce un orbe.
9. Lleva los orbes al mercado.
10. Compra una llave.
11. Viaja por la puerta al siguiente bioma.
12. Repite el aprendizaje en hasta 7 biomas.

---

# 2. Versión de Unity recomendada

Usar exactamente:

`Unity 2022.3.62f1 LTS`

El demo original también está basado en esa versión.

No es obligatorio instalar paquetes externos. El proyecto utiliza C# y las herramientas de interfaz incluidas en Unity.

---

# 3. Crear el proyecto manualmente desde cero

Si quieres entender cómo reconstruirlo sin copiar el proyecto terminado:

### Paso 1 — Unity Hub

1. Abrir Unity Hub.
2. Seleccionar **New project**.
3. Elegir plantilla **2D Core**.
4. Nombre: `LudusOrbesCompleto`.
5. Crear el proyecto.

### Paso 2 — Estructura de carpetas

En la ventana Project crear exactamente:

```text
Assets/
├── Scenes/
├── Scripts/
├── Resources/
└── Editor/
```

Las rutas importantes serán:

```text
Assets/Scripts/GameData.cs
Assets/Scripts/GameManager.cs
Assets/Scripts/BotInterpreter.cs
Assets/Scripts/GameUI.cs
Assets/Scripts/Bootstrap.cs
Assets/Scenes/Main.unity
```

---

# 4. Crear la escena

En Unity:

1. `File > New Scene`.
2. Guardar como:

```text
Assets/Scenes/Main.unity
```

3. Crear un GameObject vacío.
4. Nombre:

```text
Bootstrap
```

5. Agregar el script:

```text
Assets/Scripts/Bootstrap.cs
```

No necesitas crear manualmente la cámara, Canvas, botones ni el editor de código. Esta versión los genera por código mediante la interfaz IMGUI.

---

# 5. GameData.cs

Ubicación exacta:

```text
Assets/Scripts/GameData.cs
```

Responsabilidad:

- Guardar la partida.
- Representar los biomas.
- Representar el slime.
- Definir tamaño del mapa.
- Definir posiciones de corral, mercado y puerta.
- Definir alimentos.
- Definir colores de los biomas.

La clase principal es:

```csharp
[Serializable]
public class GameSaveData
```

Contiene:

```text
biome
money
keys
foods
orbs
biomes
```

Cada bioma contiene:

```text
unlocked
foodTaken
wild
corral
```

---

# 6. GameManager.cs

Ubicación:

```text
Assets/Scripts/GameManager.cs
```

Es el corazón de las reglas del juego.

Controla:

- posición del bot;
- bioma actual;
- comida;
- orbes;
- dinero;
- llaves;
- captura;
- alimentación;
- mercado;
- cambio de bioma;
- guardado.

## Posición inicial

```csharp
BotPosition = GameConstants.StartBot;
```

Y `StartBot` está definido como:

```csharp
new Vector2Int(1, 5)
```

---

# 7. Tamaño del mapa

En:

```text
Assets/Scripts/GameData.cs
```

está:

```csharp
public const int Width = 10;
public const int Height = 7;
```

Esto produce:

```text
X: 0 1 2 3 4 5 6 7 8 9
Y: 0 1 2 3 4 5 6
```

El bot no puede salir de estos límites.

---

# 8. Ubicación de los objetos del mundo

En `GameData.cs`:

### Bot

```text
(1,5)
```

### Corral

```text
(7,4)
```

### Mercado

```text
(9,1)
```

### Puerta

```text
(1,0)
```

### Alimentos

```text
(2,2)
(5,5)
(8,3)
```

### Slime salvaje

```text
(4,3)
```

---

# 9. BotInterpreter.cs

Ubicación:

```text
Assets/Scripts/BotInterpreter.cs
```

Esta es la diferencia principal frente al demo original.

El demo mostraba el editor y los ejemplos, pero el botón de ejecutar no tenía un intérprete completo. Aquí sí existe un intérprete funcional.

El intérprete transforma el texto del estudiante en acciones.

---

# 10. Lenguaje que entiende el juego

## Movimiento

```text
mover_derecha()
mover_izquierda()
mover_arriba()
mover_abajo()
```

## Acciones

```text
recoger()
capturar()
alimentar()
vender_orbes()
comprar_llave()
viajar()
```

## Imprimir

```text
imprimir(x())
imprimir(energia_slime())
imprimir(orbes())
```

## Variables

```text
objetivo_x = comida_x()
objetivo_y = comida_y()
```

## Condicional

```text
si energia_slime() < 100:
    alimentar()
fin
```

## Si / si no

```text
si energia_slime() < 100:
    alimentar()
si_no:
    imprimir(energia_slime())
fin
```

## Mientras

```text
mientras x() < comida_x():
    mover_derecha()
fin
```

## Repetir

```text
repetir 3:
    mover_derecha()
fin
```

## Funciones

```text
funcion ir_a_comida():
    mover_derecha()
    mover_arriba()
fin

ir_a_comida()
```

---

# 11. Operadores

El intérprete soporta:

```text
+
-
*
/
<
>
<=
>=
==
!=
```

Y operadores lógicos sencillos:

```text
y
o
no
```

Ejemplo:

```text
si energia_slime() < 100 y comida() > 0:
    alimentar()
fin
```

---

# 12. Sensores

El estudiante puede consultar:

```text
x()
y()

hay_comida()
hay_comida_aqui()
comida_x()
comida_y()

hay_slime_salvaje()
slime_x()
slime_y()
distancia_slime()
slime_en_corral()
energia_slime()

comida()
orbes()
dinero()
llaves()
bioma()

corral_x()
corral_y()
mercado_x()
mercado_y()
puerta_x()
puerta_y()

en_corral()
en_mercado()
en_puerta()
```

Todos devuelven valores que el programa puede utilizar.

---

# 13. Cómo se ejecuta un programa

Cuando el estudiante pulsa **EJECUTAR**:

```text
GameUI
   ↓
BotInterpreter.Run()
   ↓
CleanLines()
   ↓
ParseProgram()
   ↓
ParseBlock()
   ↓
ExecuteBlock()
   ↓
GameManager
```

Ejemplo:

```text
mover_derecha()
```

se transforma en:

```csharp
GameManager.Instance.MoveBot(Vector2Int.right)
```

---

# 14. Seguridad del intérprete

Hay dos límites importantes.

### Límite general

```text
2000 operaciones
```

### Límite por while

```text
500 iteraciones
```

Esto evita que un estudiante escriba accidentalmente:

```text
mientras 1 == 1:
    mover_derecha()
fin
```

y bloquee el juego.

---

# 15. GameUI.cs

Ubicación:

```text
Assets/Scripts/GameUI.cs
```

Es responsable de mostrar:

- tablero;
- bot;
- slime;
- alimentos;
- corral;
- mercado;
- puerta;
- editor de código;
- salida;
- variables;
- sensores;
- estado de ejecución.

No requiere prefabs.

---

# 16. Texturas procedurales

El juego genera por código:

```text
bot
slime
alimentos
corral
mercado
puerta
```

Por eso no tienes que descargar imágenes para que la primera versión funcione.

Las funciones están dentro de:

```text
Assets/Scripts/GameUI.cs
```

Ejemplos:

```csharp
Solid()
Box()
Bot()
Circle()
```

---

# 17. Biomas

El juego tiene 7 biomas:

1. Arrecife Seco
2. Cantera Índigo
3. Manta de Musgo
4. Desierto Cristal
5. Bosque Neón
6. Volcán Violeta
7. Cúpula Celeste

Cada bioma cambia el color del cielo, suelo y slime.

---

# 18. Sistema de progresión

Para avanzar:

```text
comida
   ↓
slime
   ↓
captura
   ↓
corral
   ↓
alimentación
   ↓
orbe
   ↓
mercado
   ↓
llave
   ↓
puerta
   ↓
bioma siguiente
```

El costo de la llave aumenta según el bioma:

```text
costo = 4 + bioma
```

---

# 19. Guardado

La partida utiliza:

```text
PlayerPrefs
```

y serialización JSON mediante:

```csharp
JsonUtility.ToJson(Save)
```

La clave es:

```text
LudusOrbes_Save_v1
```

Esto permite cerrar y abrir el juego sin perder el progreso.

---

# 20. Crear un programa básico de prueba

Pegar en el editor:

```text
mover_derecha()
mover_derecha()
mover_derecha()
mover_derecha()
recoger()
```

El bot inicia en `(1,5)`.

El alimento ubicado en `(5,5)` queda justo cuatro movimientos a la derecha.

Al ejecutar, el bot debe llegar y recogerlo.

---

# 21. Segundo ejercicio

```text
objetivo_x = comida_x()
objetivo_y = comida_y()

mientras x() < objetivo_x:
    mover_derecha()
fin

mientras x() > objetivo_x:
    mover_izquierda()
fin

mientras y() < objetivo_y:
    mover_arriba()
fin

mientras y() > objetivo_y:
    mover_abajo()
fin

recoger()
```

Aquí el estudiante empieza a trabajar con:

- variables;
- sensores;
- ciclos;
- comparación;
- movimiento automático.

---

# 22. Tercer ejercicio: capturar al slime

```text
sx = slime_x()
sy = slime_y()

mientras x() < sx:
    mover_derecha()
fin

mientras x() > sx:
    mover_izquierda()
fin

mientras y() < sy:
    mover_arriba()
fin

mientras y() > sy:
    mover_abajo()
fin

capturar()
```

---

# 23. Cuarto ejercicio: alimentar

```text
si energia_slime() < 100 y comida() > 0:
    alimentar()
si_no:
    imprimir(energia_slime())
fin
```

---

# 24. Quinto ejercicio: repetir

```text
repetir 4:
    mover_derecha()
fin
```

Es ideal para explicar el concepto de ciclo antes de introducir `mientras`.

---

# 25. Sexto ejercicio: función

```text
funcion avanzar():
    mover_derecha()
    mover_derecha()
fin

avanzar()
avanzar()
```

Explicar que una función permite reutilizar instrucciones.

---

# 26. Prueba del sistema de sensores

Ejecutar:

```text
imprimir(x())
imprimir(y())
imprimir(comida())
imprimir(orbes())
imprimir(dinero())
imprimir(llaves())
imprimir(bioma())
imprimir(energia_slime())
```

La información aparece en el panel **Salida**.

---

# 27. Qué ocurre internamente cuando el bot se mueve

1. El intérprete identifica `mover_derecha()`.
2. Llama a `GameManager.MoveBot(Vector2Int.right)`.
3. Se comprueban los límites del tablero.
4. Se anima el movimiento.
5. Se actualiza `BotPosition`.
6. `GameUI` vuelve a dibujar el tablero.

Esto separa la lógica del juego de la presentación.

---

# 28. Qué ocurre al recoger comida

`recoger()` llama a:

```text
GameManager.PickupFood()
```

La función verifica:

1. posición del bot;
2. posición de cada alimento;
3. si ya fue recogido;
4. actualiza inventario;
5. guarda la partida;
6. muestra el mensaje.

---

# 29. Qué ocurre al capturar

`capturar()` verifica:

```text
¿hay slime?
¿hay slime en el corral?
¿bot y slime están en la misma celda?
```

Si todo es correcto:

```text
wild → corral
```

---

# 30. Qué ocurre al generar un orbe

Cada alimento aumenta la energía del slime.

Cuando:

```text
energia >= 100
```

se crea:

```text
orbes[bioma]++
```

Después la energía baja ligeramente para que sea necesario volver a alimentar.

---

# 31. Cómo crear el proyecto usando los archivos terminados

1. Crear una carpeta llamada:

```text
LudusOrbesCompleto
```

2. Copiar dentro las carpetas:

```text
Assets
Packages
ProjectSettings
```

3. Abrir esa carpeta desde Unity Hub.
4. Esperar a que Unity importe los scripts.
5. Abrir:

```text
Assets/Scenes/Main.unity
```

6. Pulsar Play.

---

# 32. Si Unity muestra errores de compilación

Revisar primero:

```text
Window > General > Console
```

Los cinco scripts que deben compilar son:

```text
GameData.cs
GameManager.cs
BotInterpreter.cs
GameUI.cs
Bootstrap.cs
```

Si aparece un error de namespace o versión, verificar que el proyecto esté usando Unity 2022.3.62f1.

---

# 33. Cómo agregar un nuevo comando

Ejemplo: crear:

```text
saludar()
```

En:

```text
Assets/Scripts/BotInterpreter.cs
```

buscar:

```csharp
private IEnumerator ExecuteCommand(string line)
```

Agregar:

```csharp
case "saludar()":
    GameManager.Instance.Log("Hola programador.");
    break;
```

Luego el estudiante podrá escribir:

```text
saludar()
```

---

# 34. Cómo agregar un nuevo sensor

En `BotInterpreter.cs`, dentro de `GetValue()` agregar una condición.

Ejemplo:

```csharp
if (n == "hay_llave") return gm.Save.keys > 0 ? 1 : 0;
```

Después puede utilizarse:

```text
si hay_llave() == 1:
    viajar()
fin
```

Para que esto funcione como llamada, también puede integrarse como sensor con la misma convención utilizada por los sensores existentes.

---

# 35. Cómo agregar un nuevo bioma

En `GameData.cs`:

1. Añadir el nombre a `Biomes`.
2. Añadir un color a `Sky`.
3. Añadir un color a `Ground`.
4. Aumentar el tamaño de `orbs` y `biomes` de 7 al nuevo número.
5. Ajustar los bucles que recorren los biomas si se desea una cantidad distinta.

---

# 36. Cómo mejorar el juego después

Una segunda versión puede agregar:

- obstáculos;
- paredes;
- enemigos;
- caminos bloqueados;
- niveles con dificultad;
- misiones;
- sistema de experiencia;
- tienda de mejoras;
- nuevos tipos de slime;
- nuevos sensores;
- sonidos;
- música;
- animaciones reales;
- sprites artísticos;
- partículas;
- ranking de estudiantes;
- editor con resaltado de sintaxis;
- errores de programación señalados por línea;
- sistema de pistas;
- modo examen;
- exportación de código.

---

# 37. Arquitectura final

```text
                    ┌─────────────────────┐
                    │      GameUI         │
                    │ tablero + editor    │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │   BotInterpreter    │
                    │ parser + ejecución  │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │    GameManager      │
                    │ reglas del juego    │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │     GameData        │
                    │ estado/progreso     │
                    └─────────────────────┘
```

Esta separación es intencional: permite enseñar programación, pero también mantiene el proyecto organizado para que posteriormente pueda convertirse en un juego comercial o educativo más grande.
