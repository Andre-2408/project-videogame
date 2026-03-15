using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Cargar una escena de juego
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); // Cambia por el nombre de tu escena
    }

    // Salir del juego
    public void QuitGame()
    {
        Debug.Log("Salir del juego...");
        Application.Quit();
    }
}
