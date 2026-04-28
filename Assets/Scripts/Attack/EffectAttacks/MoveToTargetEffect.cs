using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "AttackEffect/MoveToTarget")]
public class MoveToTargetEffect : AttackEffect
{
    public override void Aplicar(CharacterEntity atacante, CharacterEntity objetivo)
    {
        Vector3Int[] direcciones = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        Vector3Int mejorCasilla = atacante.currentCell;
        float menorDistancia = float.MaxValue;

        foreach (Vector3Int dir in direcciones)
        {
            Vector3Int casillaAdyacente = objetivo.currentCell + dir;

            // --- NUEVA CONDICIÓN CRÍTICA ---
            // Si la casilla NO forma parte de las casillas rojas del ataque, la ignoramos.
            // Así evitamos que se teletransporte detrás de muros o fuera de rango.
            if (!CombatManager.instance.rangeCells.Contains(casillaAdyacente))
                continue;

            // Comprobamos si la casilla es transitable y está libre
            Node nodo = GridManager.instance.GetNode(casillaAdyacente);
            if (nodo != null && nodo.unitPresent == null)
            {
                // Calculamos la distancia desde el atacante a esta casilla
                float distancia = Vector3Int.Distance(atacante.currentCell, casillaAdyacente);

                // Nos quedamos con la casilla libre que esté más cerca del atacante
                if (distancia < menorDistancia)
                {
                    menorDistancia = distancia;
                    mejorCasilla = casillaAdyacente;
                }
            }
        }

        // Si encontramos una casilla válida y no estamos ya en ella
        if (mejorCasilla != atacante.currentCell)
        {
            Debug.Log($"{atacante.data.nombre} carga hacia {objetivo.data.nombre} aterrizando en la casilla {mejorCasilla}");

            // Llamamos a CharacterMovement para forzar el movimiento visual
            CharacterMovement mov = atacante.GetComponent<CharacterMovement>();
            if (mov != null)
            {
                mov.ForzarMovimiento(mejorCasilla);
            }
        }
        else
        {
            Debug.Log("El ataque conectó, pero no hay casillas libres válidas en el rango para realizar la carga.");
        }
    }
}