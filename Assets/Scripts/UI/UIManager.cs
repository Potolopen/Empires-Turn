using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TextCore.Text;
using NUnit.Framework;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [System.Serializable]
    public struct StatusMapping
    {
        public StatusType tipo; // Tu Enum (Quemadura, Frio, etc.)
        public Sprite icono;    // El sprite que quieres mostrar
    }

    [Header("Estadisticas")]
    [SerializeField] private TextMeshProUGUI statAttack;
    [SerializeField] private TextMeshProUGUI statDefense;
    [SerializeField] private TextMeshProUGUI statSpA;
    [SerializeField] private TextMeshProUGUI statSpD;
    [SerializeField] private TextMeshProUGUI statVelocity;

    [Header("Info de Ataques")]
    [SerializeField] private TextMeshProUGUI attackName;
    [SerializeField] private TextMeshProUGUI attackPower;
    [SerializeField] private TextMeshProUGUI attackStamina;
    [SerializeField] private TextMeshProUGUI attackAccurate;
    [SerializeField] private TextMeshProUGUI attackType;
    [SerializeField] private TextMeshProUGUI attackCritic;
    [SerializeField] private TextMeshProUGUI attackCategory;
    [SerializeField] private TextMeshProUGUI attackObjectives;
    [SerializeField] private TextMeshProUGUI attackRange;
    [SerializeField] private TextMeshProUGUI attackDescription;

    [Header("Barras de UI")]
    [SerializeField] private Image healthBarFill;  // Arrastra aquí el objeto 'HealthBar'
    [SerializeField] private Image staminaBarFill; // Arrastra aquí el objeto 'StaminaBar'
    [SerializeField] private TextMeshProUGUI healthValue;
    [SerializeField] private TextMeshProUGUI staminaValue;

    [Header("Badges")]
    [SerializeField] private Image elementalBadge;  // Arrastramos la imagen donde irá la medalla del tipo
    [SerializeField] private Image classBadge; // Arrastramos la imagen donde irá la medalla de clase
    [SerializeField] private Image movilityBadge;  // Arrastramos la imagen donde irá la medalla de movilidad
    [SerializeField] private Image statusBadge; // El objeto Image en la UI
    [SerializeField] private Image characterFace; // El objeto donde aparecerá la cara del personaje
    [SerializeField] private List<StatusMapping> bibliotecaEstados; // La lista que rellenarás en el Inspector

    private CharacterEntity selectedCharacter;
    private CharacterMovement selectedCharacterMovement;
    public AttackData firstAttack, secondAttack, thirdAttack;

    [SerializeField] public TextMeshProUGUI infoTerrain;

    [SerializeField] private Button firstAttackButton, secondAttackButton, thirdAttackButton, attackButton;
    [SerializeField] private GameObject actionMenu, attackMenu, buttonsPanel, attackInfoPanel, infoTerrainPanel, finalizarPanel, surrenderButton, continueButton;

    public TextMeshProUGUI textoMenu; // Es para poner el texto que debe salir cuando la partida finalice.

    [SerializeField] private float suavizadoBarra = 5f; // Velocidad de la animación de la barra

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
    void Update()
    {
        // Si hay un personaje seleccionado, actualizamos sus barras constantemente
        // para que se vea el cambio mientras se mueve o recibe daño
        if (selectedCharacter != null)
        {
            ActualizarBarrasUI();
            ActualizarTextosStats();
        }
    }
    // Metodo que inicia el menu de acción, teniendo en cuenta el personaje. Este menu se iniciará cuando el personaje sea seleccionado.
    public void ShowActionMenu(CharacterEntity character, CharacterMovement characterMovement)
    {
        // Se seleccionará el personaje actual
        selectedCharacter = character;

        selectedCharacterMovement = characterMovement;

        // Accedemos a las stats del personaje, comprobamos colores y turnos restantes
        ConfigurarStatUI(statAttack, "Ataque", character.stats.ataqueFisico, character.statsBase.ataqueFisico, character.turnosAtaqueFisico, character.nivelAtaqueFisico);
        ConfigurarStatUI(statDefense, "Defensa", character.stats.defensaFisica, character.statsBase.defensaFisica, character.turnosDefensaFisica, character.nivelDefensaFisica);
        ConfigurarStatUI(statSpA, "SpA", character.stats.ataqueEspecial, character.statsBase.ataqueEspecial, character.turnosAtaqueEspecial, character.nivelAtaqueEspecial);
        ConfigurarStatUI(statSpD, "SpD", character.stats.defensaEspecial, character.statsBase.defensaEspecial, character.turnosDefensaEspecial, character.nivelDefensaEspecial);
        ConfigurarStatUI(statVelocity, "Velocidad", character.stats.velocidad, character.statsBase.velocidad, character.turnosVelocidad, character.nivelVelocidad);

        // Accedemos a los sprites desde el ScriptableObject (data)
        elementalBadge.sprite = character.data.iconoTipo;
        classBadge.sprite = character.data.iconoClase;
        movilityBadge.sprite = character.data.iconoMovilidad;
        characterFace.sprite = character.data.characterFace;

        ActualizarBadgeEstado();

        // Forzamos que las barras aparezcan en su valor real al instante al seleccionar
        healthBarFill.fillAmount = character.stats.vidaActual / (float)character.stats.vidaMaxima;
        staminaBarFill.fillAmount = character.stats.staminaActual / (float)character.stats.staminaMaxima;

        // Evaluamos si el personaje seleccionado puede interactuar
        bool puedeActuar = (TurnManager.instance.turnoActual == character.equipo) &&
                           (character.estadoActual != CharacterEntity.UnidadEstado.Finalizado);
        // Activamos o desactivamos los botones según si puede actuar
        buttonsPanel.SetActive(puedeActuar);

        // Mostramos siempre el panel principal de información
        actionMenu.SetActive(true);
    }

    private void ConfigurarStatUI(TextMeshProUGUI textoStat, string nombreStat, int valorActual, int valorBase, int turnosRestantes, int nivelMod)
    {
        // 1. Asignamos el texto base
        textoStat.text = $"{nombreStat}: {valorActual}";

        // 2. Comparamos para elegir el color del número
        if (valorActual > valorBase)
        {
            textoStat.color = Color.green;
        }
        else if (valorActual < valorBase)
        {
            textoStat.color = Color.cyan;
        }
        else
        {
            textoStat.color = Color.white;
        }

        // 3. Añadimos el indicador de Nivel (si es distinto de 0)
        if (nivelMod > 0)
        {
            // Bufo: Ponemos ^ y el nivel
            textoStat.text += $" <size=80%>(^1)</size>";
        }
        else if (nivelMod < 0)
        {
            // Debufo: Ponemos v y el nivel en positivo (Mathf.Abs) para que no salga v-1
            textoStat.text += $" <size=80%>(v1)</size>";
        }

        // 4. Añadimos el Reloj de Turnos al final (en amarillo)
        if (turnosRestantes > 0)
        {
            textoStat.text += $" <color=#FFFF00><size=80%>[T:{turnosRestantes}]</size></color>";
        }
    }

    public void ActualizarBadgeEstado()
    {
        if (selectedCharacter == null) return;

        // Buscamos el sprite en la biblioteca (incluso si es Neutro)
        Sprite spriteEncontrado = GetStatusSprite(selectedCharacter.statusType);

        if (spriteEncontrado != null)
        {
            statusBadge.sprite = spriteEncontrado;
        }
    }

    // Función auxiliar para buscar el sprite en la lista
    private Sprite GetStatusSprite(StatusType tipo)
    {
        foreach (var mapping in bibliotecaEstados)
        {
            if (mapping.tipo == tipo)
            {
                return mapping.icono;
            }
        }
        return null; // Si no lo encuentra
    }

    public bool IsAMenuOpen() 
    {
        return actionMenu.activeSelf;
    }

    // Metodo para esconder el menu de accion
    public void HideActionMenu()
    {
        // Se deseleccionará el personaje.
        selectedCharacter = null;
        // Desactivaremos el menu.
        actionMenu.SetActive(false);
        buttonsPanel.SetActive(false);
    }

    // Metodo de cuando se clica el boton Atras del menu accion.
    public void OnClickAtras()
    {
        selectedCharacterMovement.ClickDerechoAccion();
    }
    
    // Metodo para el boton de aceptar, simplemente hace desaparecer el menu y deja al personaje donde está
    public void OnClickAceptar()
    {
        // Miramos si el personaje seleccionado no es nulo.
        if (selectedCharacter != null && !selectedCharacter.isActing)
        {
            selectedCharacter.estadoActual = CharacterEntity.UnidadEstado.Finalizado;
            TurnManager.instance.TerminarTurno(selectedCharacter);
            HideAttackMenu();
            selectedCharacterMovement.DeseleccionarPersonaje();
            
            // Escondemos el menu llamando a este metodo.
            HideActionMenu();
        }
    }

    public void OnClickAtacar()
    {
        if (!selectedCharacter.isActing)
        {
            selectedCharacterMovement.character.estadoActual = CharacterEntity.UnidadEstado.Apuntando;
            selectedCharacterMovement.movementOverlay.ClearAllTiles();
            //HideActionMenu();
            attackButton.gameObject.SetActive(false);
            ShowAttackMenu();
        }
    }

    // Metodo para el boton de Atacar, ahora no hace nada XD
    public void ShowAttackMenu()
    {
        //Aquí deben de salir el menu de los ataques
        attackMenu.SetActive(true);
        attackInfoPanel.SetActive(true);

        AttackData[] attacks = selectedCharacter.data.attacks;

        // Asignamos ataques a las variables
        firstAttack = attacks.Length > 0 ? attacks[0] : null;
        secondAttack = attacks.Length > 1 ? attacks[1] : null;
        thirdAttack = attacks.Length > 2 ? attacks[2] : null;

        // Actualizamos los textos de los botones
        firstAttackButton.GetComponentInChildren<TMP_Text>().text = firstAttack != null ? firstAttack.nombre : "";
        secondAttackButton.GetComponentInChildren<TMP_Text>().text = secondAttack != null ? secondAttack.nombre : "";
        thirdAttackButton.GetComponentInChildren<TMP_Text>().text = thirdAttack != null ? thirdAttack.nombre : "";

        // Asignamos colores dependiendo del tipo del ataque.
        ChangeColorButton();
    }

    // Función auxiliar que recibe el tipo elemental y devuelve un color
    private Color ObtenerColorPorElemento(ElementalType tipo)
    {
        switch (tipo)
        {
            case ElementalType.Agua:
                return new Color(0.4f, 0.6f, 1f); // Azul suave
            case ElementalType.Fuego:
                return new Color(1f, 0.4f, 0.4f); // Rojo clarito/suave
            case ElementalType.Planta:
                return new Color(0.4f, 0.9f, 0.4f); // Verde
            case ElementalType.Tierra:
                return new Color(0.6f, 0.4f, 0.2f); // Color caca
            case ElementalType.Electrico:
                return Color.yellow; // Amarillo
            case ElementalType.Acero:
                return Color.gray; // Gris
            case ElementalType.Hielo:
                return new Color(0.6f, 0.8f, 1.0f); // Azul claro
            case ElementalType.Viento:
                return new Color(0.4f, 1.0f, 0.8f); // Verde agua
            case ElementalType.Sonico:
                return new Color(1.0f, 0.95f, 0.7f); // Amarillento
            case ElementalType.Mente:
                return new Color(1.0f, 0.5f, 0.8f); // Rosado
            case ElementalType.Toxico:
                return new Color(0.7f, 0.3f, 0.9f); // Morado
            default:
                return Color.white; // Color normal por defecto
        }
    }

    public void ChangeColorButton()
    {
        // --- EVALUAR PRIMER ATAQUE ---
        if (firstAttack != null)
        {
            // 1. Obtenemos el color base según su elemento
            Color colorAtaque = ObtenerColorPorElemento(firstAttack.elementalType);

            // 2. Comprobamos la estamina
            if (firstAttack.costeStamina > selectedCharacter.stats.staminaActual)
            {
                // Le bajamos la opacidad (Alpha) al 40% para que se vea transparente
                colorAtaque.a = 0.4f;
            }
            else
            {
                // Opacidad al 100%
                colorAtaque.a = 1.0f;
            }

            // 3. Aplicamos el color final a la imagen
            firstAttackButton.image.color = colorAtaque;
        }

        // --- EVALUAR SEGUNDO ATAQUE ---
        if (secondAttack != null)
        {
            Color colorAtaque = ObtenerColorPorElemento(secondAttack.elementalType);

            if (secondAttack.costeStamina > selectedCharacter.stats.staminaActual)
            {
                colorAtaque.a = 0.4f;
            }
            else
            {
                colorAtaque.a = 1.0f;
            }

            secondAttackButton.image.color = colorAtaque;
        }

        // --- EVALUAR TERCER ATAQUE ---
        if (thirdAttack != null)
        {
            Color colorAtaque = ObtenerColorPorElemento(thirdAttack.elementalType);

            if (thirdAttack.costeStamina > selectedCharacter.stats.staminaActual)
            {
                colorAtaque.a = 0.4f;
            }
            else
            {
                colorAtaque.a = 1.0f;
            }

            thirdAttackButton.image.color = colorAtaque;
        }
    }

    private string ObtenerPotenciaAtaque(AttackData ataque)
    {
        // Si no tiene efectos, devolvemos un guion
        if (ataque.efectos == null || ataque.efectos.Length == 0) return "-";

        foreach (AttackEffect efecto in ataque.efectos)
        {
            // Comprobamos si este efecto en concreto es el de daño
            // ¡OJO! Cambia 'DamageEffect' por el nombre real de tu script de efecto de daño
            if (efecto is DamageEffect efectoDeDaño)
            {
                // Cambia 'potencia' por el nombre de tu variable de daño
                return efectoDeDaño.daño.ToString();
            }
        }

        // Si termina el bucle y no encontró ningún efecto de daño (ej: es un ataque de curar)
        return "-";
    }

    private void MostrarInfoAtaque(AttackData ataque)
    {
        if (ataque == null) return;

        attackName.text = ataque.nombre;
        attackPower.text = ObtenerPotenciaAtaque(ataque);
        attackStamina.text = ataque.costeStamina.ToString();
        attackAccurate.text = ataque.accuracy.ToString() + "%";
        attackType.text = ataque.elementalType.ToString();
        attackCritic.text = ataque.criticRate.ToString() + "%";
        attackCategory.text = ataque.categoria.ToString();
        attackObjectives.text = ataque.objective.ToString();
        attackRange.text = ataque.rango.ToString();
        attackDescription.text = ataque.descripcion;


        // 5. Objetivos (Lo pasamos al español para que quede bien en la UI)
        if (ataque.objective == AttackData.Objective.Enemigos)
        {
            attackObjectives.text = "Enemigos";
        }
        else if (ataque.objective == AttackData.Objective.Aliados)
        {
            attackObjectives.text = "Aliados";
        }
        else
        {
            attackObjectives.text = "Ambos"; // Por si en el futuro añades más opciones
        }
    }

    public void OnClickFirstAttack()
    {
        if (firstAttack != null && !selectedCharacter.isActing)
        {
            MostrarInfoAtaque(firstAttack); // Llamamos al metodo para que nos busque la info de este ataque
            CombatManager.instance.ataqueSeleccionado = firstAttack; // <-- ASIGNAMOS EL ATAQUE
            CombatManager.instance.GetRangeAttackCells(selectedCharacter, firstAttack.rango);
        }
    }

    public void OnClickSecondAttack()
    {
        if (secondAttack != null && !selectedCharacter.isActing)
        {
            MostrarInfoAtaque(secondAttack); // Llamamos al metodo para que nos busque la info de este ataque
            CombatManager.instance.ataqueSeleccionado = secondAttack; // <-- ASIGNAMOS EL ATAQUE
            CombatManager.instance.GetRangeAttackCells(selectedCharacter, secondAttack.rango);
        }
    }

    public void OnClickThirdAttack()
    {
        if (thirdAttack != null && !selectedCharacter.isActing)
        {
            MostrarInfoAtaque(thirdAttack); // Llamamos al metodo para que nos busque la info de este ataque
            CombatManager.instance.ataqueSeleccionado = thirdAttack; // <-- ASIGNAMOS EL ATAQUE
            CombatManager.instance.GetRangeAttackCells(selectedCharacter, thirdAttack.rango);
        }
    }

    public void OnClickCancelar()
    {
        if (!selectedCharacter.isActing)
        {
            selectedCharacter.estadoActual = CharacterEntity.UnidadEstado.EligiendoAccion;
            HideAttackMenu();
        }
    }

    public void HideAttackMenu()
    {
        CombatManager.instance.ataqueSeleccionado = null; // Deseleccionamos el ataque
        // Desactivaremos el menu.
        attackMenu.SetActive(false);
        attackInfoPanel.SetActive(false);
        CombatManager.instance.attackOverlay.ClearAllTiles();

        attackName.text = "-";
        attackPower.text = "-";
        attackStamina.text = "-";
        attackAccurate.text = "-";
        attackType.text = "-";
        attackCritic.text = "-";
        attackCategory.text = "-";
        attackObjectives.text = "-";
        attackRange.text = "-";
        attackDescription.text = "-";
        attackObjectives.text = "-";

        // Esto sirve para que al morir un enemigo, en las casillas de movimiento se actualice para que aparezca que ya es accesible
        if (selectedCharacterMovement != null)
        {
            selectedCharacterMovement.RefrescarCuadriculaMovimiento();
        }

        attackButton.gameObject.SetActive(true);
    }
    private void ActualizarBarrasUI()
    {
        if (selectedCharacter == null) return;

        // 1. Calculamos el "Target" (el valor real que debería tener de 0 a 1)
        float targetHealth = selectedCharacter.stats.vidaActual / (float)selectedCharacter.stats.vidaMaxima;
        float targetStamina = selectedCharacter.stats.staminaActual / (float)selectedCharacter.stats.staminaMaxima;

        // 2. Movemos el fillAmount actual hacia el target suavemente
        healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetHealth, Time.deltaTime * suavizadoBarra);
        staminaBarFill.fillAmount = Mathf.Lerp(staminaBarFill.fillAmount, targetStamina, Time.deltaTime * suavizadoBarra);

        // 3. ACTUALIZACIÓN DEL TEXTO (¡La magia ocurre aquí!)
        // Multiplicamos el fillAmount suavizado por el máximo para saber por qué número va la animación.
        // Usamos Mathf.RoundToInt para que no muestre decimales raros en la UI.
        int vidaMostrada = Mathf.RoundToInt(healthBarFill.fillAmount * selectedCharacter.stats.vidaMaxima);
        int staminaMostrada = Mathf.RoundToInt(staminaBarFill.fillAmount * selectedCharacter.stats.staminaMaxima);

        // Actualizamos los TextMeshProUGUI (puedes ajustar el formato a tu gusto)
        healthValue.text = $"{vidaMostrada} / {selectedCharacter.stats.vidaMaxima}";
        staminaValue.text = $"{staminaMostrada} / {selectedCharacter.stats.staminaMaxima}";

        // Opcional: Si la diferencia es mínima, podrías igualarlos para ahorrar cálculos visuales
        if (Mathf.Abs(healthBarFill.fillAmount - targetHealth) < 0.001f)
        {
            healthBarFill.fillAmount = targetHealth;
            // Si quieres ser extra preciso, puedes forzar el texto al valor real exacto aquí
            healthValue.text = $"{selectedCharacter.stats.vidaActual} / {selectedCharacter.stats.vidaMaxima}";
        }
    }

    private void ActualizarTextosStats()
    {
        ConfigurarStatUI(statAttack, "Ataque", selectedCharacter.stats.ataqueFisico, selectedCharacter.statsBase.ataqueFisico, selectedCharacter.turnosAtaqueFisico, selectedCharacter.nivelAtaqueFisico);
        ConfigurarStatUI(statDefense, "Defensa", selectedCharacter.stats.defensaFisica, selectedCharacter.statsBase.defensaFisica, selectedCharacter.turnosDefensaFisica, selectedCharacter.nivelDefensaFisica);
        ConfigurarStatUI(statSpA, "SpA", selectedCharacter.stats.ataqueEspecial, selectedCharacter.statsBase.ataqueEspecial, selectedCharacter.turnosAtaqueEspecial, selectedCharacter.nivelAtaqueEspecial);
        ConfigurarStatUI(statSpD, "SpD", selectedCharacter.stats.defensaEspecial, selectedCharacter.statsBase.defensaEspecial, selectedCharacter.turnosDefensaEspecial, selectedCharacter.nivelDefensaEspecial);
        ConfigurarStatUI(statVelocity, "Velocidad", selectedCharacter.stats.velocidad, selectedCharacter.statsBase.velocidad, selectedCharacter.turnosVelocidad, selectedCharacter.nivelVelocidad);
    }

    public void PartidaFinalizada(bool esVictoria)
    {
        if (esVictoria)
        {
            textoMenu.text = "¡Has Ganado a Jugador 2!";
        }
        else
        {
            textoMenu.text = "¡Has Perdido contra Jugador 2!";
        }
        continueButton.SetActive(false);
        surrenderButton.SetActive(false);
        infoTerrainPanel.SetActive(false);
        actionMenu.SetActive(false);
        attackMenu.SetActive(false);
        buttonsPanel.SetActive(false);
        attackInfoPanel.SetActive(false);
        finalizarPanel.SetActive(true);
        Time.timeScale = 0.0f;
    }

    public void AbrirMenuRendirse()
    {
        textoMenu.text = "¿Te Rindes?";
        finalizarPanel.SetActive(true);
    }

    public void QuitarMenuRendirse()
    {
        finalizarPanel.SetActive(false);
    }

    // Método para refrescar la UI estática si el personaje sufre una transformación en pleno turno
    public void RefrescarFichaUI(CharacterEntity personaje)
    {
        // Solo recargamos si el personaje afectado es exactamente el que tenemos seleccionado
        if (selectedCharacter != null && selectedCharacter == personaje)
        {
            // Refrescamos los iconos de la nueva identidad
            elementalBadge.sprite = selectedCharacter.data.iconoTipo;
            classBadge.sprite = selectedCharacter.data.iconoClase;
            movilityBadge.sprite = selectedCharacter.data.iconoMovilidad;
            characterFace.sprite = selectedCharacter.data.characterFace;

            ActualizarBadgeEstado();

            // Si el menú de ataques estaba abierto, hay que actualizar los botones
            if (attackMenu.activeSelf)
            {
                // Simplemente llamamos de nuevo a Show para que recargue los botones.
                ShowAttackMenu();
            }
        }
    }
}
