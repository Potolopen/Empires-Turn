using UnityEngine;

[CreateAssetMenu(menuName = "Characters/Character Data")]
public class CharacterData : ScriptableObject
{
    public string nombre;
    public Stats stats;
    public Ability ability;
    public AttackData[] attacks = new AttackData[3];
    public TipoMovilidad tipoMovilidad;
    public ElementalType elementalType;
    public ClassType classType;

    [Header("Visuales")]
    public Sprite iconoTipo;      // tipo agua, tipo fuego...
    public Sprite iconoClase; // Guerrero, Defensor...
    public Sprite iconoMovilidad; // terrestre, volador...
    public Sprite characterFace; // La cara del personaje
}

[System.Serializable]
public class Stats
{
    public int vidaMaxima = 0, vidaActual = 0, staminaMaxima = 0, staminaActual = 0, ataqueFisico = 0, defensaFisica = 0, ataqueEspecial = 0, defensaEspecial = 0, velocidad = 0;
}

[System.Serializable]
public class Ability
{
    public string nombre;
    
}