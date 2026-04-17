using System.Collections;
using UnityEngine;
using UnityEngine.Networking; // Necesario para hacer peticiones web
using TMPro;
using UnityEngine.SceneManagement;
using System.Net.Http; // Cambia a UnityEngine.UI si usas textos normales en vez de TextMeshPro

public class AuthManager : MonoBehaviour
{
    public static AuthManager instance;

    // Cambia el puerto 7008 por el puerto real que te aparece en la URL de tu Swagger
    private string baseUrl = "http://empiresturn.somee.com/api/Auth";

    [Header("Referencias UI")]
    public TMP_InputField inputUsuario;
    public TMP_InputField inputPassword;
    public TMP_Text textoMensaje; // Para mostrar errores o éxitos

    public string nombreUsuario;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // Evita que haya dos managers por error
            return;
        }
    }

    // Clase auxiliar para convertir nuestros datos a JSON (Debe coincidir con la de la API)
    [System.Serializable]
    private class LoginRequest
    {
        public string nombre;
        public string password;
    }

    // --- MÉTODOS PÚBLICOS PARA LOS BOTONES ---

    public void BotonLogin()
    {
        // Comprobamos si las cajas están vacías o solo tienen espacios
        if (string.IsNullOrWhiteSpace(inputUsuario.text) || string.IsNullOrWhiteSpace(inputPassword.text))
        {
            textoMensaje.text = "Por favor, rellena todos los campos.";
            textoMensaje.color = Color.yellow;
            return; // Cortamos la ejecución aquí, no enviamos nada
        }

        StartCoroutine(EnviarPeticion("/login"));
    }

    public void BotonRegister()
    {
        if (string.IsNullOrWhiteSpace(inputUsuario.text) || string.IsNullOrWhiteSpace(inputPassword.text))
        {
            textoMensaje.text = "Por favor, rellena todos los campos.";
            textoMensaje.color = Color.yellow;
            return;
        }

        StartCoroutine(EnviarPeticion("/register"));
    }

    // --- LA CORRUTINA QUE HACE LA MAGIA ---

    private IEnumerator EnviarPeticion(string endpoint)
    {
        // 1. Preparamos los datos
        LoginRequest datos = new LoginRequest
        {
            nombre = inputUsuario.text,
            password = inputPassword.text
        };

        // 2. Convertimos los datos a texto JSON
        string jsonDatos = JsonUtility.ToJson(datos);

        // 3. Configuramos la petición POST
        string urlCompleta = baseUrl + endpoint;
        using (UnityWebRequest request = new UnityWebRequest(urlCompleta, "POST"))
        {
            // Convertimos el JSON a bytes para enviarlo
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonDatos);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Le decimos a la API que le estamos enviando un JSON
            request.SetRequestHeader("Content-Type", "application/json");

            // 4. Enviamos la petición y esperamos la respuesta
            yield return request.SendWebRequest();

            // 5. Analizamos el resultado
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                textoMensaje.text = "Error de conexión con el servidor.";
                textoMensaje.color = Color.red;
            }
            else if (request.responseCode == 200) // 200 es el código de éxito (OK)
            {
                textoMensaje.text = "¡Éxito! " + endpoint + " correcto.";
                textoMensaje.color = Color.green;
                nombreUsuario = inputUsuario.text;
                inputUsuario.text = null;
                inputPassword.text = null;
                textoMensaje.text = null;
                // Aquí llamamos a la funcion de MenuUIManager para cambiar de panel
                MenuUIManager.instance.MostrarMenuPrincipal();
                
            }
            else if(request.responseCode == 400)// Error 400 (usuario repetido)
            {
                textoMensaje.text = "Error: Este usuario ya existe";
                textoMensaje.color = Color.red;
            }
            else if (request.responseCode == 401)// Error 400 (usuario inexistente)
            {
                textoMensaje.text = "Error: Este usuario no existe";
                textoMensaje.color = Color.red;
            }
            else
            {
                // Ahora Unity nos mostrará exactamente el texto real que envía tu API
                textoMensaje.text = "Error " + request.responseCode + ": " + request.downloadHandler.text;
                textoMensaje.color = Color.red;
            }
        }
    }
}