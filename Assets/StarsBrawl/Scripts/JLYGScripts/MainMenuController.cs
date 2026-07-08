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

    private string connectionString = "Server=127.0.0.1; Port=3306; Database=brawl_stars; User ID=root; Password=root; SslMode=None;";

    /* // START ANTIGUO COMENTADO:
    async void Start()
    {
        if (string.IsNullOrEmpty(UserSession.CurrentUserID)) return;
        await LoadMainMenuDataAsync(UserSession.CurrentUserID);
    }
    */

    // START NUEVO Y SEGURO:
    async void Start()
    {
        // Comprobación de seguridad para evitar NullReferenceException en la UI
        if (nicknameText == null || totalTrophiesText == null)
        {
          //  Debug.LogError("¡ERROR! Tienes campos de texto sin arrastrar en el Inspector del MainMenuController.");
            return;
        }
       

        if (string.IsNullOrEmpty(UserSession.CurrentUserID))
        {
            Debug.LogWarning("No hay ID de sesión activo.");
            return;
        }

        await LoadMainMenuDataAsync(UserSession.CurrentUserID);
    }

    public void LoginScene()
    {
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
    }
    public void GoBackButton()
    {
        // Nota: Si esta escena YA ES "MainMenu", no cargues "MainMenu" a ti mismo, 
        // cámbialo por "Login" si quieres volver al inicio.
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void UserDetails()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("UserDetails");
    }

    private async Task LoadMainMenuDataAsync(string userId)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();

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
                            // Asignación segura
                            if (nicknameText != null) nicknameText.text = reader["nickname"].ToString();
                            if (coinsText != null) coinsText.text = reader["coins"].ToString();
                            if (gemsText != null) gemsText.text = reader["gems"].ToString();
                            if (blingText != null) blingText.text = reader["bling"].ToString();
                            if (totalTrophiesText != null) totalTrophiesText.text = reader["total_trophies"].ToString();
                        }
                    }
                }

                // ... (el resto de tu lógica de LastBrawler se queda igual, 
                // recuerda añadirle los mismos 'if (variable != null)' por seguridad)
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error asíncrono en MainMenu: " + e.Message);
            }
        }
    }
}

