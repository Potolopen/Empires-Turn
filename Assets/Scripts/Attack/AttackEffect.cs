using UnityEngine;

public abstract class AttackEffect : ScriptableObject
{
    public GameObject vfxEffectPrefab; // El prefab de la animación
    public float vfxEffectDuration = 1.5f; // Cuánto tarda en borrarse de la pantalla
    public abstract void Aplicar(CharacterEntity atacante, CharacterEntity objetivo);
}