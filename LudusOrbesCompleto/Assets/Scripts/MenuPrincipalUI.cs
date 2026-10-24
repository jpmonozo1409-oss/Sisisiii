
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipalUI : MonoBehaviour
{
    public enum ModoInicio
    {
        Jugar,
        CargarPartida,
        NuevoJuego
    }

    public static ModoInicio ModoSolicitado = ModoInicio.Jugar;
    public static bool HaySolicitud = false;

    private void AbrirTablero(ModoInicio modo)
    {
        ModoSolicitado = modo;
        HaySolicitud = true;

        SceneManager.LoadScene("Main");
    }

    public void Jugar()
    {
        AbrirTablero(ModoInicio.Jugar);
    }

    public void CargarPartida()
    {
        AbrirTablero(ModoInicio.CargarPartida);
    }

    public void NuevoJuego()
    {
        AbrirTablero(ModoInicio.NuevoJuego);
    }

    public void Configuracion()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Configuracion");
    }

    public void Salir()
    {
        Application.Quit();
    }
}
