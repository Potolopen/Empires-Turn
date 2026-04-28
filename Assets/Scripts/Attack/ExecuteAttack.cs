using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Attacks/Attack")]
public class ExecuteAttack : AttackData
{
    public override void Ejecutar(CharacterEntity atacante, CharacterEntity objetivo)
    {
        // Recorre todos los efectos del ataque y aplica cada uno
        foreach (var efecto in efectos)
        {
            // Si el objetivo muere, abortamos el ataque por completo
            if (objetivo == null || objetivo.stats.vidaActual <= 0)
            {
                return;
            }
            efecto.Aplicar(atacante, objetivo);
        }
    }
}
