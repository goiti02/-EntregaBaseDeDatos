using UnityEngine;
using TMPro;
using MySqlConnector;
using System.Threading.Tasks;

public class MainMenuController : MonoBehaviour
{
    [Header("Datos del Jugador")]
    public TextMeshProUGUI nicknameText;
    public TextMeshProUGUI totalTrophiesText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI gemsText;
    public TextMeshProUGUI blingText;

    [Header("Último Brawler Usado")]
    public TextMeshProUGUI lastBrawlerLevelText;
    public TextMeshProUGUI lastBrawlerTrophiesText;

    // Recuerda poner tu contraseña root o quitarla si en XAMPP no tienes
    private string connectionString = "Server=127.0.0.1; Port=3306; Database=brawl_stars; User ID=root; Password=root; SslMode=None;";

    async void Start()
    {
        // Si entramos a la escena pero no hay ID (ej: le dimos Play directo aquí), no hacemos nada
        if (string.IsNullOrEmpty(UserSession.CurrentUserID)) return;

        await LoadMainMenuDataAsync(UserSession.CurrentUserID);
    }

    private async Task LoadMainMenuDataAsync(string userId)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();

                // 1ª CONSULTA: Datos básicos + Suma total de trofeos delegada al servidor
                string queryUser = @"
                    SELECT u.nickname, u.coins, u.gems, u.bling,
                           (SELECT SUM(trophies) FROM USER_BRAWLERS WHERE user_id = @id) as total_trophies
                    FROM USERS u
                    WHERE u.user_id = @id";

                using (var cmdUser = new MySqlCommand(queryUser, connection))
                {
                    cmdUser.Parameters.AddWithValue("@id", userId);
                    using (var reader = await cmdUser.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            nicknameText.text = reader["nickname"].ToString();
                            coinsText.text = reader["coins"].ToString();
                            gemsText.text = reader["gems"].ToString();
                            blingText.text = reader["bling"].ToString();
                            totalTrophiesText.text = reader["total_trophies"].ToString();
                        }
                    }
                }

                // 2ª CONSULTA: Stats del último brawler usado (cruzando tablas y ordenando por fecha)
                string queryLastBrawler = @"
                    SELECT ub.level, ub.trophies
                    FROM MATCH_USER mu
                    JOIN MATCHES m ON mu.match_id = m.match_id
                    JOIN USER_BRAWLERS ub ON mu.user_id = ub.user_id AND mu.brawler_id = ub.brawler_id
                    WHERE mu.user_id = @id
                    ORDER BY m.date DESC
                    LIMIT 1";

                using (var cmdBrawler = new MySqlCommand(queryLastBrawler, connection))
                {
                    cmdBrawler.Parameters.AddWithValue("@id", userId);
                    using (var reader = await cmdBrawler.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            lastBrawlerLevelText.text = "Nivel " + reader["level"].ToString();
                            lastBrawlerTrophiesText.text = reader["trophies"].ToString();
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error asíncrono en MainMenu: " + e.Message);
            }
        }
    }
}