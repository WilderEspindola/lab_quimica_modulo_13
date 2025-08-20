using UnityEngine;
using TMPro;

public class ParticleControllerFire : MonoBehaviour
{
    [Header("Control de Fuego")]
    public ParticleSystem targetParticleSystemFire;

    [Header("Configuración de Temperatura")]
    public TextMeshProUGUI temperatureDisplay; // Asignar el texto "X (°K)"
    public float minTemperature = 298f; // 298K = 25°C
    public float maxTemperature = 2500f; // 800K máximo
    public float heatingRate = 30f;     // Grados K por segundo al calentar
    public float coolingRate = 20f;     // Grados K por segundo al enfriar

    [Header("Eventos de Cambio de Temperatura")]
    public System.Action<float> OnTemperatureChanged; // Temperatura en Kelvin

    [Header("Tablas de Datos por Gas")]
    public GameObject tablaHidrogeno;    // Arrastrar UI de hidrógeno
    public GameObject tablaOxigeno;      // Arrastrar UI de oxígeno  
    public GameObject tablaCO2;          // Arrastrar UI de CO2

    [Header("Referencias a Controladores de Gas")]
    public ParticleHidrogeno hidrogenoController;
    public ParticleOxigeno oxigenoController;
    public ParticleDioxiodoCarbono co2Controller;
  

    [Header("Efectos de Sonido")]
    public AudioClip fireSound;          // Sonido del fuego (arrastrar el clip desde el Inspector)
    public AudioSource fireAudioSource;  // AudioSource asignado al mismo objeto
    [Range(0, 1)] public float fireVolume = 0.5f; // Volumen ajustable

    private float currentKelvin;
    private bool isHeating = false;

    void Start()
    {
        currentKelvin = minTemperature;
        UpdateTemperatureDisplay();
        OcultarTodasLasTablas(); // Asegurar que todas estén ocultas al inicio

        if (fireAudioSource != null && fireSound != null)
        {
            fireAudioSource.clip = fireSound;
            fireAudioSource.loop = true;
            fireAudioSource.volume = fireVolume;
        }
    }

    void Update()
    {
        if (isHeating)
        {
            currentKelvin = Mathf.Min(currentKelvin + heatingRate * Time.deltaTime, maxTemperature);
        }
        else
        {
            currentKelvin = Mathf.Max(currentKelvin - coolingRate * Time.deltaTime, minTemperature);
        }

        UpdateTemperatureDisplay();
        ActualizarTablaSegunGasActivo(); // Actualizar tabla visible según gas
    }

    public void StartParticles()
    {
        if (targetParticleSystemFire != null)
        {
            targetParticleSystemFire.Play();
        }

        if (fireAudioSource != null && !fireAudioSource.isPlaying)
        {
            fireAudioSource.Play();
        }

        isHeating = true;
        MostrarTablaGasActivo(); // Mostrar tabla cuando se enciende el fuego
    }

    public void StopParticles()
    {
        if (targetParticleSystemFire != null)
        {
            targetParticleSystemFire.Stop();
        }

        if (fireAudioSource != null)
        {
            fireAudioSource.Stop();
        }

        isHeating = false;
        OcultarTodasLasTablas(); // Ocultar todas las tablas al apagar fuego
    }

    // Modifica UpdateTemperatureDisplay para notificar cambios
    private void UpdateTemperatureDisplay()
    {
        if (temperatureDisplay != null)
        {
            temperatureDisplay.text = $"{Mathf.RoundToInt(currentKelvin)} (°K)";
            float tempRatio = Mathf.InverseLerp(minTemperature, maxTemperature, currentKelvin);
            temperatureDisplay.color = Color.Lerp(Color.white, Color.red, tempRatio);
            OnTemperatureChanged?.Invoke(currentKelvin);
        }
    }
    private void OcultarTodasLasTablas()
    {
        if (tablaHidrogeno != null) tablaHidrogeno.SetActive(false);
        if (tablaOxigeno != null) tablaOxigeno.SetActive(false);
        if (tablaCO2 != null) tablaCO2.SetActive(false);
    }

    private void MostrarTablaGasActivo()
    {
        OcultarTodasLasTablas(); // Primero ocultar todas

        // Determinar qué gas está activo y mostrar su tabla
        if (hidrogenoController != null && hidrogenoController.targetParticleSystem.isPlaying)
        {
            if (tablaHidrogeno != null) tablaHidrogeno.SetActive(true);
        }
        else if (oxigenoController != null && oxigenoController.targetParticleSystem.isPlaying)
        {
            if (tablaOxigeno != null) tablaOxigeno.SetActive(true);
        }
        else if (co2Controller != null && co2Controller.targetParticleSystem.isPlaying)
        {
            if (tablaCO2 != null) tablaCO2.SetActive(true);
        }
    }

    private void ActualizarTablaSegunGasActivo()
    {
        // Solo actualizar si el fuego está encendido
        if (isHeating)
        {
            MostrarTablaGasActivo();
        }
    }

    public float GetCurrentTemperature()
    {
        return currentKelvin;
    }
    // Agrega esta propiedad pública
    // En ParticleControllerFire, agrega esta propiedad pública:
    public bool IsFireActive
    {
        get { return isHeating; }
        private set { isHeating = value; }
    }
}