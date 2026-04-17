using System.Collections;
using UnityEngine;

public class CombatVisualManager : MonoBehaviour
{
    public static CombatVisualManager instance;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // Los diferentes efectos de estado llamarán a esta función.
    public void ProcesarEstadoVisual(CharacterEntity objetivo, GameObject animPrefab, float duracion)
    {
        if(animPrefab != null)
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
        if (ataque.vfxPrefab != null)
        {
            GameObject vfx = Instantiate(ataque.vfxPrefab, objetivo.transform.position, Quaternion.identity);

            // Se auto-destruye al terminar el tiempo total de la animación
            Destroy(vfx, ataque.duracionAnimacion);
        }

        // 2. Esperar el tiempo exacto hasta que el efecto "impacte" visualmente
        yield return new WaitForSeconds(ataque.tiempoParaAplicarDano);

        // 3. AHORA SÍ, aplicamos el daño real. Como el 'ataqueSeleccionado' 
        // no se ha vuelto null en el CombatManager, esto funcionará perfecto.
        ataque.Ejecutar(atacante, objetivo);

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
}