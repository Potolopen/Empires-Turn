using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Grid/Terrain Data")]
public class CellData : ScriptableObject
{
    public List<TileBase> tiles; // Aquí meterás todos los sprites del tileset que sean este tipo
    public TipoTerreno tipoTerreno;
    public int costeMovimiento;
}
