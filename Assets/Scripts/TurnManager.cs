using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;

    public int turnoActual; // Variable que indica el turno del jugador1 (1) o el jugador2 (2). Lo he hecho con ints porque si necesito aumentar el numero de turnos (aliados) se puede hacer.
    public List<CharacterEntity> charactersPJ1 = new List<CharacterEntity>(); // Lista que guardará los personajes del player 1
    public List<CharacterEntity> charactersPJ2 = new List<CharacterEntity>(); // Lista que guardará los personajes del player 2
    public List<CharacterEntity> todosLosPersonajes; // Lista donde guardar todos los personajes, asi sabemos que si todos terminan su turno, significa que cambiamos de turno automaticamente.

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
    public void IniciarPersonajes()
    {
        GameObject pj1Team = GameObject.Find("Player1_team");
        GameObject pj2Team = GameObject.Find("Player2_team");

        //Aqui miramos si por algun casual no encontró nada que salga directamente y no rompa nada.
        if (pj1Team == null || pj2Team == null) return;
        // Buscamos a todos los personajes en la escena al empezar
        // Limpiamos la lista por si acaso
        todosLosPersonajes.Clear();

        // Metemos los personajes de los diferentes equipos en las listas que les pertenece.
        charactersPJ1.AddRange(pj1Team.GetComponentsInChildren<CharacterEntity>());

        charactersPJ2.AddRange(pj2Team.GetComponentsInChildren<CharacterEntity>());


        // Pillamos a todos los hijos de esos objetos y los metemos en la lista global, esto se hará para ver más adelante la ubicación de cada uno.
        todosLosPersonajes.AddRange(charactersPJ1);
        todosLosPersonajes.AddRange(charactersPJ2);

        CambiarTurnoEquipos();
    }
    // Metodo para cambiar de turno
    public void CambiarTurno()
    {
        if (RevisarCambioDeTurno())
        {
            // Mira el turno actual y dependiendo de cual sea cambia por el otro. Lo tengo asi por si en un futuro pienso implementar algunos aliados que tengan su turno unico, nah XD
            if (turnoActual == 1)
            {
                turnoActual = 2;
            }
            else if (turnoActual == 2)
            {
                turnoActual = 1;
            }

            CambiarTurnoEquipos();

            Debug.Log("Turno cambiado. Ahora le toca al Jugador: " + turnoActual);
        }
        
    }

    public bool RevisarCambioDeTurno()
    {
        if (turnoActual == 1)
        {
            foreach (var p in charactersPJ1)
            {
                if (p.estadoActual != CharacterEntity.UnidadEstado.Finalizado) return false;
            }
        }
        else if (turnoActual == 2)
        {
            foreach (var p in charactersPJ2)
            {
                if (p.estadoActual != CharacterEntity.UnidadEstado.Finalizado) return false;
            }
        }
        return true;
    }

    public void CambiarTurnoEquipos()
    {
        if (turnoActual == 1)
        {
            // Usamos un bucle for INVERSO. Si se borra un personaje, no rompe la iteración.
            for (int i = charactersPJ1.Count - 1; i >= 0; i--)
            {
                CharacterEntity p = charactersPJ1[i];

                // Efectos de daño (Quemadura, Veneno...)
                CombatManager.instance.EfectosDeEstado(p);

                // Descontamos los turnos de transformación del personaje
                p.ActualizarTurnoTransformacion();

                // Si el personaje murió o es nulo, saltamos al siguiente personaje (puede morir por los efectos).
                if (p == null || p.stats.vidaActual <= 0) continue;

                // Disminuir contadores de Bufos/Debufos
                p.ActualizarTurnosModificadores();

                if (p.stats.staminaActual < p.stats.staminaMaxima)
                {
                    p.RecuperarStamina(p);
                }
                RestablecerTurno(p);
            }

            foreach (var p in charactersPJ2) TerminarTurno(p);
        }
        else if (turnoActual == 2)
        {
            // 1. Bucle for INVERSO para el jugador 2
            for (int i = charactersPJ2.Count - 1; i >= 0; i--)
            {
                CharacterEntity p = charactersPJ2[i];

                // 1. Efectos de daño
                CombatManager.instance.EfectosDeEstado(p);

                // Descontamos los turnos de transformación del personaje
                p.ActualizarTurnoTransformacion();

                // Ver si ha muerto o no
                if (p == null || p.stats.vidaActual <= 0) continue;

                // 2. NUEVO: Disminuir contadores de Bufos/Debufos
                p.ActualizarTurnosModificadores();

                if (p.stats.staminaActual < p.stats.staminaMaxima)
                {
                    p.RecuperarStamina(p);
                }
                RestablecerTurno(p);
            }

            foreach (var p in charactersPJ1) TerminarTurno(p);
        }
    }

    public void TerminarTurno(CharacterEntity character)
    {
        // Hago que isMoving se quede en false ya que ya no se mueve.
        character.isActing = false;
        // Hago que characterMoved esté en falso ya que al acabar el turno ya nos da igual si se ha movido o no.
        character.characterMoved = false;

        character.characterAttacked = false;
        // Lo cambiamos a color gris, como significado de estar inactivo.
        character.cambioColorAlActuar.color = Color.gray;
        // Marcamos como que ya actuó en su turno
        character.estadoActual = CharacterEntity.UnidadEstado.Finalizado; // Los ponemos en un estado el cual indica que ya no harán ninguna acción.

        CambiarTurno();
    }

    public void RestablecerTurno(CharacterEntity character)
    {
        character.characterAct = false;
        // Le devolvemos el color original, asi damos a entender que pueden actuar de nuevo.
        character.cambioColorAlActuar.color = Color.white;
        // Marcar como que no ha actuado.
        character.estadoActual = CharacterEntity.UnidadEstado.Reposo; // Los ponemos en un estado el cual indica que pueden ejecutar acciones.
    }

    public void RemoveCharacter(CharacterEntity character)
    {
        if (character.equipo != 1) charactersPJ2.Remove(character);
        else charactersPJ1.Remove(character);

        Debug.Log("Personaje eliminado");
    }
}
