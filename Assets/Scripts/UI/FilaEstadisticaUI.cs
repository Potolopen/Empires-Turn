using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FilaEstadisticaUI : MonoBehaviour
{
    [Header("Referencias de mi propio diseño")]
    public Image imagenCara;
    public TMP_Text textoNombre;
    public TMP_Text textoKills;
    public TMP_Text textoUsos;

    // El Jefe (Manager) llamará a esta función y le pasará los datos
    public void Configurar(Sprite cara, string nombre, int kills, int usos)
    {
        // Rellenamos nuestros propios huecos
        if (imagenCara != null) imagenCara.sprite = cara;
        if (textoNombre != null) textoNombre.text = nombre;
        if (textoKills != null) textoKills.text = kills.ToString();
        if (textoUsos != null) textoUsos.text = usos.ToString();
    }
}