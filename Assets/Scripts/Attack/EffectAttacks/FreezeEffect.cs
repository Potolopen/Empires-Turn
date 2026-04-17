using UnityEngine;

[CreateAssetMenu(menuName = "AttackEffect/Freeze")]
public class FreezeEffect : AttackEffect
{
    public int accuracy;
    public override void Aplicar(CharacterEntity atacante, CharacterEntity objetivo)
    {
        if (objetivo.statusType != StatusType.Neutro) return;

        // Precisión
        int azarPrecision = Random.Range(1, 101);

        if (azarPrecision <= accuracy)
        {
            objetivo.statusType = StatusType.Frio;
            objetivo.turnosEstadoRestantes = 3;
            objetivo.stats.ataqueEspecial = Mathf.CeilToInt(objetivo.stats.ataqueEspecial * 0.5f);
            Debug.Log(objetivo.data.nombre + "sufre frio a manos de " + atacante.data.nombre);
        }
    }
}
