using UnityEngine;
using TMPro;
using MySqlConnector;
using UnityEngine.SceneManagement;
using System.Threading.Tasks; // Necesario para la asincronía

public class LoginController : MonoBehaviour
{
    public TMP_InputField idInput;

    // Cadena de conexión (Asegúrate de que el puerto y credenciales de tu XAMPP/Workbench coincidan)
    // private string connectionString = "Server=127.0.0.1; Database=brawl_stars; User ID=root; Password=root; SslMode=None;"
    private string connectionString = "Server=127.0.0.1; Port=3306; Database=brawl_stars; User ID=root; Password=root; SslMode=None;";

    // Usamos 'async void' porque es un evento que se lanzará desde un botón en Unity
    public async void OnConfirmClick()
    {
        string inputID = idInput.text;

        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                // AQUÍ ESTÁ LA CLAVE DE LOS PUNTOS: Usamos OpenAsync para no bloquear el hilo principal
                await connection.OpenAsync();

                string query = "SELECT user_id FROM USERS WHERE user_id = @id";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", inputID);

                    // También ejecutamos la lectura de forma asíncrona
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            // Si existe, guardamos la sesión y cargamos la siguiente escena
                            UserSession.CurrentUserID = inputID;
                            SceneManager.LoadScene("MainMenu");
                        }
                        else
                        {
                            Debug.LogError("Acceso denegado: El ID introducido no existe en la base de datos.");
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error crítico de conexión a MySQL: " + e.Message);
            }
        }
    }
}