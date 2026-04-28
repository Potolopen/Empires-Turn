using UnityEngine;

[CreateAssetMenu(menuName = "AttackEffect/Stamina")]
public class StaminaEffect : AttackEffect
{
    public int stamina; // valor de la curacion
    public bool recuperaStamina;
    public bool aElMismo;

    public override void Aplicar(CharacterEntity atacante, CharacterEntity objetivo)
    {
        if (recuperaStamina)
        {
            if (objetivo.statusType == StatusType.Lentitud)
            {
                stamina = Mathf.Max(1, Mathf.CeilToInt(stamina * 0.5f));
            }

            if (!aElMismo && objetivo != atacante)
            {
                objetivo.RecibirStamina(stamina);
            }
            else if (aElMismo && objetivo == atacante)
            {
                atacante.RecibirStamina(stamina);
            }
        }
        else
        {
            objetivo.PerderStamina(stamina);
        }

        
    }
}
