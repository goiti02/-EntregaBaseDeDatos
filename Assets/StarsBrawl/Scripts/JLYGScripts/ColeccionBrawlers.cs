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

    [Header("UI States (opcional)")]
    public GameObject loadingSpinner;   // Arrastra aquí tu spinner si lo tienes
    public GameObject mensajeVacio;     // Panel/mensaje para "aún no tienes brawlers"

    private string cadenaConexion = "Server=127.0.0.1; Port=3306; Database=brawl_stars; User ID=root; Password=root; SslMode=None;";

    // Estructura para pasar datos desde el hilo de trabajo
    private class DatosBrawler
    {
        public int id;
        public string nombre;
        public int nivel;
        public int trofeos;
    }

    void Awake()
    {
        // Intentos seguros de auto-asignación temprana (si el diseñador se olvidó)
        if (textoProgreso == null)
        {
            var go = GameObject.Find("Brawlers Amount");
            if (go != null) textoProgreso = go.GetComponent<TMP_Text>();
        }

        if (contenedorRejilla == null)
        {
            var go = GameObject.Find("Brawlers Container");
            if (go != null) contenedorRejilla = go.transform;
        }

        // Aviso si falta el prefab (mejor detectarlo pronto)
        if (prefabricadoTarjeta == null)
            Debug.LogWarning("ColeccionBrawlers: `prefabricadoTarjeta` no está asignado en el Inspector.");
    }

    async void Start()
    {
        // Esperar un frame para permitir que otras inicializaciones terminen
        await Task.Yield();

        if (string.IsNullOrEmpty(UserSession.CurrentUserID))
        {
            Debug.LogError("ColeccionBrawlers: UserSession.CurrentUserID está vacío.");
            return;
        }

        // Validaciones claras antes de ejecutar la carga
        if (textoProgreso == null || contenedorRejilla == null || prefabricadoTarjeta == null)
        {
            Debug.LogError($"ColeccionBrawlers: Referencias faltantes. textoProgreso={(textoProgreso==null)}, contenedorRejilla={(contenedorRejilla==null)}, prefabricadoTarjeta={(prefabricadoTarjeta==null)}");
            return;
        }

        await CargarDatosAsync();
    }

    private async Task CargarDatosAsync()
    {
        // Mostrar estado de carga
        if (loadingSpinner != null) loadingSpinner.SetActive(true);
        if (mensajeVacio != null) mensajeVacio.SetActive(false);

        // Limpiar contenedor para evitar que la plantilla quede visible o se dupliquen hijos antiguos
        if (contenedorRejilla != null)
        {
            for (int i = contenedorRejilla.childCount - 1; i >= 0; i--)
            {
                Destroy(contenedorRejilla.GetChild(i).gameObject);
            }
        }

        var lista = new List<DatosBrawler>();
        int total = 0, desbloqueados = 0;

        // Ejecutar consultas en hilo secundario para no bloquear Unity
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

        // Volvemos al hilo principal: actualizar UI e instanciar prefabs
        if (textoProgreso != null) textoProgreso.text = desbloqueados + " / " + total;

        if (lista.Count == 0)
        {
            if (mensajeVacio != null) mensajeVacio.SetActive(true);
            if (loadingSpinner != null) loadingSpinner.SetActive(false);
            return;
        }

        foreach (var b in lista)
        {
            GameObject tarjeta = null;
            try
            {
                tarjeta = Instantiate(prefabricadoTarjeta, contenedorRejilla);
                tarjeta.SetActive(true);

                // Actualizar textos protegiendo contra transform.parent == null
                var textos = tarjeta.GetComponentsInChildren<TMP_Text>(true);
                foreach (var t in textos)
                {
                    var parent = t.transform.parent;
                    var parentName = parent != null ? parent.name : string.Empty;

                    if (parentName == "Brawler Panel") t.text = b.nombre;
                    else if (parentName == "Power Icon") t.text = "Nivel " + b.nivel;
                    else if (parentName == "Trophy Amount Slot") t.text = b.trofeos.ToString();
                }

                // Delegar imagen al componente de la tarjeta si existe
                var tarjetaComp = tarjeta.GetComponent<TarjetaBrawler>();
                if (tarjetaComp != null) tarjetaComp.ConfigurarImagen(b.nombre, listaRetratos);
                else Debug.LogWarning("Prefab no tiene `TarjetaBrawler` — saltando configuración de imagen.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error al crear/actualizar tarjeta: " + ex.Message);
                if (tarjeta != null) Destroy(tarjeta);
            }
        }

        if (loadingSpinner != null) loadingSpinner.SetActive(false);
    }
}