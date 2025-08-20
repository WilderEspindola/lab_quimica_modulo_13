using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using TMPro;
using System.Collections;

[RequireComponent(typeof(XRSimpleInteractable))]
public class DualPartButton : MonoBehaviour
{
    [Header("Configuración del Botón")]
    public Transform[] movingParts;
    public float pressedZPosition = -0.00012f;
    public float returnSpeed = 25f;

    [Header("Feedback Háptico")]
    public bool hapticFeedback = true;
    [Range(0, 1)] public float hapticIntensity = 0.3f;
    public float hapticDuration = 0.1f;

    [Header("Control del Pistón")]
    public Transform piston;
    public float pistonSpeed = 0.5f;
    public float minPistonHeight = -22.21f;
    public float maxPistonHeight = 0.05f;
    public bool isOnButton = true;

    [Header("Medidor de Volumen")]
    public TextMeshProUGUI volumeText;
    public RectTransform volumeUI;
    public float maxVolume = 5f;
    public float minVolume = 0f;
    public Vector3 uiOffset = new Vector3(0, 0.1f, 0);

    [Header("Medidor de Presión")]
    public TextMeshProUGUI pressureText;
    public float minPressure = 1f; // 1 atm
    public float maxPressure = 110f; // 15 atm
    public Color normalPressureColor = Color.white;
    public Color warningPressureColor = new Color(1f, 0.6f, 0f); // Naranja
    public Color dangerPressureColor = Color.red;
    [Range(0, 1)] public float warningThreshold = 0.7f; // 70% de presión máxima
    [Range(0, 1)] public float dangerThreshold = 0.9f; // 90% de presión máxima
    [Header("Eventos de Cambio de Valores")]
    public System.Action<float, float> OnPVValuesChanged; // Presión, Volumen

    private Vector3[] originalPositions;
    private bool isPressed = false;
    private Vector3 initialUIPosition;
    private float initialPistonY;
    private float currentPressure;

    [Header("Efectos de Sonido")]
    public AudioClip hydraulicSound;  // Único sonido para ambos movimientos
    public AudioSource pistonAudioSource;
    [Range(0, 1)] public float soundVolume = 0.4f;

    private bool wasMovingLastFrame = false;
    [Header("Sistema de Explosión")]
    public GameObject cylinder; // Asigna el cilindro físico aquí
    public AudioClip explosionSound; // Sonido de explosión
    public float explosionThreshold = 100f; // Umbral de presión para explosión
    public float resetDelay = 5f; // Tiempo en segundos para resetear
    private bool hasExploded = false; // Control para una sola explosión

    [Header("Sistema de Gases")]
    public ParticleOxigeno oxigenoController;
    public ParticleHidrogeno hidrogenoController;
    public ParticleDioxiodoCarbono co2Controller;

    [Header("Límites de Presión por Gas")]
    public float limiteVacio = 100f;
    public float limiteOxigeno = 65f;
    public float limiteHidrogeno = 85f;
    public float limiteCO2 = 73f;

    private string gasActivo = "VACIO";
    void Start()
    {
        // 1. Inicializar posiciones de las partes móviles
        originalPositions = new Vector3[movingParts.Length];
        for (int i = 0; i < movingParts.Length; i++)
        {
            originalPositions[i] = movingParts[i].localPosition;
        }

        // 2. Configurar eventos de interacción XR
        XRSimpleInteractable interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnButtonPressed);
        interactable.selectExited.AddListener(OnButtonReleased);

        // 3. Forzar posición inicial exacta del pistón
        if (piston != null)
        {
            // Asegurar posición Y inicial exacta
            Vector3 pistonPos = piston.localPosition;
            pistonPos.y = maxPistonHeight; // Posición completamente arriba
            piston.localPosition = pistonPos;

            initialPistonY = piston.localPosition.y;

            if (volumeUI != null)
            {
                initialUIPosition = volumeUI.localPosition;
                SyncVolumeUIWithPiston();
            }
        }

        // 4. Forzar valores iniciales exactos
        ForceInitialValues();

        if (pistonAudioSource != null && hydraulicSound != null)
        {
            pistonAudioSource.clip = hydraulicSound;
            pistonAudioSource.loop = true;
            pistonAudioSource.Play();
            pistonAudioSource.Pause(); // Precarga sin reproducir
        }
    }

    private void ForceInitialValues()
    {
        // Asegurar volumen exacto (5.000) CON ENCABEZADO
        if (volumeText != null)
        {
            volumeText.text = "VOLUMEN ACTUAL DEL CILINDRO\n" +
                              "--- 5.000 m³ ---";
        }

        // Asegurar presión exacta (1.00)
        if (pressureText != null)
        {
            pressureText.text = "   1.00 (atm)";
            pressureText.color = normalPressureColor;
        }

        // Sincronizar valores internos
        if (piston != null)
        {
            currentPressure = minPressure;
        }
    }
    private bool firstFrame = true;
    void Update()
    {
        if (firstFrame)
        {
            firstFrame = false;
            return;
        }
        // Movimiento del botón físico
        AnimateButtonParts();

        // Control del pistón y actualización de UI
        if (isPressed && piston != null)
        {
            ControlPistonMovement();
            UpdateVolumeUI();
            UpdatePressureUI();
        }
        if (pistonAudioSource != null)
        {
            pistonAudioSource.volume = soundVolume; // Fuerza el valor actual
        }
        DetectarGasActivo();

        // Detectar explosión por presión (modificado)
        if (!hasExploded && currentPressure >= ObtenerLimiteActual())
        {
            TriggerExplosion();
        }
    }
    private void DetectarGasActivo()
    {
        // Verificar qué gas está activo
        if (oxigenoController != null && oxigenoController.targetParticleSystem.isPlaying)
        {
            gasActivo = "OXIGENO";
        }
        else if (hidrogenoController != null && hidrogenoController.targetParticleSystem.isPlaying)
        {
            gasActivo = "HIDROGENO";
        }
        else if (co2Controller != null && co2Controller.targetParticleSystem.isPlaying)
        {
            gasActivo = "CO2";
        }
        else
        {
            gasActivo = "VACIO";
        }
    }
    public float ObtenerLimiteActual()
    {
        switch (gasActivo)
        {
            case "OXIGENO": return limiteOxigeno;
            case "HIDROGENO": return limiteHidrogeno;
            case "CO2": return limiteCO2;
            default: return limiteVacio;
        }
    }
    private void TriggerExplosion()
    {
        hasExploded = true;

        // Desactivar el cilindro
        if (cylinder != null)
        {
            cylinder.SetActive(false);
        }
        // Desactivar el pistón (usando la referencia existente)
        if (piston != null)
        {
            piston.gameObject.SetActive(false);
        }

        // Reproducir sonido de explosión
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // Opcional: Detener todo movimiento
        isPressed = false;
        if (pistonAudioSource != null)
        {
            pistonAudioSource.Stop();
        }
        // Iniciar el reseteo después del delay
        StartCoroutine(ResetSystemAfterDelay());
        Debug.Log("¡EXPLOSIÓN! Presión crítica alcanzada: " + currentPressure + " atm");
    }
    private IEnumerator ResetSystemAfterDelay()
    {
        // Esperar el tiempo configurado
        yield return new WaitForSeconds(resetDelay);

        // Restablecer todo el sistema
        ResetSystem();
    }
    public void ResetSystem()
    {
        // Reactivar el cilindro
        if (cylinder != null)
        {
            cylinder.SetActive(true);
        }

        // Reactivar y resetear la posición del pistón
        if (piston != null)
        {
            piston.gameObject.SetActive(true);
            Vector3 pistonPos = piston.localPosition;
            pistonPos.y = maxPistonHeight; // Posición completamente arriba
            piston.localPosition = pistonPos;
        }

        // Resetear los valores de UI
        ForceInitialValues();

        // Resetear la bandera de explosión
        hasExploded = false;

        // Sincronizar UI
        SyncVolumeUIWithPiston();

        Debug.Log("Sistema reiniciado después de explosión");
    }

    private void AnimateButtonParts()
    {
        for (int i = 0; i < movingParts.Length; i++)
        {
            Vector3 targetPos = isPressed ?
                new Vector3(originalPositions[i].x, originalPositions[i].y, pressedZPosition) :
                originalPositions[i];

            movingParts[i].localPosition = Vector3.Lerp(
                movingParts[i].localPosition,
                targetPos,
                returnSpeed * Time.deltaTime
            );
        }
    }

    private void OnButtonPressed(SelectEnterEventArgs args)
    {
        isPressed = true;

        // Mover partes al instante al presionar
        for (int i = 0; i < movingParts.Length; i++)
        {
            Vector3 newPos = movingParts[i].localPosition;
            newPos.z = pressedZPosition;
            movingParts[i].localPosition = newPos;
        }

        // Feedback háptico
        if (hapticFeedback && args.interactorObject is XRBaseInputInteractor inputInteractor)
        {
            if (inputInteractor.TryGetComponent(out XRController controller))
            {
                controller.SendHapticImpulse(hapticIntensity, hapticDuration);
            }
        }
    }

    private void OnButtonReleased(SelectExitEventArgs args)
    {
        isPressed = false;

        // Detener sonido inmediatamente al soltar
        if (pistonAudioSource != null)
        {
            pistonAudioSource.Stop();
        }
    }

    private void ControlPistonMovement()
    {
        float direction = isOnButton ? -1f : 1f;
        Vector3 newPos = piston.localPosition;
        bool isMoving = !Mathf.Approximately(newPos.y, Mathf.Clamp(
            newPos.y + (direction * pistonSpeed * Time.deltaTime),
            minPistonHeight,
            maxPistonHeight
        ));

        newPos.y = Mathf.Clamp(
            newPos.y + (direction * pistonSpeed * Time.deltaTime),
            minPistonHeight,
            maxPistonHeight
        );

        // Control de audio preciso
        if (pistonAudioSource != null && hydraulicSound != null)
        {
            if (isPressed && isMoving)
            {
                if (!pistonAudioSource.isPlaying)
                {
                    pistonAudioSource.clip = hydraulicSound;
                    pistonAudioSource.loop = true;
                    pistonAudioSource.volume = soundVolume;
                    pistonAudioSource.Play();
                }
            }
            else
            {
                pistonAudioSource.Stop();
            }
        }

        piston.localPosition = newPos;
        SyncVolumeUIWithPiston();
        wasMovingLastFrame = isMoving;
    }

    private void SyncVolumeUIWithPiston()
    {
        if (volumeUI != null)
        {
            Vector3 volumePos = volumeUI.localPosition;
            volumePos.y = initialUIPosition.y + (piston.localPosition.y * piston.parent.localScale.y);
            volumeUI.localPosition = volumePos;
        }
    }
    private void UpdateVolumeUI()
    {
        if (volumeText == null || piston == null) return;

        // Caso especial: pistón en posición máxima (volumen máximo)
        if (Mathf.Approximately(piston.localPosition.y, maxPistonHeight))
        {
            volumeText.text = "VOLUMEN ACTUAL DEL CILINDRO\n" +
                              "--- 5.000 m³ ---";
            return;
        }

        float normalizedPosition = Mathf.InverseLerp(minPistonHeight, maxPistonHeight, piston.localPosition.y);
        float currentVolume = Mathf.Lerp(minVolume, maxVolume, normalizedPosition);

        // Nuevo formato con encabezado y flecha
        volumeText.text = "VOLUMEN ACTUAL DEL CILINDRO\n" +
                          $"--- {currentVolume.ToString("0.000")} m³ ---";
    }

    // Modifica el método UpdatePressureUI para que notifique cambios
    private void UpdatePressureUI()
    {
        if (pressureText == null || piston == null) return;

        float normalizedPosition = Mathf.InverseLerp(maxPistonHeight, minPistonHeight, piston.localPosition.y);
        currentPressure = Mathf.Lerp(minPressure, maxPressure, normalizedPosition);

        pressureText.text = $"   {currentPressure.ToString("0.00")} (atm)";

        if (normalizedPosition >= dangerThreshold)
            pressureText.color = dangerPressureColor;
        else if (normalizedPosition >= warningThreshold)
            pressureText.color = warningPressureColor;
        else
            pressureText.color = normalPressureColor;

        // Notificar el cambio de valores
        OnPVValuesChanged?.Invoke(currentPressure, GetCurrentVolume());
    }

    // Asegúrate que este método sea público
    public float GetCurrentVolume()
    {
        float normalizedPosition = Mathf.InverseLerp(minPistonHeight, maxPistonHeight, piston.localPosition.y);
        return Mathf.Lerp(minVolume, maxVolume, normalizedPosition);
    }

    public float GetCurrentPressure()
    {
        return currentPressure;
    }
}