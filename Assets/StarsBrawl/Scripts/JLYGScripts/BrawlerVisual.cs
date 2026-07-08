using UnityEngine;
using TMPro;

public class BrawlerVisual : MonoBehaviour
{
    public TextMeshProUGUI nombreUI;
    public TextMeshProUGUI nivelUI;
    public TextMeshProUGUI trofeosUI;

    public void Configurar(string nombre, int nivel, int trofeos)
    {
        if (nombreUI != null) nombreUI.text = nombre;
        if (nivelUI != null) nivelUI.text = "Nivel " + nivel;
        if (trofeosUI != null) trofeosUI.text = trofeos.ToString() + " trofeos";
    }
}