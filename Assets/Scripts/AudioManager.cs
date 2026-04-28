using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource modStat;
    public AudioClip bufoClip;
    public AudioClip debufoClip;

    public AudioSource ataque;
    public AudioClip ataqueNormalClip;
    public AudioClip ataqueSuperEficazClip;
    public AudioClip ataquePocoEficazClip;

    public AudioSource fallarAtaque;

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

    public void SonidoBufo()
    {
        modStat.resource = bufoClip;
        modStat.Play();
    }

    public void SonidoDebufo()
    {
        modStat.resource = debufoClip;
        modStat.Play();
    }

    public void AtaqueNormal()
    {
        ataque.resource = ataqueNormalClip; 
        ataque.Play();
    }

    public void AtaqueSuperEficaz()
    {
        ataque.resource = ataqueSuperEficazClip;
        ataque.Play();
    }

    public void AtaquePocoEficaz()
    {
        ataque.resource = ataquePocoEficazClip;
        ataque.Play();
    }

    public void FallarAtaque()
    {
        fallarAtaque.Play();
    }
}
