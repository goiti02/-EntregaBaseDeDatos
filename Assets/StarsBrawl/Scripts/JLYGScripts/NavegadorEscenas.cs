using UnityEngine;
using UnityEngine.SceneManagement;


public class NavegadorEscenas : MonoBehaviour
{
    public void IrA(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }
    public void GoBack()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void UserDetailsScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("UserDetails");
    }

    public void MatchDetailsScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MatchDetails");
    }

}