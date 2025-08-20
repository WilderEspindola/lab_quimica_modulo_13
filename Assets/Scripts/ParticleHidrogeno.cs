using UnityEngine;

public class ParticleHidrogeno : MonoBehaviour
{
    [Header("Configuración de Partículas")]
    public ParticleSystem targetParticleSystem;

    [Header("Referencia UI")]
    public GameObject reportButton;

    [Header("Configuración de Sonido")]
    public AudioClip gasReleaseSound;
    public AudioSource gasAudioSource;
    [Range(0, 1)] public float gasVolume = 0.7f;

    [Header("Fuentes de Datos")]
    public DualPartButton pressureVolumeSource;
    public ParticleControllerFire temperatureSource;

    [Header("Límite de Presión")]
    public float pressureLimit = 85f; // Límite de presión en atm

    // Propiedades solo para obtener los datos
    public float CurrentPressure { get; private set; }
    public float CurrentVolume { get; private set; }
    public float CurrentTemperature { get; private set; }

    private void Update()
    {
        // Solo obtener los datos de las fuentes originales
        if (pressureVolumeSource != null)
        {
            CurrentPressure = pressureVolumeSource.GetCurrentPressure();
            CurrentVolume = pressureVolumeSource.GetCurrentVolume();
        }

        if (temperatureSource != null)
        {
            CurrentTemperature = temperatureSource.GetCurrentTemperature();
        }

        // Verificar si la presión supera el límite y detener partículas
        CheckPressureLimit();
    }

    private void CheckPressureLimit()
    {
        if (targetParticleSystem != null && targetParticleSystem.isPlaying)
        {
            if (CurrentPressure > pressureLimit)
            {
                Debug.Log($"Presión demasiado alta ({CurrentPressure:0.00} atm > {pressureLimit} atm). Deteniendo partículas.");
                StopParticles();
            }
        }
    }

    private void Start()
    {
        if (reportButton != null)
        {
            reportButton.SetActive(false);
        }

        if (gasAudioSource != null && gasReleaseSound != null)
        {
            gasAudioSource.clip = gasReleaseSound;
            gasAudioSource.loop = true;
            gasAudioSource.volume = gasVolume;
        }
    }

    public void StartParticles()
    {
        if (targetParticleSystem != null)
        {
            // Verificar presión antes de iniciar
            if (CurrentPressure > pressureLimit)
            {
                Debug.LogWarning($"No se pueden iniciar partículas. Presión demasiado alta: {CurrentPressure:0.00} atm");
                return;
            }

            targetParticleSystem.Play();

            if (reportButton != null)
            {
                reportButton.SetActive(true);
            }

            if (gasAudioSource != null && !gasAudioSource.isPlaying)
            {
                gasAudioSource.Play();
            }
        }
    }

    public void StopParticles()
    {
        if (targetParticleSystem != null)
        {
            targetParticleSystem.Stop();

            if (reportButton != null)
            {
                reportButton.SetActive(false);
            }

            if (gasAudioSource != null)
            {
                gasAudioSource.Stop();
            }
        }
    }
}