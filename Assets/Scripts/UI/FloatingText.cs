using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Configuración visual")]
    public float moveSpeed = 1.5f;     // Velocidad a la que sube el texto
    public float destroyTime = 1.5f;   // Tiempo que tarda en desaparecer

    private TextMeshPro textMesh;
    private Color textColor;

    private void Awake()
    {
        // Usamos TextMeshPro normal (no el de UI) porque estará en el espacio del mundo (World Space)
        textMesh = GetComponent<TextMeshPro>();
    }

    void Start()
    {
        // Guardamos el color inicial para poder modificar su transparencia (Alpha)
        if (textMesh != null) textColor = textMesh.color;

        // Autodestruir el objeto después del tiempo establecido
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // 1. Mover hacia arriba constantemente
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        // 2. Reducir la opacidad (Fade Out) progresivamente
        if (textMesh != null)
        {
            textColor.a -= (1f / destroyTime) * Time.deltaTime;
            textMesh.color = textColor;
        }
    }

    // Función que llamaremos desde otros scripts para configurar el valor y el color
    public void Setup(string text, Color color)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();

        textMesh.text = text;
        textMesh.color = color;
    }
}