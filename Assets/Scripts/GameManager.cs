using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Conexión con Sistemas")]
    public ResultadosManager estadisticasManager;

    [Header("Listas de Combate (Vivos)")]
    [SerializeField] private List<CharacterEntity> characters_pj1 = new List<CharacterEntity>();
    [SerializeField] private List<CharacterEntity> characters_pj2 = new List<CharacterEntity>();

    // Listas Historial: Aquí se quedan todos, aunque mueran.
    private List<CharacterEntity> historial_pj1 = new List<CharacterEntity>();

    void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
    }

    // Se llama al crear cada personaje al inicio de la partida
    public void RegisterCharacter(CharacterEntity character)
    {
        if (character.equipo == 1)
        {
            characters_pj1.Add(character);
            historial_pj1.Add(character); // Guardamos la referencia para el final
        }
        else
        {
            characters_pj2.Add(character);
        }
    }

    // Se llama cada vez que alguien llega a 0 de vida
    public void DeathManagment(CharacterEntity character)
    {
        // Solo lo quitamos de la lista de "Vivos"
        if (character.equipo == 1) characters_pj1.Remove(character);
        else characters_pj2.Remove(character);

        TurnManager.instance.RemoveCharacter(character);

        // Comprobamos si alguien ha ganado
        FinPartida();
    }

    public void FinPartida()
    {
        if (characters_pj1.Count <= 0)
        {
            Debug.Log("Jugador 2 Gana");
            EmpaquetarYEnviar(false); // Player 1 perdió
        }
        else if (characters_pj2.Count <= 0)
        {
            Debug.Log("Jugador 1 Gana");
            EmpaquetarYEnviar(true); // Player 1 ganó
        }
    }

    private void EmpaquetarYEnviar(bool esVictoria)
    {
        // 1. CREAR EL PAQUETE
        FinPartidaDto paquete = new FinPartidaDto();

        // 2. OBTENER ID DEL USUARIO (Aquí está el truco)
        // Buscamos el ID que guardó el AuthManager al hacer login
        // (Asumo que añadiremos una variable 'idUsuarioLogueado' en tu AuthManager)
        paquete.usuarioId = PlayerPrefs.GetInt("IdUsuarioLogueado", 1);

        paquete.esVictoria = esVictoria;
        paquete.personajes = new List<PersonajePartidaDto>();

        // 3. RECOLECTAR KILLS DE TU EQUIPO
        foreach (CharacterEntity pj in historial_pj1)
        {
            PersonajePartidaDto info = new PersonajePartidaDto();
            info.personajeId = pj.data.idDataBase; // ID del ScriptableObject
            info.kills = pj.killsEstaPartida;      // Kills acumuladas

            paquete.personajes.Add(info);
        }

        // 4. ENVIAR AL SERVIDOR
        if (estadisticasManager != null)
            estadisticasManager.EnviarEstadisticasAlServidor(paquete);

        // 5. REINICIAR TRAS UN BREVE DESCANSO
        //Invoke("ReiniciarPartida", 10f);
        UIManager.instance.PartidaFinalizada(esVictoria);
    }

    public void SalirPartida()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Inicio"); //Volvemos al menú
    }
}