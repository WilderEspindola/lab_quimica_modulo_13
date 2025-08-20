using UnityEngine;

public class ParticleController : MonoBehaviour
{
    [Header("Configuraci�n de Part�culas")]
    public ParticleSystem targetParticleSystem;

    [Header("Referencia UI")]
    public GameObject reportButton; // Arrastra el bot�n de reporte aqu�

    private void Start()
    {
        // Ocultar el bot�n al inicio
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

            // Mostrar el bot�n de reporte cuando se inician las part�culas
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

            // Ocultar el bot�n de reporte cuando se detienen las part�culas
            if (reportButton != null)
            {
                reportButton.SetActive(false);
            }
        }
    }
}