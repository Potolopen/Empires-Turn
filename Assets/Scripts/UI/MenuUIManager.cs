using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    public static MenuUIManager instance;

    [Header("Paneles")]
    public GameObject panelLogin;
    public GameObject panelRegister;
    public GameObject panelMenuPrincipal;
    public GameObject panelSelector;
    public GameObject panelEstadisticas;

    public SelectorManager selectorManager;

    [Header("Info Header")]
    public TMP_Text textoUsuario;
    public TMP_Text textoInfo;
    public GameObject botonCerrarSesion;
    public GameObject botonVolver;


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

    void Start()
    {
        // Preguntamos a Unity: "¿Tienes guardada la llave del ID de usuario?"
        if (PlayerPrefs.HasKey("IdUsuarioLogueado"))
        {
            // Esto significa que estamos logueados y sirve para volver después del combate.
            panelLogin.SetActive(false);
            MostrarMenuPrincipal();

            // Recuperamos el nombre que guardamos en el AuthManager
            textoUsuario.text = PlayerPrefs.GetString("NombreUsuarioLogueado", "Jugador");
        }
        else
        {
            // No hay nadie logueado.
            panelLogin.SetActive(true);
            panelMenuPrincipal.SetActive(false);
        }
    }

    // Se llama cuando el Login es exitoso
    public void MostrarMenuPrincipal()
    {
        panelLogin.SetActive(false);
        panelEstadisticas.SetActive(false);
        textoUsuario.text = AuthManager.instance.nombreUsuario;
        botonCerrarSesion.SetActive(true);
        textoInfo.text = "¡Bienvenido a Empire's Turn!";
        panelMenuPrincipal.SetActive(true);
    }

    public void CerrarSesion()
    {
        // Borramos la info de quien estaba logueado
        PlayerPrefs.DeleteKey("IdUsuarioLogueado");
        panelMenuPrincipal.SetActive(false);
        textoUsuario.text = null;
        botonCerrarSesion.SetActive(false);
        textoInfo.text = "Inicia Sesión";
        panelLogin.SetActive(true);
    }

    public void MostrarEstadisticas()
    {
        panelMenuPrincipal.SetActive(false);
        botonCerrarSesion.SetActive(false);
        textoInfo.text = "Estadisticas";
        botonVolver.SetActive(true);
        panelEstadisticas.SetActive(true);
        EstadisticasUIManager.instance.CargarDatos();
    }

    public void SalirEstadisticas()
    {
        panelEstadisticas.SetActive(false);
        botonVolver.SetActive(false);
        textoInfo.text = "¡Bienvenido a Empire's Turn!";
        botonCerrarSesion.SetActive(true);
        panelMenuPrincipal.SetActive(true);
    }

    public void CerrarSelector()
    {
        selectorManager.IniciarSelector();
        panelSelector.SetActive(false);
        botonVolver.SetActive(false);
        textoInfo.text = "¡Bienvenido a Empire's Turn!";
        botonCerrarSesion.SetActive(true);
        panelMenuPrincipal.SetActive(true);
    }

    public void AbrirSelector()
    {
        botonCerrarSesion.SetActive(false);
        botonVolver.SetActive(true);
        panelMenuPrincipal.SetActive(false);
        panelSelector.SetActive(true);
        selectorManager.IniciarSelector(); // Arranca la lógica
    }

    public void AbrirRegistro()
    {
        panelLogin.SetActive(false);
        textoInfo.text = "Registrate";
        panelRegister.SetActive(true);
    }

    public void SalirRegistro()
    {
        panelRegister.SetActive(false);
        textoInfo.text = "Inicia Sesión";
        panelLogin.SetActive(true);
    }

    // Se asigna al botón de "Jugar"
    public void IrACombate()
    {
        SceneManager.LoadScene("SampleScene");
    }
}