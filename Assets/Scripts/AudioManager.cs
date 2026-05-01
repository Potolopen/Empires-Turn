using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio sources que reproducirán sonidos")]
    public AudioSource sourcePrincipal;
    // He simplificado a un source para que hagan fila.

    // Creamos una fila de sonidos para que no se reproduzcan a la vez
    private Queue<AudioClip> colaDeSonidos = new Queue<AudioClip>();
    private bool estaReproduciendo = false;

    [Header("Sonidos para la modificación de estadisticas")]
    public AudioClip bufoClip;
    public AudioClip debufoClip;

    [Header("Sonidos para el efecto del ataque")]
    public AudioClip ataqueNormalClip;
    public AudioClip ataqueSuperEficazClip;
    public AudioClip ataquePocoEficazClip;
    public AudioClip ataqueFallidoClip;
    public AudioClip curaClip;
    public AudioClip staminaClip;

    [Header("Sonidos para cada tipo")]
    public AudioClip aguaClip;
    public AudioClip fuegoClip;
    public AudioClip plantaClip;
    public AudioClip tierraClip;
    public AudioClip electricoClip;
    public AudioClip aceroClip;
    public AudioClip hieloClip;
    public AudioClip vientoClip;
    public AudioClip sonicoClip;
    public AudioClip menteClip;
    public AudioClip toxicoClip;
    public AudioClip normalClip;

    [Header("Sonidos para cada estado")]
    public AudioClip quemaduraClip;
    public AudioClip frioClip;
    public AudioClip envenenamientoClip;
    public AudioClip lentitudClip;
    public AudioClip neutroClip;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // Evita que haya dos managers por error
            return;
        }
    }

    private void EncolarSonido(AudioClip clip)
    {
        if (clip == null) return;

        colaDeSonidos.Enqueue(clip);

        if (!estaReproduciendo)
        {
            StartCoroutine(ProcesarCola());
        }
    }

    IEnumerator ProcesarCola()
    {
        estaReproduciendo = true;

        while (colaDeSonidos.Count > 0)
        {
            AudioClip siguienteClip = colaDeSonidos.Dequeue();
            sourcePrincipal.clip = siguienteClip;
            sourcePrincipal.Play();

            // Espera hasta que el clip termine
            yield return new WaitWhile(() => sourcePrincipal.isPlaying);

            // Opcional: una pequeña pausa de 0.1s entre sonidos para que no se peguen tanto
            yield return new WaitForSeconds(0.1f);
        }

        estaReproduciendo = false;
    }

    public void SonidoBufo()
    {
        EncolarSonido(bufoClip);
    }

    public void SonidoDebufo()
    {
        EncolarSonido(debufoClip);
    }

    public void AtaqueNormal()
    {
        EncolarSonido(ataqueNormalClip);
    }

    public void AtaqueSuperEficaz()
    {
        EncolarSonido(ataqueSuperEficazClip);
    }

    public void AtaquePocoEficaz()
    {
        EncolarSonido(ataquePocoEficazClip);
    }

    public void FallarAtaque()
    {
        EncolarSonido(ataqueFallidoClip);
    }

    public void Curacion()
    {
        EncolarSonido(curaClip);
    }

    public void RecuperarEstamina()
    {
        EncolarSonido(staminaClip);
    }

    public void sonidoAtaqueSegunTipo(ElementalType type)
    {
        switch (type)
        {
            case ElementalType.Agua:
                EncolarSonido(aguaClip);
                break;
                
            case ElementalType.Fuego:
                EncolarSonido(fuegoClip);
                break;

            case ElementalType.Planta:
                EncolarSonido(plantaClip);
                break;

            case ElementalType.Tierra:
                EncolarSonido(tierraClip);
                break;

            case ElementalType.Electrico:
                EncolarSonido(electricoClip);
                break;

            case ElementalType.Acero:
                EncolarSonido(aceroClip);
                break;

            case ElementalType.Hielo:
                EncolarSonido(hieloClip);
                break;

            case ElementalType.Viento:
                EncolarSonido(vientoClip);
                break;

            case ElementalType.Sonico:
                EncolarSonido(sonicoClip);
                break;

            case ElementalType.Mente:
                EncolarSonido(menteClip);
                break;

            case ElementalType.Toxico:
                EncolarSonido(toxicoClip);
                break;

            case ElementalType.Normal:
                EncolarSonido(normalClip);
                break;
        }
    }

    public void sonidoSegunEstado(StatusType status)
    {
        switch (status)
        {
            case StatusType.Quemadura:
                EncolarSonido(quemaduraClip);
                break;

            case StatusType.Frio:
                EncolarSonido(frioClip);
                break;

            case StatusType.Envenenamiento:
                EncolarSonido(envenenamientoClip);
                break;

            case StatusType.Lentitud:
                EncolarSonido(lentitudClip);
                break;

            case StatusType.Neutro:
                EncolarSonido(neutroClip);
                break;
        }
    }
}
