
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    private void Start()
    {
        // El menú no debe iniciar el tablero.
        if (SceneManager.GetActiveScene().name != "Main")
        {
            return;
        }

        // Crear el administrador si todavía no existe.
        if (GameManager.Instance == null)
        {
            GameObject manager = new GameObject("GameManager");
            manager.AddComponent<GameManager>();
        }

        GameManager gm = GameManager.Instance;

        // Ejecutar la acción solicitada desde el menú.
        if (MenuPrincipalUI.HaySolicitud)
        {
            switch (MenuPrincipalUI.ModoSolicitado)
            {
                case MenuPrincipalUI.ModoInicio.Jugar:
                    // Mantener la partida que está en memoria.
                    break;

                case MenuPrincipalUI.ModoInicio.CargarPartida:
                    if (!gm.LoadGame())
                    {
                        gm.NewGame();
                        gm.Log(
                            "No hay una partida guardada válida. " +
                            "Se inició un mundo nuevo."
                        );
                    }
                    break;

                case MenuPrincipalUI.ModoInicio.NuevoJuego:
                    gm.NewGame();
                    gm.SaveGame();
                    break;
            }

            MenuPrincipalUI.HaySolicitud = false;
        }
        else
        {
            // Permitir abrir Main directamente desde Unity.
            if (!gm.LoadGame())
            {
                gm.NewGame();
            }
        }

        // Crear la interfaz solo si no existe.
        if (FindObjectOfType<GameUI>() == null)
        {
            GameObject ui = new GameObject("GameUI");
            ui.AddComponent<GameUI>();
        }
    }
}
