using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// ==========================================
// 1. CLASES ESPEJO (El formato de la caja)
// ==========================================
[System.Serializable]
public class PersonajePartidaDto
{
    public int personajeId;
    public int kills;
}

[System.Serializable]
public class FinPartidaDto
{
    public int usuarioId;
    public bool esVictoria;
    public List<PersonajePartidaDto> personajes;
}

// ==========================================
// 2. EL CARTERO (El script principal)
// ==========================================
public class ResultadosManager : MonoBehaviour
{
    private string urlServidor = "https://empiresturn.somee.com/api/Stats/GuardarPartida";

    // El GameManager llamará a este método cuando acabe la partida
    public void EnviarEstadisticasAlServidor(FinPartidaDto datosPartida)
    {
        // Iniciamos un proceso en segundo plano (Corrutina)
        StartCoroutine(RutinaEnviarEstadisticas(datosPartida));
    }

    private IEnumerator RutinaEnviarEstadisticas(FinPartidaDto datosPartida)
    {
        // PASO A: Traducción a JSON
        string jsonDatos = JsonUtility.ToJson(datosPartida);
        Debug.Log("Empaquetando datos para enviar: " + jsonDatos);

        // PASO B: Preparar el paquete (UnityWebRequest)
        UnityWebRequest request = new UnityWebRequest(urlServidor, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonDatos);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Le ponemos la etiqueta para que el servidor Somee sepa qué idioma hablamos
        request.SetRequestHeader("Content-Type", "application/json");

        // PASO C: Enviar y esperar
        yield return request.SendWebRequest();

        // PASO D: Leer la respuesta del servidor
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error al contactar con el servidor: " + request.error);
            Debug.LogError("Mensaje de Somee: " + request.downloadHandler.text);
        }
        else
        {
            Debug.Log("¡El servidor ha guardado los datos correctamente!");
            Debug.Log("Respuesta de Somee: " + request.downloadHandler.text);
        }
    }
}