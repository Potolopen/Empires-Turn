using UnityEngine;
public abstract class AttackData : ScriptableObject
{
    public enum Objective { Enemigos, Aliados, Ambos }

    public enum Categoria { Fisico, Especial, Estado }

    public string nombre;
    public int rango;
    public int accuracy;
    public Objective objective;
    public int criticRate;
    public int costeStamina;
    public ElementalType elementalType;
    public Categoria categoria;
    public AttackEffect[] efectos;

    [Header("Info de Ataque")]
    [TextArea] // Esto hace que en el Inspector te salga una caja grande para escribir cómodamente
    public string descripcion;

    [Header("Configuración Visual (VFX)")]
    public GameObject vfxPrefab; // El prefab de la animación
    public float duracionAnimacion = 1.5f; // Cuánto tarda en borrarse de la pantalla
    public float tiempoParaAplicarDano = 0.5f; // Cuánto tarda en "explotar" para restar la vida

    public abstract void Ejecutar(CharacterEntity atacante, CharacterEntity objetivo);
}
