using UnityEngine;
using System.Collections.Generic; // Necesario para usar List<>

// 1. Creamos un pequeño bloque de datos para agrupar la info en el Inspector
[System.Serializable] // ¡Súper importante para que Unity lo muestre en el Inspector!
public struct InfoModificacion
{
    public StatType statAfectada;
    public int cantidadModificacion;
    public int duracionTurnos;
    public bool esParaEnemigo;
    [Range(1, 100)] // Añadir esto es un truco genial para que en el Inspector te salga una barrita deslizable del 1 al 100
    public int probabilidad;
}

[CreateAssetMenu(menuName = "AttackEffect/Buffs")]
public class BuffsEffect : AttackEffect
{
    [Header("Lista de Modificaciones")]
    // 2. Creamos una lista de esos bloques
    [SerializeField]
    private List<InfoModificacion> modificaciones = new List<InfoModificacion>();

    public override void Aplicar(CharacterEntity atacante, CharacterEntity objetivo)
    {
        // Creamos "interruptores" para saber si sonó algo
        bool aplicoBufo = false;
        bool aplicoDebufo = false;

        // 3. Recorremos la lista y aplicamos CADA modificación al objetivo
        foreach (var mod in modificaciones)
        {
            int azarPrecision = Random.Range(1, 101);

            if (azarPrecision <= mod.probabilidad)
            {
                if (mod.esParaEnemigo)
                {
                    objetivo.AñadirModificadorDeStat(mod.statAfectada, mod.cantidadModificacion, mod.duracionTurnos);

                    if (mod.cantidadModificacion > 0)
                    {
                        aplicoBufo = true;
                    }
                    else if (mod.cantidadModificacion < 0)
                    {
                        aplicoDebufo = true;
                    }


                    Debug.Log($"{atacante.data.nombre} modificó {mod.statAfectada} de {objetivo.data.nombre} en {mod.cantidadModificacion} niveles por {mod.duracionTurnos} turnos.");
                }
                else
                {
                    atacante.AñadirModificadorDeStat(mod.statAfectada, mod.cantidadModificacion, mod.duracionTurnos);

                    if (mod.cantidadModificacion > 0)
                    {
                        aplicoBufo = true;
                    }
                    else if (mod.cantidadModificacion < 0)
                    {
                        aplicoDebufo = true;
                    }


                    Debug.Log($"{atacante.data.nombre} modificó {mod.statAfectada} de {atacante.data.nombre} en {mod.cantidadModificacion} niveles por {mod.duracionTurnos} turnos.");
                }

            }
        }
        if (aplicoBufo)
        {
            AudioManager.instance.SonidoBufo();
        }
        else if (aplicoDebufo)
        {
            AudioManager.instance.SonidoDebufo();
        }
    }
}