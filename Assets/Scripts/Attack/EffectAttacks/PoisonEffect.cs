using UnityEngine;

[CreateAssetMenu(menuName = "AttackEffect/Poison")]
public class PoisonEffect : AttackEffect
{
    public int accuracy;
    public override void Aplicar(CharacterEntity atacante, CharacterEntity objetivo)
    {
        if (objetivo.statusType != StatusType.Neutro) return;

        // Precisión
        int azarPrecision = Random.Range(1, 101);

        if (azarPrecision <= accuracy)
        {
            objetivo.statusType = StatusType.Envenenado;
            objetivo.turnosEstadoRestantes = 3;
            Debug.Log(objetivo.data.nombre + "sufre envenenamiento a manos de " + atacante.data.nombre);
        }
    }
}
