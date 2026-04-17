using UnityEngine;
using UnityEngine.UI;

public class LocalCharacterUI : MonoBehaviour
{
    [Header("Referencias UI Local")]
    [SerializeField] private float suavizadoBarra = 5f;

    private Image localHealthBarFill;
    private Image localStaminaBarFill;

    // Referencia a las estadísticas de este personaje en concreto
    private CharacterEntity misStats;

    void Start()
    {
        misStats = GetComponent<CharacterEntity>();

        // Buscamos directamente dentro del Canvas 
        // (OJO: Asegúrate de poner el nombre exacto que tienen las barras de relleno en Unity)
        Transform healthTransform = transform.Find("Canvas/HealthBar");
        Transform staminaTransform = transform.Find("Canvas/StaminaBar");

        // Si los encuentra, nos guardamos su componente Image
        if (healthTransform != null) localHealthBarFill = healthTransform.GetComponent<Image>();
        if (staminaTransform != null) localStaminaBarFill = staminaTransform.GetComponent<Image>();
    }

    void Update()
    {
        if (misStats == null) return;

        // Calculamos los targets
        float targetHealth = misStats.stats.vidaActual / (float)misStats.stats.vidaMaxima;
        float targetStamina = misStats.stats.staminaActual / (float)misStats.stats.staminaMaxima;

        // Suavizamos el fillAmount (igual que en tu otro código)
        localHealthBarFill.fillAmount = Mathf.Lerp(localHealthBarFill.fillAmount, targetHealth, Time.deltaTime * suavizadoBarra);
        localStaminaBarFill.fillAmount = Mathf.Lerp(localStaminaBarFill.fillAmount, targetStamina, Time.deltaTime * suavizadoBarra);
    }
}