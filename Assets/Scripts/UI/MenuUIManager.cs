using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIManager : MonoBehaviour
{
    public static MenuUIManager instance;

    [Header("Paneles")]
    public GameObject panelLogin;
    public GameObject panelMenuPrincipal;
    public GameObject panelSelector;

    public SelectorManager selectorManager;

    public TMP_Text textoUsuario;

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

    // Se llama cuando el Login es exitoso
    public void MostrarMenuPrincipal()
    {
        panelLogin.SetActive(false);
        textoUsuario.text = AuthManager.instance.nombreUsuario;
        panelMenuPrincipal.SetActive(true);
    }

    public void CerrarSesion()
    {
        panelMenuPrincipal.SetActive(false);
        panelLogin.SetActive(true);
    }

    public void AbrirSelector()
    {
        panelMenuPrincipal.SetActive(false);
        panelSelector.SetActive(true);
        selectorManager.IniciarSelector(); // Arranca la lógica
    }

    // Se asigna al botón de "Jugar"
    public void IrACombate()
    {
        SceneManager.LoadScene("SampleScene");
    }
}