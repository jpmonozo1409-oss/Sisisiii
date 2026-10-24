
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfiguracionUI : MonoBehaviour
{
    private Texture2D fondo;
    private Texture2D btnIdioma;
    private Texture2D btnSonido;
    private Texture2D btnControles;
    private Texture2D btnPantalla;
    private Texture2D btnVolver;

    private GUIStyle botonInvisible;
    private bool mostrarControles = false;
    private bool mostrarIdiomas = false;
    private string idiomaSeleccionado = "Español";

    private void Start()
    {
        // Cargar el fondo desde Assets/Resources
        fondo = Resources.Load<Texture2D>("Configuracion");

        // Cargar los cuatro botones desde Assets/Resources/Icons
        btnIdioma = Resources.Load<Texture2D>("Icons/BtnIdioma");
        btnSonido = Resources.Load<Texture2D>("Icons/BtnSonido");
        btnControles = Resources.Load<Texture2D>("Icons/BtnControles");
        btnPantalla = Resources.Load<Texture2D>("Icons/BtnPantalla");
        btnVolver = Resources.Load<Texture2D>("Icons/BtnVolver");

        // Estilo sin texto ni fondo para usar las imágenes originales
        botonInvisible = new GUIStyle();
        botonInvisible.normal.background = null;
        botonInvisible.hover.background = null;
        botonInvisible.active.background = null;
        botonInvisible.focused.background = null;
    }

    private void OnGUI()
    {
        GUI.depth = 0;

        // Dibujar el fondo
        if (fondo != null)
        {
            GUI.DrawTexture(
                new Rect(0, 0, Screen.width, Screen.height),
                fondo,
                ScaleMode.StretchToFill
            );
        }
        else
        {
            GUI.Label(
                new Rect(20, 20, 500, 40),
                "No se encontró Configuracion.jpeg en Resources."
            );
        }

        if (mostrarControles)
        {
            DibujarVentanaControles();
            return;
        }

        if (mostrarIdiomas)
        {
            DibujarVentanaIdiomas();
            return;
        }

        // Tamaño y posición de los botones

        float ancho = Mathf.Min(680f, Screen.width * 0.82f);
        float alto = ancho * 0.24f;
        float espacio = -30f;

        float x = (Screen.width - ancho) / 2f;

        // Distribuir los cinco botones y centrar el conjunto
        float altoTotal = alto * 5f + espacio * 4f;
        float y = (Screen.height - altoTotal) / 2f + 150f;


        DibujarBoton(btnIdioma, "IDIOMA", x, y, ancho, alto);
        DibujarBoton(btnSonido, "SONIDO", x, y + alto + espacio, ancho, alto);
        DibujarBoton(btnControles, "CONTROLES", x, y + (alto + espacio) * 2, ancho, alto);
        DibujarBoton(btnPantalla, "PANTALLA", x, y + (alto + espacio) * 3, ancho, alto);
        DibujarBoton(btnVolver, "VOLVER", x, y + (alto + espacio) * 4, ancho, alto);


    }

    private void DibujarBoton(
        Texture2D imagen,
        string nombre,
        float x,
        float y,
        float ancho,
        float alto)
    {
        Rect area = new Rect(x, y, ancho, alto);

        // Mostrar la imagen original
        if (imagen != null)
        {
            GUI.DrawTexture(area, imagen, ScaleMode.StretchToFill, true);
        }
        else
        {
            GUI.Label(area, "Falta imagen: " + nombre);
        }

        // Detectar el clic sin dibujar otro botón encima

        if (GUI.Button(area, GUIContent.none, botonInvisible))
        {
            if (nombre == "SONIDO")
            {
                AudioListener.pause = !AudioListener.pause;

                Debug.Log(
                    AudioListener.pause
                        ? "Sonido desactivado."
                        : "Sonido activado."
                );
            }

            else if (nombre == "CONTROLES")
            {
                mostrarControles = true;
            }
            else if (nombre == "PANTALLA")
            {
                Screen.fullScreen = !Screen.fullScreen;

                Debug.Log(
                    Screen.fullScreen
                        ? "Pantalla completa activada."
                        : "Modo ventana activado."
                );
            }
            else if (nombre == "IDIOMA")
            {
                mostrarIdiomas = true;
            }
            else if (nombre == "VOLVER")
            {
                SceneManager.LoadScene("MenuPrincipal");
            }
            else
            {
                Debug.Log("Seleccionaste: " + nombre);
            }

        }

    }


    private void DibujarVentanaControles()
    {
        float anchoVentana = Mathf.Min(650f, Screen.width * 0.85f);
        float altoVentana = Mathf.Min(440f, Screen.height * 0.85f);

        float xVentana = (Screen.width - anchoVentana) / 2f;
        float yVentana = (Screen.height - altoVentana) / 2f;

        Rect ventana = new Rect(
            xVentana, yVentana, anchoVentana, altoVentana
        );

        GUI.color = new Color(0.08f, 0.10f, 0.16f, 1f);
        GUI.DrawTexture(ventana, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle estiloTitulo = new GUIStyle(GUI.skin.label);
        estiloTitulo.fontSize = 24;
        estiloTitulo.fontStyle = FontStyle.Bold;
        estiloTitulo.alignment = TextAnchor.MiddleCenter;
        estiloTitulo.normal.textColor = Color.yellow;

        GUI.Label(
            new Rect(xVentana + 15, yVentana + 15,
                     anchoVentana - 30, 45),
            "CONTROLES DE LUDUS ORBES",
            estiloTitulo
        );

        GUIStyle estiloTexto = new GUIStyle(GUI.skin.label);
        estiloTexto.fontSize = Mathf.Clamp(Screen.height / 40, 14, 22);
        estiloTexto.normal.textColor = Color.white;
        estiloTexto.wordWrap = true;

        GUI.Label(
            new Rect(xVentana + 30, yVentana + 75,
                     anchoVentana - 60, altoVentana - 145),
            "1. Mueve el bot por el tablero.\n\n" +
            "2. Recoge alimentos para obtener energia.\n\n" +
            "3. Acercate a los slimes salvajes para capturarlos.\n\n" +
            "4. Alimenta a tus slimes para generar orbes.\n\n" +
            "5. Vende los orbes para conseguir monedas.",
            estiloTexto
        );

        Rect areaCerrar = new Rect(
            xVentana + anchoVentana / 2f - 80,
            yVentana + altoVentana - 55,
            160,
            38
        );

        if (GUI.Button(areaCerrar, "CERRAR"))
        {
            mostrarControles = false;
        }
    }


    private void DibujarVentanaIdiomas()
    {
        float ancho = Mathf.Min(500f, Screen.width * 0.85f);
        float alto = 300f;

        float x = (Screen.width - ancho) / 2f;
        float y = (Screen.height - alto) / 2f;

        Rect ventana = new Rect(x, y, ancho, alto);

        GUI.color = new Color(0.08f, 0.10f, 0.16f, 1f);
        GUI.DrawTexture(ventana, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle titulo = new GUIStyle(GUI.skin.label);
        titulo.fontSize = 24;
        titulo.fontStyle = FontStyle.Bold;
        titulo.alignment = TextAnchor.MiddleCenter;
        titulo.normal.textColor = Color.yellow;

        GUI.Label(
            new Rect(x + 15, y + 15, ancho - 30, 45),
            "SELECCIONAR IDIOMA",
            titulo
        );

        GUIStyle boton = new GUIStyle(GUI.skin.button);
        boton.fontSize = 18;
        boton.fontStyle = FontStyle.Bold;

        if (GUI.Button(
            new Rect(x + 60, y + 85, ancho - 120, 45),
            "ESPAÑOL",
            boton))
        {
            idiomaSeleccionado = "Español";
            Debug.Log("Idioma seleccionado: " + idiomaSeleccionado);
        }

        if (GUI.Button(
            new Rect(x + 60, y + 145, ancho - 120, 45),
            "ENGLISH",
            boton))
        {
            idiomaSeleccionado = "English";
            Debug.Log("Selected language: " + idiomaSeleccionado);
        }

        if (GUI.Button(
            new Rect(x + 150, y + 225, ancho - 300, 40),
            "CERRAR",
            boton))
        {
            mostrarIdiomas = false;
        }
    }



}
