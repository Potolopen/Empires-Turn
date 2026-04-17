using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    public Tilemap tilemap;
    public CellData[] allCellData;
    public Grid grid;

    public Tilemap cellsOverlay;
    public Tile cell_edge;

    public GameObject selector;

    private Dictionary<TileBase, CellData> dataFromTiles;
    private Dictionary<Vector3Int, Node> gridNodes;

    // NUEVO: Variable para recordar dónde estaba el ratón en el frame anterior
    private Vector3Int ultimaCeldaMouse = new Vector3Int(-9999, -9999, -9999);

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        dataFromTiles = new Dictionary<TileBase, CellData>();

        foreach (var data in allCellData)
        {
            foreach (var t in data.tiles)
            {
                if (t != null && !dataFromTiles.ContainsKey(t))
                {
                    dataFromTiles.Add(t, data);
                }
            }
        }

        gridNodes = new Dictionary<Vector3Int, Node>();
        BoundsInt bounds = tilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            CellData cellData = GetCellData(pos);

            if (cellData == null) continue;

            Node node = new Node();
            node.posicion = pos;
            node.terreno = cellData;
            node.unitPresent = null;

            gridNodes.Add(pos, node);
        }

        if (cellsOverlay != null && cell_edge != null)
        {
            DibujarCuadriculaVisual();
        }
    }

    // NUEVO: LateUpdate se encarga de mover el visual y actualizar la UI de forma optimizada
    void LateUpdate()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int celdaActual = tilemap.WorldToCell(mousePos);
        celdaActual.z = 0;

        // Solo procesamos la UI si el ratón realmente ha saltado a una celda distinta
        if (celdaActual != ultimaCeldaMouse)
        {
            ultimaCeldaMouse = celdaActual;
            ActualizarSelectorYUI(celdaActual);
        }
    }

    // NUEVO: Función dedicada a la parte visual y de UI
    private void ActualizarSelectorYUI(Vector3Int celda)
    {
        Vector3 posicionCentrada = tilemap.GetCellCenterWorld(celda);

        selector.gameObject.SetActive(true);
        selector.transform.position = posicionCentrada;

        CellData infoData = GetCellData(celda);
        if (infoData != null)
        {
            UIManager.instance.infoTerrain.text = "Terreno: " + infoData.tipoTerreno.ToString();
        }
        else
        {
            UIManager.instance.infoTerrain.text = "Terreno: -";
        }
    }

    // MODIFICADO: Ahora solo hace la matemática rápida, sin tocar UI
    public Vector3Int GetCeldaBajoMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int celda = tilemap.WorldToCell(mousePos);
        celda.z = 0;
        return celda;
    }

    public CellData GetCellData(Vector3Int cellPos)
    {
        TileBase tile = tilemap.GetTile(cellPos);
        if (tile != null && dataFromTiles.ContainsKey(tile))
        {
            return dataFromTiles[tile];
        }
        return null;
    }

    public Node GetNode(Vector3Int pos)
    {
        if (gridNodes.ContainsKey(pos))
            return gridNodes[pos];

        return null;
    }

    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        return grid.WorldToCell(worldPos);
    }

    public void DibujarCuadriculaVisual()
    {
        cellsOverlay.ClearAllTiles();
        foreach (Vector3Int pos in gridNodes.Keys)
        {
            cellsOverlay.SetTile(pos, cell_edge);
        }
    }

    public Vector3 CellCenterWorld(Vector3Int cellPos)
    {
        return tilemap.GetCellCenterWorld(cellPos);
    }
}

public class Node
{
    public Vector3Int posicion;
    public CharacterEntity unitPresent;
    public bool ocupada => unitPresent != null;
    public CellData terreno;
}