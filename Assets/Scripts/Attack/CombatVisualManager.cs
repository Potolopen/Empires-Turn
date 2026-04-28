using System.Collections;
using UnityEngine;

public class CombatVisualManager : MonoBehaviour
{
    public static CombatVisualManager instance;

    [Header("Textos Flotantes")]
    public GameObject floatingTextPrefab; // Arrastra aquí tu prefab desde el Inspector
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // Los diferentes efectos de estado llamarán a esta función.
    public void ProcesarEstadoVisual(CharacterEntity objetivo, GameObject animPrefab, float duracion)
    {
        // Si el objetivo ya fue destruido, no instanciamos nada
        if (objetivo == null) return;

        if (animPrefab != null)
        {
            GameObject vfx = Instantiate(animPrefab, objetivo.transform.position, Quaternion.identity);
            Destroy(vfx, duracion);
        }
    }

    // El CombatManager llamará a esta función en lugar de aplicar el daño instantáneamente
    public void ProcesarAtaqueVisual(CharacterEntity atacante, CharacterEntity objetivo, AttackData ataque)
    {
        StartCoroutine(RutinaVisualAtaque(atacante, objetivo, ataque));
    }

    private IEnumerator RutinaVisualAtaque(CharacterEntity atacante, CharacterEntity objetivo, AttackData ataque)
    {
        // 1. Mostrar la animación sobre el enemigo
        if (ataque.vfxPrefab != null && objetivo != null)
        {
            GameObject vfx = Instantiate(ataque.vfxPrefab, objetivo.transform.position, Quaternion.identity);

            // Se auto-destruye al terminar el tiempo total de la animación
            Destroy(vfx, ataque.duracionAnimacion);
        }

        // 2. Esperar el tiempo exacto hasta que el efecto "impacte" visualmente
        yield return new WaitForSeconds(ataque.tiempoParaAplicarDano);

        // 3. AHORA SÍ, aplicamos el daño real. Como el 'ataqueSeleccionado' 
        // no se ha vuelto null en el CombatManager, esto funcionará perfecto.
        // Validamos que ninguno haya muerto/sido destruido antes de aplicar el daño.
        if (atacante != null && objetivo != null)
        {
            ataque.Ejecutar(atacante, objetivo);
        }

        // 4. Esperar el resto del tiempo de la animación antes de dar por terminado todo
        float tiempoRestante = ataque.duracionAnimacion - ataque.tiempoParaAplicarDano;
        if (tiempoRestante > 0)
        {
            yield return new WaitForSeconds(tiempoRestante);
        }

        // 5. La animación ha terminado. Avisamos al CombatManager para que limpie la interfaz, 
        // quite la estamina y ponga el ataqueSeleccionado a null.
        CombatManager.instance.FinalizarLimpiezaDeAtaque(atacante);
    }

    // Instancia el texto flotante
    public void MostrarTextoFlotante(Vector3 posicion, string texto, Color color)
    {
        if (floatingTextPrefab != null)
        {
            // 1. Generamos valores aleatorios pequeños para X e Y
            float randomX = Random.Range(-0.5f, 0.5f); // Un poco a la izquierda o derecha
            float randomY = Random.Range(0.6f, 1.2f);  // Siempre hacia arriba, pero con variación

            // 2. Sumamos esa variación a la posición original del personaje
            Vector3 offsetPos = posicion + new Vector3(randomX, randomY, 0);

            GameObject go = Instantiate(floatingTextPrefab, offsetPos, Quaternion.identity);
            FloatingText ft = go.GetComponent<FloatingText>();

            if (ft != null)
            {
                ft.Setup(texto, color);
            }
        }
        else
        {
            Debug.LogWarning("Falta asignar el prefab de Floating Text en CombatVisualManager");
        }
    }
}