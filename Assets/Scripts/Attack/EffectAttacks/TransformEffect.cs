using UnityEngine;

[CreateAssetMenu(menuName = "AttackEffect/Transform")]
public class TransformEffect : AttackEffect
{
    public override void Aplicar(CharacterEntity atacante, CharacterEntity objetivo)
    {
        if (objetivo == null) return;

        if (objetivo == atacante)
        {
            Debug.Log("Selecciona a otro pringao");
            return;
        }

        if (CombatVisualManager.instance != null && vfxEffectPrefab != null)
        {
            CombatVisualManager.instance.ProcesarEstadoVisual(atacante, vfxEffectPrefab, vfxEffectDuration);
        }

        // Llamamos a la función que gestiona toda la lógica en el personaje
        atacante.TransformarEn(objetivo);

        // Aqui he de poner algun efecto, ya veré.
    }
}