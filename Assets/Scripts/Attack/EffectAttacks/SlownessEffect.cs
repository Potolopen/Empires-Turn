using UnityEngine;

[CreateAssetMenu(menuName = "AttackEffect/Slowness")]
public class SlownessEffect : AttackEffect
{
    public int accuracy;
    public override void Aplicar(CharacterEntity atacante, CharacterEntity objetivo)
    {
        if (objetivo.statusType != StatusType.Neutro) return;

        // Precisión
        int azarPrecision = Random.Range(1, 101);

        if (azarPrecision <= accuracy)
        {
            objetivo.statusType = StatusType.Lentitud;
            objetivo.turnosEstadoRestantes = 3;
            objetivo.stats.velocidad = Mathf.CeilToInt(objetivo.stats.velocidad * 0.5f);

            objetivo.vfxEstadoActual = this.vfxEffectPrefab;

            Debug.Log(objetivo.data.nombre + "sufre lentitud a manos de " + atacante.data.nombre);

            if (CombatVisualManager.instance != null && vfxEffectPrefab != null)
            {
                CombatVisualManager.instance.ProcesarEstadoVisual(objetivo, vfxEffectPrefab, vfxEffectDuration);
            }
        }
    }
}
