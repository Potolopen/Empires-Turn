using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField]
    private List<CharacterEntity> characters_pj1 = new List<CharacterEntity>();
    [SerializeField]
    private List<CharacterEntity> characters_pj2 = new List<CharacterEntity>();

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

    // El SpawnManager llamará a esto cuando cree a cada personaje
    public void RegisterCharacter(CharacterEntity character)
    {
        if (character.equipo != 1) characters_pj2.Add(character);
        else characters_pj1.Add(character);
    }

    public void DeathManagment(CharacterEntity character)
    {
        if (character.equipo != 1) characters_pj2.Remove(character);
        else characters_pj1.Remove(character);

        TurnManager.instance.RemoveCharacter(character);

        Win();
    }

    public void Win()
    {
        if (characters_pj1.Count <= 0)
        {
            Debug.Log("Jugador2 Ha Ganado");
            ReiniciarPartida();
        }
        else if (characters_pj2.Count <= 0)
        {
            Debug.Log("Jugador1 Ha Ganado");
            ReiniciarPartida();
        }
    }

    public void ReiniciarPartida()
    {
        // Obtiene la escena que está abierta actualmente
        Scene escenaActual = SceneManager.GetActiveScene();

        // La carga de nuevo desde cero
        SceneManager.LoadScene(escenaActual.name);
    }
}
