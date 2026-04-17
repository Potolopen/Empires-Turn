using UnityEngine;

[CreateAssetMenu(menuName = "AttackEffect/Heal")]
public class HealEffect : AttackEffect
{
    public int heal; // valor de la curacion

    public override void Aplicar(CharacterEntity atacante, CharacterEntity objetivo)
    {
        if (objetivo.statusType == StatusType.Envenenado)
        {
            heal = Mathf.Max(1, Mathf.CeilToInt(heal * 0.5f));
        }

        objetivo.RecibirCuracion(heal);
    }
}
