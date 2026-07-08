using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class TarjetaBrawler : MonoBehaviour
{
    public Image imagenRetrato;

    // ESTA ES LA FUNCIÓN QUE FALTA
    public void ConfigurarImagen(string nombreBrawler, List<Sprite> lista)
    {
        if (imagenRetrato != null)
        {
            string nombreLimpio = nombreBrawler.ToLower().Replace(" ", "").Replace("-", "");
            Sprite s = lista.Find(x => x.name.ToLower().Replace(" ", "").Replace("-", "").Contains(nombreLimpio));

            if (s != null)
            {
                imagenRetrato.sprite = s;
                imagenRetrato.color = Color.white;
            }
        }
    }
}

//using UnityEngine;
//using UnityEngine.UI;
//using TMPro; // Asegúrate de tener esto
//using System.Collections.Generic;

//public class TarjetaBrawler : MonoBehaviour
//{
//    // Arrastra estos componentes desde tu Prefab al Inspector
//    public Image imagenRetrato;
//    public TMP_Text textoNombre;
//    public TMP_Text textoNivel;
//    public TMP_Text textoCopas;

//    public void ActualizarDatos(string nombre, string nivel, string copas, List<Sprite> lista)
//    {
//        // 1. Actualizar Nombre
//        if (textoNombre != null) textoNombre.text = nombre;

//        // 2. Actualizar Nivel
//        if (textoNivel != null) textoNivel.text = "Nivel " + nivel;

//        // 3. Actualizar Copas
//        if (textoCopas != null) textoCopas.text = copas;

//        // 4. Actualizar Imagen
//        if (imagenRetrato != null)
//        {
//            string nombreLimpio = nombre.ToLower().Replace(" ", "").Replace("-", "");
//            Sprite s = lista.Find(x => x.name.ToLower().Replace(" ", "").Replace("-", "").Contains(nombreLimpio));

//            if (s != null)
//            {
//                imagenRetrato.sprite = s;
//                imagenRetrato.color = Color.white;
//            }
//        }
//    }
//}