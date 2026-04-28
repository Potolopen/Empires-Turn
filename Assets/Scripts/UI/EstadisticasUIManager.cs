using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

// --- LAS CAJAS PARA TRADUCIR EL JSON DE SOMEE ---
[System.Serializable]
public class ResumenPersonajeDto
{
    public int personajeId;
    public int killsTotales;
    public int vecesUsado;
}

[System.Serializable]
public class ResumenEstadisticasDto
{
    public int partidasJugadas;
    public int victorias;
    public int derrotas;
    public List<ResumenPersonajeDto> personajes;
}

public class EstadisticasUIManager : MonoBehaviour
{
    public static EstadisticasUIManager instance;

    [Header("Conexión API")]
    // Asegúrate de cambiar esto si tu URL es distinta
    private string urlBase = "http://empiresturn.somee.com/api/Stats/Ver/";

    [Header("UI Global (Resumen)")]
    public TMP_Text textoPartidas;
    public TMP_Text textoVictorias;
    public TMP_Text textoDerrotas;

    [Header("Generador de Filas")]
    public Transform contenedorLista; // El objeto 'Content' del Scroll View
    public GameObject filaPrefab;     // El Prefab que tiene el script FilaEstadisticaUI

    [Header("Tus Personajes (Para sacar las caras)")]
    public List<CharacterData> todosLosPersonajes = new List<CharacterData>();

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void CargarDatos()
    {
        // Sacamos la ID del usuario que guardó el AuthManager al iniciar sesión
        int idUsuario = PlayerPrefs.GetInt("IdUsuarioLogueado", 1);
        StartCoroutine(RutinaDescargarEstadisticas(idUsuario));
    }

    private IEnumerator RutinaDescargarEstadisticas(int idUsuario)
    {
        // 1. Limpieza: Borramos las filas viejas por si el jugador abre y cierra el menú
        foreach (Transform child in contenedorLista)
        {
            Destroy(child.gameObject);
        }

        textoPartidas.text = "0";
        textoVictorias.text = "0";
        textoDerrotas.text = "0";

        // 2. Pedir datos a Somee
        string urlCompleta = urlBase + idUsuario;
        using (UnityWebRequest request = UnityWebRequest.Get(urlCompleta))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error al descargar stats: " + request.error);
                textoPartidas.text = "Error de conexión.";
            }
            else
            {
                // 3. Traducir la respuesta JSON a nuestras clases de C#
                string json = request.downloadHandler.text;
                ResumenEstadisticasDto resumen = JsonUtility.FromJson<ResumenEstadisticasDto>(json);

                // 4. Actualizar textos generales
                textoPartidas.text = resumen.partidasJugadas.ToString();
                textoVictorias.text = resumen.victorias.ToString();
                textoDerrotas.text = resumen.derrotas.ToString();

                // 5. Fabricar las filas de los personajes
                foreach (ResumenPersonajeDto pjStats in resumen.personajes)
                {
                    // Buscamos a este personaje en tu lista de ScriptableObjects usando la ID
                    CharacterData data = todosLosPersonajes.Find(p => p.idDataBase == pjStats.personajeId);

                    if (data != null)
                    {
                        // Instanciamos el Prefab
                        GameObject nuevaFila = Instantiate(filaPrefab, contenedorLista);

                        // Cogemos el script de la fila y le damos los datos
                        FilaEstadisticaUI scriptFila = nuevaFila.GetComponent<FilaEstadisticaUI>();
                        scriptFila.Configurar(data.characterFace, data.nombre, pjStats.killsTotales, pjStats.vecesUsado);
                    }
                }
            }
        }
    }
}