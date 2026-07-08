using UnityEngine;
using TMPro;
using MySqlConnector;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ColeccionBrawlers : MonoBehaviour
{
    public TMP_Text textoProgreso;
    public Transform contenedorRejilla;
    public GameObject prefabricadoTarjeta;
    public List<Sprite> listaRetratos;

    [Header("UI States")]
    public GameObject loadingSpinner;   // Arrastra aquí tu spinner (activo cuando carga)
    public GameObject mensajeVacio;     // Arrastra aquí el panel/mensaje de "vacío"

    private string cadenaConexion = "Server=127.0.0.1; Port=3306; Database=brawl_stars; User ID=root; Password=root; SslMode=None;";

    // Si el prefab está también como hijo en la escena lo conservamos como plantilla
    private GameObject plantillaEnEscena;

    private class DatosBrawler
    {
        public int id;
        public string nombre;
        public int nivel;
        public int trofeos;
    }

    async void Start()
    {
        if (string.IsNullOrEmpty(UserSession.CurrentUserID)) return;

        if (textoProgreso == null) textoProgreso = GameObject.Find("Brawlers Amount")?.GetComponent<TMP_Text>();
        if (contenedorRejilla == null) contenedorRejilla = GameObject.Find("Brawlers Container")?.transform;

        // Si el objeto referenciado es un GameObject presente dentro del contenedor, lo usamos como plantilla y lo desactivamos
        if (prefabricadoTarjeta != null && prefabricadoTarjeta.transform.parent == contenedorRejilla)
        {
            plantillaEnEscena = prefabricadoTarjeta;
            plantillaEnEscena.SetActive(false);
        }

        await CargarDatosAsync();
    }

    private async Task CargarDatosAsync()
    {
        // Mostrar estado de carga
        if (loadingSpinner != null) loadingSpinner.SetActive(true);
        if (mensajeVacio != null) mensajeVacio.SetActive(false);

        // Limpiar contenedor (conservar plantilla si existe)
        if (contenedorRejilla != null)
        {
            for (int i = contenedorRejilla.childCount - 1; i >= 0; i--)
            {
                var child = contenedorRejilla.GetChild(i).gameObject;
                if (plantillaEnEscena != null && child == plantillaEnEscena) continue;
                Destroy(child);
            }
        }

        List<DatosBrawler> lista = new List<DatosBrawler>();
        int total = 0, desbloqueados = 0;

        // Ejecutar consulta en hilo de trabajo para no bloquear Unity
        await Task.Run(() =>
        {
            using (var conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();

                    using (var cmdTotal = new MySqlCommand("SELECT COUNT(*) FROM BRAWLERS", conexion))
                        total = System.Convert.ToInt32(cmdTotal.ExecuteScalar());

                    using (var cmdUser = new MySqlCommand("SELECT COUNT(*) FROM USER_BRAWLERS WHERE user_id = @id", conexion))
                    {
                        cmdUser.Parameters.AddWithValue("@id", UserSession.CurrentUserID);
                        desbloqueados = System.Convert.ToInt32(cmdUser.ExecuteScalar());
                    }

                    string sqlLista = @"
                        SELECT b.brawler_id, b.name, ub.level, ub.trophies
                        FROM USER_BRAWLERS ub
                        JOIN BRAWLERS b ON ub.brawler_id = b.brawler_id
                        WHERE ub.user_id = @id AND ub.level > 0";
                    using (var cmdLista = new MySqlCommand(sqlLista, conexion))
                    {
                        cmdLista.Parameters.AddWithValue("@id", UserSession.CurrentUserID);
                        using (var reader = cmdLista.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new DatosBrawler
                                {
                                    id = reader.GetInt32("brawler_id"),
                                    nombre = reader.GetString("name"),
                                    nivel = reader.GetInt32("level"),
                                    trofeos = reader.GetInt32("trophies")
                                });
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Fallo en MySQL: " + ex.Message);
                }
            }
        });

        // Volvemos al hilo principal: actualizar UI
        if (textoProgreso != null) textoProgreso.text = desbloqueados + " / " + total;

        if (lista.Count == 0)
        {
            if (mensajeVacio != null) mensajeVacio.SetActive(true);
        }
        else
        {
            foreach (var b in lista)
            {
                // Instanciar clon: si usamos plantilla en escena, clonar esa; si no, clonar el prefab arrastrado desde Assets
                GameObject nuevaTarjeta = plantillaEnEscena != null
                    ? Instantiate(plantillaEnEscena, contenedorRejilla)
                    : Instantiate(prefabricadoTarjeta, contenedorRejilla);

                nuevaTarjeta.SetActive(true);

                // Actualizar textos (fallback)
                TextMeshProUGUI[] textos = nuevaTarjeta.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in textos)
                {
                    if (t.transform.parent != null && t.transform.parent.name == "Brawler Panel") t.text = b.nombre;
                    else if (t.transform.parent != null && t.transform.parent.name == "Power Icon") t.text = "Nivel " + b.nivel;
                    else if (t.transform.parent != null && t.transform.parent.name == "Trophy Amount Slot") t.text = b.trofeos.ToString();
                }

                // Delegar la asignación de sprite al componente TarjetaBrawler si existe
                var tarjetaComp = nuevaTarjeta.GetComponent<TarjetaBrawler>();
                if (tarjetaComp != null) tarjetaComp.ConfigurarImagen(b.nombre, listaRetratos);
            }
        }

        // Ocultar spinner cuando termine
        if (loadingSpinner != null) loadingSpinner.SetActive(false);
    }
}