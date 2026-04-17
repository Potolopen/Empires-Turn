using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;


    public List<GameObject> personajesP1Prefabs;
    public List<GameObject> personajesP2Prefabs;

    public Transform player1Parent;
    public Transform player2Parent;

    public Transform spawnPointsP1;
    public Transform spawnPointsP2;

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

    private void Start()
    {
        // Si venimos del menú, las listas tendrán los 6 personajes
        // Si la lista está vacía (porque probamos la escena solo), puedes poner unos por defecto
        if (DatosCombate.equipoP1.Count > 0)
        {
            personajesP1Prefabs = DatosCombate.equipoP1;
            personajesP2Prefabs = DatosCombate.equipoP2;
        }

        SpawnEquipo(personajesP1Prefabs, spawnPointsP1, player1Parent);
        SpawnEquipo(personajesP2Prefabs, spawnPointsP2, player2Parent);
        TurnManager.instance.IniciarPersonajes();
    }

    void SpawnEquipo(List<GameObject> prefabs, Transform spawnContainer, Transform parent)
    {
        int cantidad = Mathf.Min(prefabs.Count, spawnContainer.childCount);

        for (int i = 0; i < cantidad; i++)
        {
            Transform spawn = spawnContainer.GetChild(i);

            GameObject obj = Instantiate(prefabs[i], spawn.position, Quaternion.identity, parent);

            CharacterEntity character = obj.GetComponent<CharacterEntity>();

            if (parent == player1Parent)
                character.equipo = 1;
            else
                character.equipo = 2;

            GameManager.instance.RegisterCharacter(character);
        }
    }
}

