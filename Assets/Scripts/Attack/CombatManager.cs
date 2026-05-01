using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Tilemaps;
using static CharacterMovement;

public class CombatManager : MonoBehaviour
{
    public static CombatManager instance;

    public CharacterEntity character;
    public Tilemap attackOverlay;
    public Tile overlayAttackTile;
    public List<Vector3Int> rangeCells; // Lista de casillas que el ataque puede llegar.
    public AttackData ataqueSeleccionado; // Para guardar el ataque elegido en la UI

    [HideInInspector] public bool ataqueActualAcerto = false;
    [HideInInspector] public bool ataqueActualEsCritico = false;

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
    void Start()
    {

        if (character == null)
            character = GetComponent<CharacterEntity>();

        GameObject overlayAT = GameObject.Find("AttackOverlay");
        if (overlayAT != null)
        {
            attackOverlay = overlayAT.GetComponent<Tilemap>();
        }
    }

    // Array con las cuatro direcciones cardinales, usado para BFS
    static readonly Vector3Int[] directions = {
        Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
    };

    public bool PuedeAtravesar(Vector3Int cell)
    {
        Node nodo = GridManager.instance.GetNode(cell);
        if (nodo == null || nodo.unitPresent == null) return true;
        if (nodo.unitPresent.equipo == character.equipo) return true;
        return false;
    }

    public List<Vector3Int> GetRangeAttackCells(CharacterEntity character, int rango)
    {
        attackOverlay.ClearAllTiles();
        List<Vector3Int> reachable = new List<Vector3Int>();
        Queue<BFSNode> queue = new Queue<BFSNode>();
        Dictionary<Vector3Int, int> visited = new Dictionary<Vector3Int, int>();

        queue.Enqueue(new BFSNode(character.currentCell, rango));

        while (queue.Count > 0)
        {
            BFSNode node = queue.Dequeue();
            Vector3Int cell = node.cell;
            int rangoRestante = node.movimientoRestante;

            // Ignorar si ya visitamos con igual o más rango
            if (visited.ContainsKey(cell) && visited[cell] >= rangoRestante)
                continue;

            visited[cell] = rangoRestante;

            reachable.Add(cell);
            attackOverlay.SetTile(cell, overlayAttackTile);
            rangeCells = reachable;


            // Vecinos
            foreach (Vector3Int dir in directions)
            {
                Vector3Int vecino = cell + dir;
                Node nodoVecino = GridManager.instance.GetNode(vecino);

                if (nodoVecino == null) continue; // fuera del mapa
                                                  // enemigos no bloquean

                int nuevoRango = rangoRestante - 1; // decremento correcto
                if (nuevoRango >= 0) // encolar mientras quede rango
                    queue.Enqueue(new BFSNode(vecino, nuevoRango));
            }
        }

        return reachable;
    }

    public void EfectosDeEstado(CharacterEntity character)
    {
        if (character.statusType == StatusType.Neutro) return;

        int valorTotal = 0;
        bool haceDaño = true;

        switch (character.statusType)
        {
            case StatusType.Quemadura:
                valorTotal = Mathf.CeilToInt(character.stats.vidaMaxima * 0.2f);
                Debug.Log(character.data.nombre + " se quema!!");
                AudioManager.instance.sonidoSegunEstado(character.statusType);
                break;
            case StatusType.Frio:
                valorTotal = Mathf.CeilToInt(character.stats.vidaMaxima * 0.2f);
                AudioManager.instance.sonidoSegunEstado(character.statusType);
                break;
            case StatusType.Envenenamiento:
                if (character.turnosEstadoRestantes == 3)
                {
                    valorTotal = Mathf.CeilToInt(character.stats.vidaMaxima * 0.2f);
                }
                else if (character.turnosEstadoRestantes == 2)
                {
                    valorTotal = Mathf.CeilToInt(character.stats.vidaMaxima * 0.3f);
                }
                else
                {
                    valorTotal = Mathf.CeilToInt(character.stats.vidaMaxima * 0.4f);
                }
                AudioManager.instance.sonidoSegunEstado(character.statusType);
                break;
            case StatusType.Lentitud:
                AudioManager.instance.sonidoSegunEstado(character.statusType);
                haceDaño = false;
                break;
        }

        // 1. Aplicamos daño a la vida si el estado lo requiere
        if (haceDaño && valorTotal > 0)
        {
            // Usamos tu método ya creado. ¡Él se encarga del texto flotante y de comprobar si muere!
            character.RecibirDaño(valorTotal);
        }
        
        if (character.stats.vidaActual <= 0)
        {
            return;
        }

        character.turnosEstadoRestantes--;

        if (character.turnosEstadoRestantes <= 0)
        {
            Debug.Log("El efecto de estado en " + character.data.nombre + " ha expirado.");
            character.LimpiarEstado();
        }
        else
        {
            // --- NUEVO: Reproducir la animación de daño por estado ---
            if (CombatVisualManager.instance != null && character.vfxEstadoActual != null)
            {
                // Usamos un tiempo por defecto (ej. 1 segundo) para el "tick" de daño
                CombatVisualManager.instance.ProcesarEstadoVisual(character, character.vfxEstadoActual, 1f);
            }
        }
    }

    public bool EsCasillaValidaParaAtacar(Vector3Int cell)
    {
        return rangeCells.Contains(cell);
    }

    public void LimpiarRangoAtaque()
    {
        attackOverlay.ClearAllTiles(); // Borra el rastro de casillas rojas
    }

    public float CalcularClase(ClassType atacante, ClassType defensor)
    {
        // Por defecto, el daño es neutral (1.0)
        float multi = 1.0f;

        switch (atacante)
        {
            case ClassType.Guerrero:
                if (defensor == ClassType.Mago) multi = 1.2f; // Guerrero a Mago aumenta precisión
                else if (defensor == ClassType.Tanque) multi = 0.8f; // Guerrero a tanque baja precisión
                break;

            case ClassType.Tanque:
                if (defensor == ClassType.Guerrero) multi = 1.2f; // Tanque a Guerrero aumenta precisión
                else if (defensor == ClassType.Mago) multi = 0.8f; // Tanque a Mago baja precisión
                break;

            case ClassType.Mago:
                if (defensor == ClassType.Tanque) multi = 1.2f; // Mago a Tanque aumenta precisión
                else if (defensor == ClassType.Guerrero) multi = 0.8f; // Mago a Guerrero baja precisión
                break;
        }

        return multi;
    }

    public float CalcularEfectividad(ElementalType atacante, ElementalType defensor)
    {
        // Por defecto, el daño es neutral (1.0)
        float multi = 1.0f;

        switch (atacante)
        {
            case ElementalType.Agua:
                if (defensor == ElementalType.Fuego) multi = 2.0f; // Agua apaga Fuego
                else if (defensor == ElementalType.Toxico) multi = 0.5f; // Agua apaga Fuego
                break;

            case ElementalType.Fuego:
                if (defensor == ElementalType.Planta) multi = 2.0f; // Fuego quema Planta
                else if (defensor == ElementalType.Agua) multi = 0.5f; // Agua apaga Fuego
                break;

            case ElementalType.Planta:
                if (defensor == ElementalType.Tierra) multi = 2.0f; // Planta domina Tierra
                else if (defensor == ElementalType.Fuego) multi = 0.5f; // Agua apaga Fuego
                break;

            case ElementalType.Tierra:
                if (defensor == ElementalType.Electrico) multi = 2.0f; // Electricidad domina Acero
                else if (defensor == ElementalType.Planta) multi = 0.5f; // Agua apaga Fuego
                break;

            case ElementalType.Electrico:
                if (defensor == ElementalType.Acero) multi = 2.0f; // Electricidad domina Acero
                else if (defensor == ElementalType.Tierra) multi = 0.5f; // Agua apaga Fuego
                break;

            case ElementalType.Acero:
                if (defensor == ElementalType.Hielo) multi = 2.0f; // Acero domina Toxico
                else if (defensor == ElementalType.Electrico) multi = 0.5f; // Agua apaga Fuego
                break;

            case ElementalType.Hielo:
                if (defensor == ElementalType.Viento) multi = 2.0f; // Acero domina Toxico
                else if (defensor == ElementalType.Acero) multi = 0.5f; // Agua apaga Fuego
                break;

            case ElementalType.Viento:
                if (defensor == ElementalType.Sonico) multi = 2.0f; // Acero domina Toxico
                else if (defensor == ElementalType.Hielo) multi = 0.5f; // Agua apaga Fuego
                break;

            case ElementalType.Sonico:
                if (defensor == ElementalType.Mente) multi = 2.0f; // Acero domina Toxico
                else if (defensor == ElementalType.Viento) multi = 0.5f; // Agua apaga Fuego
                break;

            case ElementalType.Mente:
                if (defensor == ElementalType.Toxico) multi = 2.0f; // Acero domina Toxico
                else if (defensor == ElementalType.Sonico) multi = 0.5f; // Agua apaga Fuego
                break;

            case ElementalType.Toxico:
                if (defensor == ElementalType.Agua) multi = 2.0f; // Tóxico domina Agua
                else if (defensor == ElementalType.Mente) multi = 0.5f; // Agua apaga Fuego
                break;

                // Si el atacante es Normal, o cualquier otro que no esté aquí, 
                // el multiplicador se queda en 1.0f automáticamente.
        }

        return multi;
    }

    public void AtacarObjetivo(CharacterEntity atacante, CharacterEntity enemigo)
    {
        if (ataqueSeleccionado != null && ataqueSeleccionado.objective == AttackData.Objective.Enemigos && enemigo.equipo == atacante.equipo || ataqueSeleccionado != null && ataqueSeleccionado.objective == AttackData.Objective.Aliados && enemigo.equipo != atacante.equipo)
        {
            Debug.Log("Debes seleccionar a otro personaje de otro equipo");
            return;
        }

        if (ataqueSeleccionado != null && atacante.stats.staminaActual >= ataqueSeleccionado.costeStamina)
        {
            atacante.isActing = true;
            atacante.characterAct = true;
            atacante.characterAttacked = true;

            int attackAccuracy = ataqueSeleccionado.accuracy;
            attackAccuracy = Mathf.CeilToInt(attackAccuracy * CalcularClase(atacante.data.classType, enemigo.data.classType));
            // Precisión
            int azarPrecision = Random.Range(1, 101);
            ataqueActualAcerto = azarPrecision <= attackAccuracy;

            // Se puede poner este if también pero lo de arriba está más simplificado, basicamente mira si se da el caso, si no es false y si si es true
            /*if (azarPrecision <= ataqueSeleccionado.accuracy)
            {
                ataqueActualAcerto = true;
            }*/

            if (ataqueActualAcerto && (ataqueSeleccionado.objective == AttackData.Objective.Enemigos || ataqueSeleccionado.objective == AttackData.Objective.Ambos && atacante.equipo != enemigo.equipo))
            {
                // Crítico
                int azarCritico = Random.Range(1, 101);
                ataqueActualEsCritico = (azarCritico <= ataqueSeleccionado.criticRate);
                // Se puede poner este if también pero lo de arriba está más simplificado, basicamente mira si se da el caso, si no es false y si si es true
                /*if (azarCritico <= ataqueSeleccionado.criticRate)
                {
                    ataqueActualEsCritico = true;
                }*/


                Debug.Log($"{atacante.data.nombre} usa {ataqueSeleccionado.nombre} contra {enemigo.data.nombre}");

                // --- CAMBIO AQUÍ ---
                // Le pasamos el control al AttackVisualManager. Él pausará el tiempo,
                // mostrará la animación y luego ejecutará el daño y llamará a FinalizarLimpiezaDeAtaque.
                if (CombatVisualManager.instance != null)
                {
                    AudioManager.instance.sonidoAtaqueSegunTipo(ataqueSeleccionado.elementalType);
                    CombatVisualManager.instance.ProcesarAtaqueVisual(atacante, enemigo, ataqueSeleccionado);
                }  
            }
            else if (ataqueSeleccionado.objective == AttackData.Objective.Aliados || ataqueSeleccionado.objective == AttackData.Objective.Ambos && atacante.equipo == enemigo.equipo)
            {
                Debug.Log($"{atacante.data.nombre} usa {ataqueSeleccionado.nombre} contra {enemigo.data.nombre}");

                // --- CAMBIO AQUÍ ---
                // Le pasamos el control al AttackVisualManager. Él pausará el tiempo,
                // mostrará la animación y luego ejecutará el daño y llamará a FinalizarLimpiezaDeAtaque.
                if (CombatVisualManager.instance != null)
                {
                    AudioManager.instance.sonidoAtaqueSegunTipo(ataqueSeleccionado.elementalType);
                    CombatVisualManager.instance.ProcesarAtaqueVisual(atacante, enemigo, ataqueSeleccionado);
                }

                Debug.Log("Aqui acierta siempre brrr");
            }
            else
            {
                Debug.Log("<color=red>¡El ataque ha fallado!</color>");

                AudioManager.instance.FallarAtaque();
                CombatVisualManager.instance.MostrarTextoFlotante(enemigo.transform.position, "¡¡Ha fallado!!", Color.cyan);
                // Si falla, no hay animación que esperar, así que limpiamos inmediatamente.
                FinalizarLimpiezaDeAtaque(atacante);
            }
        }
        else
        {
            Debug.Log("No te queda stamina para realizar el ataque!!");
        }

    }

    // --- NUEVO MÉTODO DE LIMPIEZA ---
    // Este método lo llamará el AttackVisualManager cuando la explosión termine.
    public void FinalizarLimpiezaDeAtaque(CharacterEntity atacante)
    {
        // Reducimos la estamina actual por lo que cuesta el ataque (comprobamos que no sea null por si acaso)
        if (ataqueSeleccionado != null)
        {
            atacante.stats.staminaActual -= ataqueSeleccionado.costeStamina;

            if (CombatVisualManager.instance != null)
            {
                CombatVisualManager.instance.MostrarTextoFlotante(atacante.transform.position, $"-{ataqueSeleccionado.costeStamina}", Color.yellow);
            }

            // Miramos si la variable isForcingMovement es falso o true, es para ataques que te muevan hacia el enemigo.
            CharacterMovement mov = atacante.GetComponent<CharacterMovement>();

            // Si el personaje está en medio de un Dash, NO lo liberamos todavía.
            if (mov != null && mov.isForcingMovement)
            {
                Debug.Log("El ataque limpió la UI, pero el personaje sigue moviéndose. isActing se liberará al terminar la embestida.");
            }
            else
            {
                // Comportamiento normal para ataques estáticos
                atacante.isActing = false;
                Debug.Log("Ha dejado de actuar!!!");

                /*if (atacante.estadoActual == CharacterEntity.UnidadEstado.Apuntando)
                {
                    atacante.estadoActual = CharacterEntity.UnidadEstado.EligiendoAccion;
                }*/
            }
        }

        // Opcional: Asegurar que la stamina no sea negativa
        atacante.stats.staminaActual = Mathf.Max(0, atacante.stats.staminaActual);

        // Limpiamos el mapa y el ataque seleccionado (AHORA SÍ, de forma segura)
        ataqueSeleccionado = null;
        attackOverlay.ClearAllTiles();

        if (UIManager.instance != null)
        {
            UIManager.instance.ChangeColorButton();
        }
    }
}