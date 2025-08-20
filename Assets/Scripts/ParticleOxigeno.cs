using UnityEngine;

public class ParticleOxigeno : MonoBehaviour
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
    public float pressureLimit = 65f; // Límite de presión en atm

    [Header("Configuración de Velocidad con Fuego")]
    public float speedIncreaseRate = 0.04f; // Incremento de velocidad cada intervalo
    public float speedInterval = 1f;        // Intervalo en segundos para cambiar velocidad

    // Propiedades solo para obtener los datos
    public float CurrentPressure { get; private set; }
    public float CurrentVolume { get; private set; }
    public float CurrentTemperature { get; private set; }

    private float speedTimer = 0f;
    private ParticleSystem.MainModule particleMain;
    private float initialSpeed; // Para guardar la velocidad inicial

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

            // CONTROLAR VELOCIDAD BASADO EN EL FUEGO
            ControlParticleSpeed();
        }

        // Verificar si la presión supera el límite y detener partículas
        CheckPressureLimit();
    }

    private void ControlParticleSpeed()
    {
        if (targetParticleSystem != null && targetParticleSystem.isPlaying)
        {
            speedTimer += Time.deltaTime;

            if (speedTimer >= speedInterval)
            {
                if (IsFireActive())
                {
                    // AUMENTAR velocidad cuando el fuego está activo
                    IncreaseParticleSpeed();
                }
                else
                {
                    // DISMINUIR velocidad cuando el fuego está apagado
                    DecreaseParticleSpeed();
                }
                speedTimer = 0f;
            }
        }
        else
        {
            speedTimer = 0f; // Resetear timer si las partículas no están activas
        }
    }

    private void IncreaseParticleSpeed()
    {
        float currentSpeed = particleMain.startSpeed.constant;
        particleMain.startSpeed = currentSpeed + speedIncreaseRate;
        Debug.Log($"🔥 Velocidad aumentada: {particleMain.startSpeed.constant}");
    }

    private void DecreaseParticleSpeed()
    {
        float currentSpeed = particleMain.startSpeed.constant;
        float newSpeed = currentSpeed - speedIncreaseRate;

        // No permitir velocidad negativa y volver a la velocidad inicial como mínimo
        if (newSpeed < initialSpeed)
        {
            newSpeed = initialSpeed;
        }

        particleMain.startSpeed = newSpeed;
        Debug.Log($"❄️ Velocidad disminuida: {particleMain.startSpeed.constant}");
    }

    private bool IsFireActive()
    {
        // Verificar si el fire controller existe y tiene fuego activo
        return temperatureSource != null && temperatureSource.IsFireActive;
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

        // Configurar el sistema de partículas
        if (targetParticleSystem != null)
        {
            particleMain = targetParticleSystem.main;
            initialSpeed = particleMain.startSpeed.constant; // Guardar velocidad inicial
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

            // Resetear velocidad a la inicial cada vez que se inician
            if (particleMain.startSpeed.mode == ParticleSystemCurveMode.Constant)
            {
                particleMain.startSpeed = initialSpeed;
            }

            targetParticleSystem.Play();
            speedTimer = 0f; // Resetear timer

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

            // Resetear velocidad a la inicial cuando se detienen completamente
            if (particleMain.startSpeed.mode == ParticleSystemCurveMode.Constant)
            {
                particleMain.startSpeed = initialSpeed;
            }

            speedTimer = 0f; // Resetear timer

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