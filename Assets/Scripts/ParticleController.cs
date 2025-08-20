using UnityEngine;

public class ParticleController : MonoBehaviour
{
    [Header("Configuración de Partículas")]
    public ParticleSystem targetParticleSystem;

    [Header("Referencia UI")]
    public GameObject reportButton; // Arrastra el botón de reporte aquí

    private void Start()
    {
        // Ocultar el botón al inicio
        if (reportButton != null)
        {
            reportButton.SetActive(false);
        }
    }

    public void StartParticles()
    {
        if (targetParticleSystem != null)
        {
            targetParticleSystem.Play();

            // Mostrar el botón de reporte cuando se inician las partículas
            if (reportButton != null)
            {
                reportButton.SetActive(true);
            }
        }
    }

    public void StopParticles()
    {
        if (targetParticleSystem != null)
        {
            targetParticleSystem.Stop();

            // Ocultar el botón de reporte cuando se detienen las partículas
            if (reportButton != null)
            {
                reportButton.SetActive(false);
            }
        }
    }
}