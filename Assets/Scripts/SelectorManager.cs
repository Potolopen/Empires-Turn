using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SelectorManager : MonoBehaviour
{
    [Header("Configuración")]
    public List<GameObject> todosLosPersonajes; // Lista de todos los prefabs disponibles
    public GameObject botonPrefab;              // El prefab del botón con componente Image y Button
    public Transform contenedorBotones;        // El objeto con el "Grid Layout Group"
    public TMP_Text textoEstado;                // Para decir "Turno P1" or "P2"

    private int turnoDeJugador; // 1 o 2
    private int seleccionesP1, seleccionesP2;

    public void IniciarSelector()
    {
        DatosCombate.equipoP1.Clear();
        DatosCombate.equipoP2.Clear();
        seleccionesP1 = 0; seleccionesP2 = 0;

        // 50/50 para ver quién empieza
        turnoDeJugador = Random.Range(1, 3);
        ActualizarTexto();
        GenerarBotones();
    }

    void GenerarBotones()
    {
        foreach (Transform hijo in contenedorBotones) Destroy(hijo.gameObject);

        foreach (GameObject prefab in todosLosPersonajes)
        {
            GameObject nuevoBoton = Instantiate(botonPrefab, contenedorBotones, false);
            CharacterEntity entity = prefab.GetComponent<CharacterEntity>();

            // --- ASIGNACIÓN DE CARA USANDO TU IDEA ---
            // Buscamos todas las imágenes del botón y sus hijos.
            // true -> incluye imágenes desactivadas.
            Image[] imagenes = nuevoBoton.GetComponentsInChildren<Image>(true);

            // Verificamos que al menos existan dos imágenes (Fondo y Cara) para evitar errores.
            if (imagenes.Length >= 2)
            {
                // Asignar la cara del personaje al botón desde el ScriptableObject.
                // imagenes[0] es el fondo (padre), imagenes[1] es la cara (hijo).
                imagenes[1].sprite = entity.data.characterFace;

                // Nos guardamos la referencia de la imagen de la cara para pasarla al Listener.
                Image faceImage = imagenes[1];

                Button btn = nuevoBoton.GetComponent<Button>();

                TMP_Text txt = nuevoBoton.GetComponentInChildren<TMP_Text>();

                // Actualizamos el Listener para pasar también la referencia de la cara.
                btn.onClick.AddListener(() => Seleccionar(prefab, btn, txt, faceImage));
            }
            else
            {
                Debug.LogError("El Prefab del botón no tiene la estructura correcta (Padre con Image -> Hijo con Image).");
            }
            // ------------------------------------------
        }
    }

    // Actualizamos la función Seleccionar para que reciba la imagen de la cara
    void Seleccionar(GameObject personaje, Button boton, TMP_Text seleccion,Image faceImage)
    {
        if (turnoDeJugador == 1)
        {
            DatosCombate.equipoP1.Add(personaje);
            seleccionesP1++;
            seleccion.text = seleccionesP1.ToString();
            seleccion.color = Color.green;
        }
        else
        {
            DatosCombate.equipoP2.Add(personaje);
            seleccionesP2++;
            seleccion.text = seleccionesP2.ToString();
            seleccion.color = Color.red;
        }

        // Efecto visual de deshabilitado (baja opacidad si el "Disabled Color" del botón está configurado)
        // NOTA: Esto solo afecta automáticamente al Target Graphic del botón (normalmente el fondo).
        boton.interactable = false;

        // --- EFECTO MANUAL EN LA CARA ---
        // También bajamos la opacidad de la cara manualmente usando la referencia que pasamos.
        if (faceImage != null)
        {
            Color tempColor = faceImage.color;
            tempColor.a = 0.5f; // Mitad de opacidad
            faceImage.color = tempColor;
        }
        // ---------------------------------

        if (seleccionesP1 >= 6 && seleccionesP2 >= 6)
        {
            SceneManager.LoadScene("SampleScene"); // ¡A combatir!
        }
        else
        {
            turnoDeJugador = (turnoDeJugador == 1) ? 2 : 1;
            ActualizarTexto();
        }
    }

    void ActualizarTexto()
    {
        if(turnoDeJugador == 1) 
        {
            textoEstado.text = "Turno del Jugador " + AuthManager.instance.nombreUsuario;
        }
        else
        {
            textoEstado.text = "Turno del Jugador " + turnoDeJugador;
        }
    }

    public void EliminarDatos()
    {
        DatosCombate.equipoP1.Clear();
        DatosCombate.equipoP2.Clear();
    }
}