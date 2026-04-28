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
    public TMP_InputField inputUsuarioRegister;
    public TMP_InputField inputPasswordRegister;
    public TMP_InputField inputPasswordRegister2;
    public TMP_Text textoMensaje; // Para mostrar errores o éxitos
    public TMP_Text textoMensajeRegister; // Para mostrar errores o éxitos en registro

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

    // Clase para leer la respuesta
    [System.Serializable]
    private class LoginResponse
    {
        public int usuarioId; // O el nombre exacto que use tu API para la ID
        public string mensaje;
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
        if (string.IsNullOrWhiteSpace(inputUsuarioRegister.text) || string.IsNullOrWhiteSpace(inputPasswordRegister.text) || string.IsNullOrWhiteSpace(inputPasswordRegister2.text))
        {
            textoMensajeRegister.text = "Por favor, rellena todos los campos.";
            textoMensajeRegister.color = Color.yellow;
            return;
        }

        // Mínimo de 8 caracteres para la contraseña
        if (inputPasswordRegister.text.Length < 8)
        {
            textoMensajeRegister.text = "La contraseña debe tener al menos 8 caracteres.";
            textoMensajeRegister.color = Color.yellow;
            return;
        }

        if (inputPasswordRegister.text != inputPasswordRegister2.text)
        {
            textoMensajeRegister.text = "Las contraseñas no coinciden.";
            textoMensajeRegister.color= Color.red;
            return;
        }

        StartCoroutine(EnviarPeticion("/register"));
    }

    // --- LA CORRUTINA QUE HACE LA MAGIA ---

    private IEnumerator EnviarPeticion(string endpoint)
    {
        if (endpoint == "/login")
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
                textoMensaje.text = "Cargando...";
                textoMensaje.color = Color.yellow;
                yield return request.SendWebRequest();

                // 5. Analizamos el resultado
                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    textoMensaje.text = "Error de conexión con el servidor.";
                    textoMensaje.color = Color.red;
                }
                else if (request.responseCode == 200) // 200 es el código de éxito (OK)
                {
                    textoMensaje.text = "";
                    // Leemos el texto JSON que el servidor nos ha devuelto
                    string textoRespuesta = request.downloadHandler.text;

                    // Lo convertimos a nuestra clase LoginResponse
                    LoginResponse respuesta = JsonUtility.FromJson<LoginResponse>(textoRespuesta);

                    // Guardamos la ID en PlayerPrefs
                    // Así el GameManager podrá leerla en la otra escena
                    PlayerPrefs.SetInt("IdUsuarioLogueado", respuesta.usuarioId);
                    PlayerPrefs.SetString("NombreUsuarioLogueado", inputUsuario.text);
                    PlayerPrefs.Save(); // Aseguramos que se guarde en la memoria

                    // Mostramos el nombre del usuario
                    nombreUsuario = inputUsuario.text;

                    inputUsuario.text = null;
                    inputPassword.text = null;

                    // Cambiamos de panel gracias al MenuUIManager
                    MenuUIManager.instance.MostrarMenuPrincipal();

                }
                else if (request.responseCode == 400)// Error 400 (usuario repetido)
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
        else
        {
            // 1. Preparamos los datos
            LoginRequest datos = new LoginRequest
            {
                nombre = inputUsuarioRegister.text,
                password = inputPasswordRegister.text
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
                    // 1. Leemos el texto JSON que el servidor nos ha devuelto
                    string textoRespuesta = request.downloadHandler.text;

                    // 2. Lo convertimos a nuestra clase LoginResponse
                    LoginResponse respuesta = JsonUtility.FromJson<LoginResponse>(textoRespuesta);

                    // 3. ¡EL TRUCO MAGICO! Guardamos la ID en PlayerPrefs
                    // Así el GameManager podrá leerla en la otra escena
                    PlayerPrefs.SetInt("IdUsuarioLogueado", respuesta.usuarioId);
                    PlayerPrefs.SetString("NombreUsuarioLogueado", inputUsuarioRegister.text);
                    PlayerPrefs.Save(); // Aseguramos que se guarde en la memoria

                    // 4. Continuamos con tu código normal

                    inputUsuarioRegister.text = null;
                    inputPasswordRegister.text = null;
                    inputPasswordRegister2.text = null;

                    textoMensaje.text = "¡Usuario creado correctamente!";
                    textoMensaje.color = Color.green;
                    // Cambiamos de panel gracias al MenuUIManager
                    MenuUIManager.instance.SalirRegistro();

                }
                else if (request.responseCode == 400)// Error 400 (usuario repetido)
                {
                    textoMensajeRegister.text = "Error: Este usuario ya existe";
                    textoMensajeRegister.color = Color.red;
                }
                else if (request.responseCode == 401)// Error 400 (usuario inexistente)
                {
                    textoMensajeRegister.text = "Error: Este usuario no existe";
                    textoMensajeRegister.color = Color.red;
                }
                else
                {
                    // Ahora Unity nos mostrará exactamente el texto real que envía tu API
                    textoMensajeRegister.text = "Error " + request.responseCode + ": " + request.downloadHandler.text;
                    textoMensajeRegister.color = Color.red;
                }
            }
        }
        
    }
}