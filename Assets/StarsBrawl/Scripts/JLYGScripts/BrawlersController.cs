////using UnityEngine;
////using TMPro;
////using MySqlConnector;
////using System.Threading.Tasks;
////using System.Collections.Generic;

////public class BrawlersController : MonoBehaviour
////{
////    [Header("Interfaz General")]
////    public TextMeshProUGUI brawlersCountText;

////    [Header("Lista de Brawlers")]
////    public Transform brawlersContainer;
////    public GameObject brawlerPrefab;

////    // IMPORTANTE: Asegúrate de que tu contraseña sea correcta (vacía o "root")
////    private string connectionString = "Server=127.0.0.1; Port=3306; Database=brawl_stars; User ID=root; Password=root; SslMode=None;";

////    // 1. Creamos una clase contenedora para sacar los datos limpios del hilo secundario
////    private class BrawlerData
////    {
////        public int id;
////        public string name;
////        public int level;
////        public int trophies;
////    }

////    async void Start()
////    {
////        if (string.IsNullOrEmpty(UserSession.CurrentUserID))
////        {
////            Debug.LogWarning("No hay sesión activa.");
////            return;
////        }

////        await LoadBrawlersDataSafeAsync(UserSession.CurrentUserID);
////    }

////    private async Task LoadBrawlersDataSafeAsync(string userId)
////    {
////        int total = 0;
////        int unlocked = 0;
////        List<BrawlerData> brawlersList = new List<BrawlerData>();

////        // 2. HILO SECUNDARIO: Aquí hacemos todo el trabajo pesado y asíncrono de SQL
////        await Task.Run(() =>
////        {
////            using (var connection = new MySqlConnection(connectionString))
////            {
////                try
////                {
////                    connection.Open(); // Usamos los métodos normales dentro del hilo asíncrono

////                    using (var cmdTotal = new MySqlCommand("SELECT COUNT(*) FROM BRAWLERS", connection))
////                    {
////                        total = System.Convert.ToInt32(cmdTotal.ExecuteScalar());
////                    }

////                    using (var cmdUnlocked = new MySqlCommand("SELECT COUNT(*) FROM USER_BRAWLERS WHERE user_id = @id", connection))
////                    {
////                        cmdUnlocked.Parameters.AddWithValue("@id", userId);
////                        unlocked = System.Convert.ToInt32(cmdUnlocked.ExecuteScalar());
////                    }

////                    string queryList = @"
////                        SELECT b.brawler_id, b.name, ub.level, ub.trophies
////                        FROM USER_BRAWLERS ub
////                        JOIN BRAWLERS b ON ub.brawler_id = b.brawler_id
////                        WHERE ub.user_id = @id";

////                    using (var cmdList = new MySqlCommand(queryList, connection))
////                    {
////                        cmdList.Parameters.AddWithValue("@id", userId);
////                        using (var reader = cmdList.ExecuteReader())
////                        {
////                            while (reader.Read())
////                            {
////                                // Guardamos la info pura sin tocar nada visual aún
////                                brawlersList.Add(new BrawlerData
////                                {
////                                    id = reader.GetInt32("brawler_id"),
////                                    name = reader.GetString("name"),
////                                    level = reader.GetInt32("level"),
////                                    trophies = reader.GetInt32("trophies")
////                                });
////                            }
////                        }
////                    }
////                }
////                catch (System.Exception e)
////                {
////                    Debug.LogError("Error en base de datos: " + e.Message);
////                }
////            }
////        });

////        // 3. HILO PRINCIPAL: Volvemos a Unity de forma segura para instanciar la UI
////        if (brawlersCountText != null)
////        {
////            brawlersCountText.text = unlocked + " / " + total;
////        }

////        PortraitProvider portraitProvider = FindObjectOfType<PortraitProvider>();

////        foreach (var b in brawlersList)
////        {
////            GameObject brawlerObj = Instantiate(brawlerPrefab, brawlersContainer);
////            BrawlerItemUI itemUI = brawlerObj.GetComponent<BrawlerItemUI>();

////            if (itemUI != null)
////            {
////                Sprite portrait = null;
////                if (portraitProvider != null)
////                {
////                    portraitProvider.TryGetPortraitByID(b.id, out portrait);
////                }

////                itemUI.Setup(b.name, b.level, b.trophies, portrait, b.id.ToString());
////            }
////        }
////    }
////}


//using UnityEngine;
//using TMPro;
//using MySqlConnector;
//using System.Threading.Tasks;

//public class BrawlersController : MonoBehaviour
//{
//    [Header("Interfaz General")]
//    public TextMeshProUGUI brawlersCountText; // Texto para el formato "Desbloqueados / Totales"

//    [Header("Lista de Brawlers")]
//    public Transform brawlersContainer;       // El objeto Grid/Contenedor donde se meterán los clones
//    public GameObject brawlerPrefab;          // El elemento visual prefabricado de la tarjeta del brawler

//    private string connectionString = "Server=127.0.0.1; Port=3306; Database=brawl_stars; User ID=root; Password=root; SslMode=None;";

//    async void Start()
//    {
//        if (string.IsNullOrEmpty(UserSession.CurrentUserID))
//        {
//            Debug.LogError("No hay un usuario logueado en la sesión para cargar sus Brawlers.");
//            return;
//        }

//        // Ejecutamos la carga de datos de manera asíncrona
//        await LoadBrawlersDataAsync(UserSession.CurrentUserID);
//    }

//    private async Task LoadBrawlersDataAsync(string userId)
//    {
//        using (var connection = new MySqlConnection(connectionString))
//        {
//            try
//            {
//                await connection.OpenAsync();

//                int totalBrawlers = 0;
//                int unlockedBrawlers = 0;

//                // 1ª CONSULTA: Contamos los brawlers totales usando COUNT en el servidor
//                string queryTotal = "SELECT COUNT(*) FROM BRAWLERS";
//                using (var cmd = new MySqlCommand(queryTotal, connection))
//                {
//                    totalBrawlers = System.Convert.ToInt32(await cmd.ExecuteScalarAsync());
//                }

//                // 2ª CONSULTA: Contamos los brawlers del usuario actual usando COUNT en el servidor
//                string queryUnlocked = "SELECT COUNT(*) FROM USER_BRAWLERS WHERE user_id = @id";
//                using (var cmd = new MySqlCommand(queryUnlocked, connection))
//                {
//                    cmd.Parameters.AddWithValue("@id", userId);
//                    unlockedBrawlers = System.Convert.ToInt32(await cmd.ExecuteScalarAsync());
//                }

//                // Actualizamos el texto de progreso de la interfaz
//                if (brawlersCountText != null)
//                {
//                    brawlersCountText.text = unlockedBrawlers + " / " + totalBrawlers;
//                }

//                // 3ª CONSULTA: Traemos la lista detallada de brawlers del usuario cruzando tablas con un JOIN
//                string queryList = @"
//                    SELECT b.brawler_id, b.name, ub.level, ub.trophies
//                    FROM USER_BRAWLERS ub
//                    JOIN BRAWLERS b ON ub.brawler_id = b.brawler_id
//                    WHERE ub.user_id = @id";

//                using (var cmdList = new MySqlCommand(queryList, connection))
//                {
//                    cmdList.Parameters.AddWithValue("@id", userId);
//                    using (var reader = await cmdList.ExecuteReaderAsync())
//                    {
//                        // Buscamos el componente PortraitProvider del proyecto base para los Sprites/Portraits
//                        PortraitProvider portraitProvider = FindObjectOfType<PortraitProvider>();

//                        while (await reader.ReadAsync())
//                        {
//                            int brawlerId = reader.GetInt32("brawler_id");
//                            string brawlerName = reader.GetString("name");
//                            int level = reader.GetInt32("level");
//                            int trophies = reader.GetInt32("trophies");

//                            // Clonamos el Prefab dentro del contenedor visual de la UI
//                            GameObject brawlerObj = Instantiate(brawlerPrefab, brawlersContainer);
//                            BrawlerItemUI itemUI = brawlerObj.GetComponent<BrawlerItemUI>();

//                            if (itemUI != null)
//                            {
//                                Sprite portrait = null;
//                                if (portraitProvider != null)
//                                {
//                                    // Intentamos obtener el retrato dinámicamente usando el ID del brawler de la base de datos
//                                    portraitProvider.TryGetPortraitByID(brawlerId, out portrait);
//                                }

//                                // Configuramos la tarjeta visual con los datos e ID correspondientes
//                                itemUI.Setup(brawlerName, level, trophies, portrait, brawlerId.ToString());
//                            }
//                        }
//                    }
//                }
//            }
//            catch (System.Exception e)
//            {
//                Debug.LogError("Error asíncrono al cargar la lista de Brawlers: " + e.Message);
//            }
//        }
//    }
//}