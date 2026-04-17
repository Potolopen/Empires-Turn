using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    public Tilemap limitTilemap;
    public Camera cam;

    public float moveSpeed = 20f; // Una sola velocidad es más fácil de manejar

    private Vector3 minBounds;
    private Vector3 maxBounds;

    void Start()
    {
        if (cam == null) cam = Camera.main;

        // Calcular los límites del tilemap
        Bounds tilemapBounds = limitTilemap.localBounds;
        minBounds = tilemapBounds.min;
        maxBounds = tilemapBounds.max;
    }

    void Update()
    {
        // 1. Obtener entrada de teclado (W, A, S, D o Flechas)
        // Esto devuelve un valor entre -1 y 1
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D o Izquierda/Derecha
        float moveY = Input.GetAxisRaw("Vertical");   // W/S o Arriba/Abajo

        // 2. Calcular la nueva posición deseada
        Vector3 direction = new Vector3(moveX, moveY, 0).normalized;
        Vector3 nextPosition = transform.position + direction * moveSpeed * Time.deltaTime;

        // 3. Limitar la cámara (Constraints)
        // Necesitamos saber cuánto ve la cámara para no mostrar el "vacío"
        float camHeight = cam.orthographicSize;
        float camWidth = cam.orthographicSize * cam.aspect;

        // Aplicamos el Clamp usando tus minBounds y maxBounds
        float clampedX = Mathf.Clamp(nextPosition.x, minBounds.x + camWidth, maxBounds.x - camWidth);
        float clampedY = Mathf.Clamp(nextPosition.y, minBounds.y + camHeight, maxBounds.y - camHeight);

        // 4. Aplicar la posición final
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}