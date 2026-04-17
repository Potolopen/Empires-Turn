using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System;

public class CharacterMovement : MonoBehaviour
{
    private static CharacterMovement currentlySelected; // Referencia al personaje actualmente seleccionado, solo puede haber uno a la vez

    public CharacterEntity character; // Referencia al CharacterEntity que controla stats, celda actual y demás datos
    public Tilemap movementOverlay;   // Tilemap donde se pintarán las casillas de movimiento
    public Tile overlayMoveTile;      // Tile azul transparente usado para indicar casillas alcanzables

    public bool seleccionado = false; // Boleana para saber si este personaje está seleccionado
    private List<Vector3Int> reachableCells; // Lista de casillas que el personaje puede alcanzar
    private Dictionary<Vector3Int, Vector3Int> rutaCalculada = new Dictionary<Vector3Int, Vector3Int>(); // Diccionario usado en BFS para generar rutas
    private Vector3Int startCell; // Celda inicial del personaje para poder retroceder si se cancela el movimiento
    private int startStamina; // Variable que contiene el valor inicial de la estamina antes de moverse, para poder retroceder y recuperar lo perdido.
    private StatusType startStatus;

    private bool startAct; // Para mirar si ya habia actuado, esto servirá para que cuando retrocedas tu movimiento, si no hiciste nada antes, characterAct se desactive, si
    //hiciste algo antes del mov, y retrocedes no se desactivará.

    // Array con las cuatro direcciones cardinales, usado para BFS
    static readonly Vector3Int[] directions = {
        Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
    };

    // Clase interna para BFS, contiene la celda y el movimiento restante al llegar a ella
    public class BFSNode
    {
        public Vector3Int cell;
        public int movimientoRestante;
        public BFSNode(Vector3Int c, int mov)
        {
            cell = c;
            movimientoRestante = mov;
        }
    }

    // Start se ejecuta una vez al iniciar la escena
    void Start()
    {
        // Si alguna referencia es nula, buscamos automáticamente el objeto correspondiente en la escena

        if (character == null)
            character = GetComponent<CharacterEntity>();

        // Buscamos automáticamente el overlay de movimiento en la escena
        GameObject overlayGO = GameObject.Find("MovementOverlay");
        if (overlayGO != null)
        {
            movementOverlay = overlayGO.GetComponent<Tilemap>();
        }

        // Convertimos la posición del personaje en el mundo a la celda del Grid
        character.currentCell = GridManager.instance.tilemap.WorldToCell(character.transform.position);
        // Ajustamos la posición del personaje al centro exacto de la celda para evitar saltos visuales
        character.transform.position = GridManager.instance.tilemap.GetCellCenterWorld(character.currentCell);

        // Marcamos la celda inicial como ocupada por el personaje
        AsignarCasilla(character.currentCell);

        // Guardamos la celda inicial para poder retroceder si se cancela el movimiento
        startCell = character.currentCell;
    }

    // Update se ejecuta cada frame
    void Update()
    {
        // Detecta clic izquierdo del ratón PRIMERO. 
        // Si no hay clic, el Update termina aquí en 0.0001 milisegundos.
        if (Input.GetMouseButtonDown(0))
        {
            // Si clicamos en la UI, ignoramos
            if (EventSystem.current.IsPointerOverGameObject()) return;

            // AHORA SÍ calculamos dónde está el ratón, porque hemos hecho clic
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mouseWorld.x, mouseWorld.y);
            Vector3Int celda = GridManager.instance.GetCeldaBajoMouse();

            // Si clicamos sobre el boxCollider2D del personaje y no estaba seleccionado
            if (character.GetComponent<BoxCollider2D>().OverlapPoint(mousePos2D) && !seleccionado)
            {
                // 1. Evitar interrumpir al personaje actual si está en medio de una acción importante
                if (currentlySelected != null && currentlySelected != this)
                {
                    // Si el personaje actual se está moviendo, está apuntando, o YA SE MOVIÓ y no ha terminado su turno...
                    if (currentlySelected.character.isMoving ||
                        currentlySelected.character.estadoActual == CharacterEntity.UnidadEstado.Apuntando ||
                        currentlySelected.character.characterMoved) // <-- ESTA ES LA CLAVE
                    {
                        // Ignoramos el clic por completo. El jugador debe usar "Atrás" o finalizar el turno actual.
                        return;
                    }

                    // Si el personaje actual NO ha hecho nada de lo anterior, lo podemos deseleccionar tranquilamente
                    currentlySelected.DeseleccionarPersonaje();
                }

                // 2. Asignar el nuevo personaje seleccionado
                currentlySelected = this;
                seleccionado = true;

                // 3. Evaluar si este personaje PUEDE actuar
                bool puedeActuar = (TurnManager.instance.turnoActual == character.equipo) &&
                                   (character.estadoActual != CharacterEntity.UnidadEstado.Finalizado);

                if (puedeActuar)
                {
                    character.estadoActual = CharacterEntity.UnidadEstado.EligiendoAccion;
                    reachableCells = GetReachableCells();
                    PintarCasillasAlcanzables();
                }

                // 4. Mostrar la interfaz
                UIManager.instance.ShowActionMenu(character, currentlySelected);
            }
            else if (seleccionado) // En caso de estar seleccionado
            {
                // CASO A: El personaje está en modo ATAQUE (Casillas rojas)
                if (character.estadoActual == CharacterEntity.UnidadEstado.Apuntando)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        //Vector3Int celdaClicada = GetCeldaBajoMouse();
                        Node nodo = GridManager.instance.GetNode(celda);

                        // Si la celda clicada está dentro de las alcanzables
                        if (CombatManager.instance.EsCasillaValidaParaAtacar(celda) && (nodo != null && nodo.unitPresent != null))
                        {
                            // --- EL ESCUDO DE ESTAMINA ---
                            // Solo procesamos el ataque y el cambio de turnos SI tienes estamina
                            if (character.stats.staminaActual >= CombatManager.instance.ataqueSeleccionado.costeStamina)
                            {
                                //character.characterAct = true;
                                //character.characterAttacked = true;
                                CombatManager.instance.AtacarObjetivo(this.GetComponent<CharacterEntity>(), nodo.unitPresent);
                            }
                        }
                    }
                }
                // CASO B: El personaje está en modo MOVIMIENTO (Casillas azules)
                // Este es tu código original, pero lo protegemos con el estado
                else if (character.estadoActual == CharacterEntity.UnidadEstado.EligiendoAccion)
                {
                    // Si clicamos sobre la misma celda donde está el personaje
                    if (celda.Equals(character.currentCell) && !character.characterMoved)
                    {
                        character.estadoActual = CharacterEntity.UnidadEstado.Reposo;
                        // Limpiamos overlay y deseleccionamos
                        DeseleccionarPersonaje();

                        // Desactivamos la UI de acciones
                        UIManager.instance.HideActionMenu();
                        return;
                    }

                    // Si la celda clicada está dentro de las alcanzables
                    if (EsCasillaValida(celda) && !character.characterMoved)
                    {
                        currentlySelected.character.characterAttacked = false;
                        character.isMoving = true; // Marcamos que se está moviendo
                        character.characterMoved = true;
                        StartCoroutine(MoverHaciaCelda(celda)); // Iniciamos movimiento
                    }
                }
            }
        }
        if (Input.GetMouseButtonDown(1) && currentlySelected != null)
        {
            // 🚨 LA CLAVE: Solo dejamos que el personaje actualmente seleccionado maneje este bloque.
            // Así evitamos que los otros 10 personajes del mapa intenten deseleccionarse a la vez.
            if (currentlySelected != this) return;

            // 1. Cancelar apuntado siempre es posible
            if (character.estadoActual == CharacterEntity.UnidadEstado.Apuntando)
            {
                UIManager.instance.OnClickCancelar();
            }
            // 2. Retroceder movimiento: solo si el personaje no ha atacado.
            else if (character.characterMoved && !character.isMoving && !character.characterAttacked)
            {
                UIManager.instance.OnClickAtras();
                UIManager.instance.ShowActionMenu(character, this);
            }
            // 3. Deseleccionar general:
            // Permite deseleccionar si no se ha movido AÚN, o si ya está FINALIZADO (aliado o enemigo)
            else if (!character.characterMoved || character.estadoActual == CharacterEntity.UnidadEstado.Finalizado)
            {
                DeseleccionarPersonaje();
                UIManager.instance.HideActionMenu();
            }
        }
    }

    // Coroutina para mover al personaje de forma suave
    IEnumerator MoverHaciaCelda(Vector3Int destino)
    {
        startCell = character.currentCell; // Guardamos la celda de inicio
        startStamina = character.stats.staminaActual; // Guardamos la stamina antes de movernos
        startStatus = character.statusType; // Guardamos el estado actual, por si al movernos, se modifica el estado
        startAct = character.characterAct; // Guardamos si ha actuado o no, para saber si al retroceder se puede desactivar o no characterAct
        LiberarCasilla(character.currentCell); // Liberamos la celda actual
        //movementOverlay.ClearAllTiles(); // Limpiamos overlay
        //DeseleccionarPersonaje(); // Mientras se mueve, no está seleccionado

        List<Vector3Int> ruta = ObtenerRuta(character.currentCell, destino); // Obtenemos ruta BFS

        // Movemos paso a paso por la ruta
        foreach (var celda in ruta)
        {
            // --- AQUÍ CALCULAMOS Y RESTAMOS LA ESTAMINA ---
            CellData dataCasilla = GridManager.instance.GetCellData(celda);
            if (dataCasilla != null)
            {
                // Usamos la tupla para obtener el coste de estamina de esta casilla concreta
                var (_, costeEstamina) = character.CalcularCosteMovimiento(dataCasilla);

                // Restamos y nos aseguramos de no bajar de 0
                character.stats.staminaActual = Mathf.Max(0, character.stats.staminaActual - costeEstamina);

                Debug.Log($"Pisando {celda}. Gasto: {costeEstamina}. Estamina restante: {character.stats.staminaActual}");
            }

            Vector3 objetivo = GridManager.instance.tilemap.GetCellCenterWorld(celda);
            while ((character.transform.position - objetivo).sqrMagnitude > 0.01f)
            {
                // Movimiento suave con velocidad constante
                character.transform.position = Vector3.MoveTowards(character.transform.position, objetivo, 10f * Time.deltaTime);
                yield return null;
            }
            character.transform.position = objetivo; // Aseguramos posición exacta
            character.currentCell = celda; // Actualizamos celda lógica
        }

        // Asignamos la nueva celda al personaje
        AsignarCasilla(character.currentCell);

        // Fin de movimiento
        character.isMoving = false;
        character.characterAct = true;
    }

    // Función para mover el personaje de vuelta a la posición inicial
    public void MoverPosicionAnterior()
    {
        character.transform.position = GridManager.instance.tilemap.GetCellCenterWorld(startCell);
        LiberarCasilla(character.currentCell);
        AsignarCasilla(startCell);
        character.stats.staminaActual = startStamina;
        character.statusType = startStatus;
        character.characterAct = startAct;
        seleccionado = true;
        character.characterMoved = false;
    }

    // Devuelve true si la celda está en la lista de alcanzables
    bool EsCasillaValida(Vector3Int cell)
    {
        return reachableCells.Contains(cell);
    }

    // Reconstruye la ruta desde BFS
    List<Vector3Int> ObtenerRuta(Vector3Int start, Vector3Int destino)
    {
        List<Vector3Int> ruta = new List<Vector3Int>();
        if (!rutaCalculada.ContainsKey(destino)) return ruta;

        Vector3Int temp = destino;
        while (temp != start)
        {
            ruta.Add(temp);
            temp = rutaCalculada[temp];
        }
        ruta.Reverse(); // De inicio a destino
        return ruta;
    }

    // BFS para obtener todas las casillas alcanzables
    public List<Vector3Int> GetReachableCells()
    {
        List<Vector3Int> reachable = new List<Vector3Int>();
        Queue<BFSNode> queue = new Queue<BFSNode>();
        rutaCalculada.Clear(); // Limpiamos ruta previa

        queue.Enqueue(new BFSNode(character.currentCell, character.stats.velocidad));
        rutaCalculada[character.currentCell] = character.currentCell;

        Dictionary<Vector3Int, int> visited = new Dictionary<Vector3Int, int>();

        while (queue.Count > 0)
        {
            BFSNode node = queue.Dequeue();
            Vector3Int cell = node.cell;
            int movRestante = node.movimientoRestante;

            // Si ya visitamos con más movimiento, ignoramos
            if (visited.ContainsKey(cell) && visited[cell] >= movRestante)
                continue;

            visited[cell] = movRestante;
            Node nodoActual = GridManager.instance.GetNode(cell);
            bool estaOcupada = nodoActual != null && nodoActual.unitPresent != null;

            // Añadimos la celda como alcanzable si no está ocupada o es la actual
            if (!estaOcupada && cell != character.currentCell)
            {
                reachable.Add(cell);
            }

            // Revisamos vecinos cardinales
            foreach (Vector3Int dir in directions)
            {
                Vector3Int vecino = cell + dir;
                CellData cellData = GridManager.instance.GetCellData(vecino);
                if (cellData == null) continue;
                if (!PuedeAtravesar(vecino)) continue;

                var (costeMovimiento, costeEstamina) = character.CalcularCosteMovimiento(cellData); // Calculamos coste según terreno y tipo de movimiento
                int nuevoMov = movRestante - costeMovimiento;
                if (nuevoMov >= 0)
                {
                    if (!rutaCalculada.ContainsKey(vecino))
                    {
                        rutaCalculada[vecino] = cell;
                    }
                    queue.Enqueue(new BFSNode(vecino, nuevoMov));
                }
            }
        }
        return reachable;
    }

    // Pinta las casillas alcanzables en el overlay
    public void PintarCasillasAlcanzables()
    {
        movementOverlay.ClearAllTiles();
        foreach (var cell in reachableCells)
            movementOverlay.SetTile(cell, overlayMoveTile);
    }

    // Marca la celda como ocupada por el personaje
    private void AsignarCasilla(Vector3Int cell)
    {
        Node nodo = GridManager.instance.GetNode(cell);
        if (nodo != null)
        {
            nodo.unitPresent = character;
            character.currentCell = cell;
        }
    }

    // Libera la celda si estaba ocupada por este personaje
    private void LiberarCasilla(Vector3Int cell)
    {
        Node nodo = GridManager.instance.GetNode(cell);
        if (nodo != null && nodo.unitPresent == character)
            nodo.unitPresent = null;
    }

    // Devuelve true si la celda es transitable por el personaje (aliado sí, enemigo no)
    public bool PuedeAtravesar(Vector3Int cell)
    {
        Node nodo = GridManager.instance.GetNode(cell);
        if (nodo == null || nodo.unitPresent == null) return true;
        if (nodo.unitPresent.equipo == character.equipo) return true;
        return false;
    }

    // Deselecciona el personaje y limpia overlay
    public void DeseleccionarPersonaje()
    {
        seleccionado = false;
        movementOverlay.ClearAllTiles();
        if (currentlySelected.character.estadoActual != CharacterEntity.UnidadEstado.Finalizado)
        {
            currentlySelected.character.estadoActual = CharacterEntity.UnidadEstado.Reposo;
        }
        currentlySelected = null;
    }

    // Método para refrescar la cuadrícula dependiendo de si ya nos hemos movido o no
    public void RefrescarCuadriculaMovimiento()
    {
        if (!character.characterMoved)
        {
            // 1. Aún no se ha movido: Calculamos las rutas desde cero y pintamos.
            reachableCells = GetReachableCells();
            PintarCasillasAlcanzables();
        }
        else if (character.characterMoved && !character.characterAttacked)
        {
            // 2. Ya se movió, PERO NO ha atacado.
            // Como NO llamamos a GetReachableCells(), la variable 'reachableCells' 
            // sigue teniendo memorizadas las casillas de la posición antigua.
            // Simplemente las volvemos a dibujar en pantalla.
            PintarCasillasAlcanzables();
        }
        else
        {
            // 3. Ya se movió y ya atacó. Aquí sí limpiamos la pantalla.
            movementOverlay.ClearAllTiles();
        }
    }
}
