using UnityEngine;

[CreateAssetMenu(menuName = "AttackEffect/Damage")]
public class DamageEffect : AttackEffect
{
    public int daño; // valor del daño

    public override void Aplicar(CharacterEntity atacante, CharacterEntity objetivo)
    {
        int dañoBruto = 0;
        int defensaObjetivo = 0;

        ElementalType attackType = CombatManager.instance.ataqueSeleccionado.elementalType;

        float multiEfectividad = CombatManager.instance.CalcularEfectividad(attackType, objetivo.data.elementalType);

        if (multiEfectividad == 2.0f)
        {
            AudioManager.instance.AtaqueSuperEficaz();
        }
        else if (multiEfectividad == 1.0f)
        {
            AudioManager.instance.AtaqueNormal();
        }
        else
        {
            AudioManager.instance.AtaquePocoEficaz();
        }

        // Se podria hacer con un if al uso, pero esto también es una buena forma de hacerlo

        /*if (CombatManager.instance.ataqueActualEsCritico)
        {
            multiCritico = 1.5f;
        }*/

        // Esto significa: Si es crítico pon 1.5, si no pon 1.0
        float multiCritico = CombatManager.instance.ataqueActualEsCritico ? 1.5f : 1.0f;

        // Seleccionamos las estadísticas según el tipo de daño
        switch (CombatManager.instance.ataqueSeleccionado.categoria)
        {
            case AttackData.Categoria.Fisico:
                dañoBruto = atacante.stats.ataqueFisico + daño;
                defensaObjetivo = objetivo.stats.defensaFisica;
                break;

            case AttackData.Categoria.Especial:
                dañoBruto = atacante.stats.ataqueEspecial + daño;
                defensaObjetivo = objetivo.stats.defensaEspecial;
                break;

            default:
                break;
        }

        Debug.Log("Ataque más daño del ataque: " + dañoBruto + " Defensa del rival: " + defensaObjetivo);

        // 1. Aplicamos el Crítico primero (Ayuda a superar la defensa)
        float dañoConCritico = dañoBruto * multiCritico;

        Debug.Log("Aplicamos el critico: " + dañoBruto * multiCritico);

        // 2. Restamos la defensa y nos aseguramos de que al menos pase 1 de daño
        float dañoTrasDefensa = Mathf.Max(1, dañoConCritico - defensaObjetivo);

        Debug.Log("Le restamos a la defensa: " + (dañoConCritico - defensaObjetivo));

        // Cálculo final de mitigación
        // 4. Resultado final
        int dañoFinal = Mathf.CeilToInt(dañoTrasDefensa * multiEfectividad);

        Debug.Log("Este es el daño final: " + dañoTrasDefensa * multiEfectividad);

        // Aplicamos el resultado al objetivo
        objetivo.RecibirDaño(dañoFinal, atacante);
    }
}
