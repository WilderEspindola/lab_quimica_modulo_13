using UnityEngine;

public class ParticleHidrogeno : MonoBehaviour
{
    [Header("Configuraci�n de Part�culas")]
    public ParticleSystem targetParticleSystem;

    [Header("Referencia UI")]
    public GameObject reportButton;

    [Header("Configuraci�n de Sonido")]
    public AudioClip gasReleaseSound;
    public AudioSource gasAudioSource;
    [Range(0, 1)] public float gasVolume = 0.7f;

    [Header("Fuentes de Datos")]
    public DualPartButton pressureVolumeSource;
    public ParticleControllerFire temperatureSource;

    [Header("L�mite de Presi�n")]
    public float pressureLimit = 85f; // L�mite de presi�n en atm

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

        // Verificar si la presi�n supera el l�mite y detener part�culas
        CheckPressureLimit();
    }

    private void CheckPressureLimit()
    {
        if (targetParticleSystem != null && targetParticleSystem.isPlaying)
        {
            if (CurrentPressure > pressureLimit)
            {
                Debug.Log($"Presi�n demasiado alta ({CurrentPressure:0.00} atm > {pressureLimit} atm). Deteniendo part�culas.");
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
            // Verificar presi�n antes de iniciar
            if (CurrentPressure > pressureLimit)
            {
                Debug.LogWarning($"No se pueden iniciar part�culas. Presi�n demasiado alta: {CurrentPressure:0.00} atm");
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