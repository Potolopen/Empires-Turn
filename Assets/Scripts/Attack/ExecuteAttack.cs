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
            efecto.Aplicar(atacante, objetivo);
        }
    }
}
