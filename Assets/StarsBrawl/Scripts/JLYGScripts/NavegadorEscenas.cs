using UnityEngine;
using UnityEngine.SceneManagement;

public class NavegadorEscenas : MonoBehaviour
{
    public void IrA(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }
}