using Unity.VisualScripting;
using UnityEngine;

// Enum para saber qué estadística estamos modificando
public enum StatType
{
    AtaqueFisico, DefensaFisica, AtaqueEspecial, DefensaEspecial, Velocidad
}

public class CharacterEntity : MonoBehaviour
{
    public CharacterData data;

    [Header("Estadísticas de Combate")]
    public Stats stats; // Las estadísticas actuales
    [HideInInspector] public Stats statsBase; // Las originales de respaldo

    [Header("Niveles de Modificación (-6 a +6)")]
    public int nivelAtaqueFisico = 0;
    public int nivelDefensaFisica = 0;
    public int nivelAtaqueEspecial = 0;
    public int nivelDefensaEspecial = 0;
    public int nivelVelocidad = 0;

    [Header("Turnos Restantes de Modificación")]
    public int turnosAtaqueFisico = 0;
    public int turnosDefensaFisica = 0;
    public int turnosAtaqueEspecial = 0;
    public int turnosDefensaEspecial = 0;
    public int turnosVelocidad = 0;

    [Header("Datos de Casilla y Turno")]
    public Vector3Int currentCell;
    public int equipo;
    public SpriteRenderer cambioColorAlActuar;

    public bool isMoving = false;
    public bool characterMoved = false;
    public bool characterAttacked = false;
    public bool characterAct = false;

    [Header("Estados")]
    public int turnosEstadoRestantes = 0;

    public enum UnidadEstado { Reposo, EligiendoAccion, Apuntando, Finalizado }
    public UnidadEstado estadoActual = UnidadEstado.Reposo;
    public StatusType statusType;
    // Esta variable sirve para guardar la animación del estado actual
    [HideInInspector] public GameObject vfxEstadoActual;

    private void Awake()
    {
        cambioColorAlActuar = GetComponent<SpriteRenderer>();

        // 1. Guardamos la base intacta
        statsBase = new Stats()
        {
            vidaMaxima = data.stats.vidaMaxima,
            vidaActual = data.stats.vidaActual,
            staminaMaxima = data.stats.staminaMaxima,
            staminaActual = data.stats.staminaActual,
            ataqueFisico = data.stats.ataqueFisico,
            defensaFisica = data.stats.defensaFisica,
            ataqueEspecial = data.stats.ataqueEspecial,
            defensaEspecial = data.stats.defensaEspecial,
            velocidad = data.stats.velocidad
        };

        // 2. Inicializamos las stats actuales igual que la base
        stats = new Stats()
        {
            vidaMaxima = statsBase.vidaMaxima,
            vidaActual = statsBase.vidaActual,
            staminaMaxima = statsBase.staminaMaxima,
            staminaActual = statsBase.staminaActual,
            ataqueFisico = statsBase.ataqueFisico,
            defensaFisica = statsBase.defensaFisica,
            ataqueEspecial = statsBase.ataqueEspecial,
            defensaEspecial = statsBase.defensaEspecial,
            velocidad = statsBase.velocidad
        };
    }

    // ==========================================
    // NUEVO SISTEMA: RELOJ ÚNICO (Estilo Fire Emblem)
    // ==========================================

    public void AñadirModificadorDeStat(StatType tipo, int cantidadModificacion, int turnos)
    {
        switch (tipo)
        {
            case StatType.AtaqueFisico:
                nivelAtaqueFisico = Mathf.Clamp(nivelAtaqueFisico + cantidadModificacion, -6, 6);
                turnosAtaqueFisico = turnos; // Reiniciamos el reloj
                if (nivelAtaqueFisico == 0) turnosAtaqueFisico = 0; // Si se anula (ej: +1 y luego -1), limpiamos reloj
                break;
            case StatType.DefensaFisica:
                nivelDefensaFisica = Mathf.Clamp(nivelDefensaFisica + cantidadModificacion, -6, 6);
                turnosDefensaFisica = turnos;
                if (nivelDefensaFisica == 0) turnosDefensaFisica = 0;
                break;
            case StatType.AtaqueEspecial:
                nivelAtaqueEspecial = Mathf.Clamp(nivelAtaqueEspecial + cantidadModificacion, -6, 6);
                turnosAtaqueEspecial = turnos;
                if (nivelAtaqueEspecial == 0) turnosAtaqueEspecial = 0;
                break;
            case StatType.DefensaEspecial:
                nivelDefensaEspecial = Mathf.Clamp(nivelDefensaEspecial + cantidadModificacion, -6, 6);
                turnosDefensaEspecial = turnos;
                if (nivelDefensaEspecial == 0) turnosDefensaEspecial = 0;
                break;
            case StatType.Velocidad:
                nivelVelocidad = Mathf.Clamp(nivelVelocidad + cantidadModificacion, -6, 6);
                turnosVelocidad = turnos;
                if (nivelVelocidad == 0) turnosVelocidad = 0;
                break;
        }

        RecalcularEstadisticas();
    }

    public void ActualizarTurnosModificadores()
    {
        bool huboCambios = false;

        // Reducimos los relojes que estén activos. Si llegan a 0, la stat vuelve a su estado base (Nivel 0)
        if (turnosAtaqueFisico > 0) { turnosAtaqueFisico--; if (turnosAtaqueFisico == 0) { nivelAtaqueFisico = 0; huboCambios = true; } }
        if (turnosDefensaFisica > 0) { turnosDefensaFisica--; if (turnosDefensaFisica == 0) { nivelDefensaFisica = 0; huboCambios = true; } }
        if (turnosAtaqueEspecial > 0) { turnosAtaqueEspecial--; if (turnosAtaqueEspecial == 0) { nivelAtaqueEspecial = 0; huboCambios = true; } }
        if (turnosDefensaEspecial > 0) { turnosDefensaEspecial--; if (turnosDefensaEspecial == 0) { nivelDefensaEspecial = 0; huboCambios = true; } }
        if (turnosVelocidad > 0) { turnosVelocidad--; if (turnosVelocidad == 0) { nivelVelocidad = 0; huboCambios = true; } }

        if (huboCambios)
        {
            RecalcularEstadisticas();
        }
    }

    public void RecalcularEstadisticas()
    {
        // 1. Calculamos y aplicamos la estadística final usando la base multiplicada por la fase
        stats.ataqueFisico = Mathf.Max(1, Mathf.FloorToInt(statsBase.ataqueFisico * CalcularMultiplicadorFase(nivelAtaqueFisico)));
        stats.defensaFisica = Mathf.Max(1, Mathf.FloorToInt(statsBase.defensaFisica * CalcularMultiplicadorFase(nivelDefensaFisica)));
        stats.ataqueEspecial = Mathf.Max(1, Mathf.FloorToInt(statsBase.ataqueEspecial * CalcularMultiplicadorFase(nivelAtaqueEspecial)));
        stats.defensaEspecial = Mathf.Max(1, Mathf.FloorToInt(statsBase.defensaEspecial * CalcularMultiplicadorFase(nivelDefensaEspecial)));
        stats.velocidad = Mathf.Max(1, Mathf.FloorToInt(statsBase.velocidad * CalcularMultiplicadorFase(nivelVelocidad)));

        // 2. Por último, aplicamos penalizaciones porcentuales de Estados alterados (si existen)
        if (statusType == StatusType.Lentitud)
        {
            stats.velocidad = Mathf.FloorToInt(stats.velocidad * 0.5f);
        }
        else if (statusType == StatusType.Frio)
        {
            stats.ataqueEspecial = Mathf.FloorToInt(stats.ataqueEspecial * 0.5f);
        }
    }

    private float CalcularMultiplicadorFase(int nivel)
    {
        if (nivel >= 0)
        {
            return (2f + nivel) / 2f;
        }
        else
        {
            return 2f / (2f + Mathf.Abs(nivel));
        }
    }

    // ==========================================
    // SISTEMA DE ESTADOS, COMBATE Y MOVIMIENTO
    // ==========================================

    public void LimpiarEstado()
    {
        vfxEstadoActual = null;
        statusType = StatusType.Neutro;
        turnosEstadoRestantes = 0;

        RecalcularEstadisticas();
        Debug.Log(data.nombre + " ha limpiado sus estados.");
    }

    public void RecuperarStamina(CharacterEntity character)
    {
        int incremento = character.characterAct ?
            Mathf.CeilToInt(character.stats.staminaMaxima * 0.25f) :
            Mathf.CeilToInt(character.stats.staminaMaxima * 0.35f);

        character.stats.staminaActual = Mathf.Min(character.stats.staminaActual + incremento, character.stats.staminaMaxima);
        Debug.Log(character.data.nombre + " ha recuperado: " + incremento + " de estamina");
    }

    public void RecibirCuracion(int curacion)
    {
        stats.vidaActual += curacion;
        stats.vidaActual = Mathf.Min(stats.vidaMaxima, stats.vidaActual);
        Debug.Log(this.data.nombre + " ha recuperado " + curacion + " puntos de vida.");
    }

    public void RecibirStamina(int stamina)
    {
        stats.staminaActual += stamina;
        stats.staminaActual = Mathf.Min(stats.staminaMaxima, stats.staminaActual);
        Debug.Log(this.data.nombre + " ha recuperado " + stamina + " puntos de estamina.");
    }

    public void PerderStamina(int stamina)
    {
        stats.staminaActual -= stamina;
        stats.staminaActual = Mathf.Max(0, stats.staminaActual);
        Debug.Log(this.data.nombre + " ha recuperado " + stamina + " puntos de estamina.");
    }

    public void RecibirDaño(int daño)
    {
        stats.vidaActual -= daño;
        stats.vidaActual = Mathf.Max(0, stats.vidaActual);
        Debug.Log(this.data.nombre + " ha perdido " + daño + " puntos de vida.");

        if (this.stats.vidaActual <= 0)
        {
            Die();
        }
    }

    public (int costeMovimiento, int costeEstamina) CalcularCosteMovimiento(CellData cellData)
    {
        int costeMov = cellData.costeMovimiento;
        int costeEst = 0;

        switch (data.tipoMovilidad)
        {
            case TipoMovilidad.Terrestre:
                if (cellData.tipoTerreno == TipoTerreno.Agua) { costeMov += 20; costeEst += 15; }
                else if (cellData.tipoTerreno == TipoTerreno.Rocoso) { costeMov += 10; costeEst += 10; }
                else if (cellData.tipoTerreno == TipoTerreno.Tierra) { costeMov += 5; costeEst += 5; }
                break;
            case TipoMovilidad.Acuatico:
                if (cellData.tipoTerreno == TipoTerreno.Agua) { costeMov += 5; costeEst += 5; }
                else if (cellData.tipoTerreno == TipoTerreno.Rocoso) { costeMov += 20; costeEst += 15; }
                else if (cellData.tipoTerreno == TipoTerreno.Tierra) { costeMov += 10; costeEst += 10; }
                break;
            case TipoMovilidad.Volador:
                costeMov += 10; costeEst += 10;
                break;
        }
        return (Mathf.Max(1, costeMov), Mathf.Max(0, costeEst));
    }

    public void Die()
    {
        // 1. Liberamos lógicamente la casilla en el Grid antes de morir
        Node nodo = GridManager.instance.GetNode(currentCell);
        if (nodo != null && nodo.unitPresent == this)
        {
            nodo.unitPresent = null;
        }

        // 2. Continuamos con tu lógica original
        GameManager.instance.DeathManagment(this);
        Destroy(this.gameObject);
    }
}