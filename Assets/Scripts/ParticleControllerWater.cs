using UnityEngine;

public class ParticleControllerWater : MonoBehaviour
{
    [Header("Configuración de Partículas")]
    public ParticleSystem targetParticleSystem; // Nombre más específico

    [Header("Configuración de Sonido")]
    public AudioClip waterSound;
    public AudioSource waterAudioSource;
    [Range(0, 1)] public float soundVolume = 0.7f;

    private void Start()
    {
        // Configuración inicial del audio
        if (waterAudioSource != null && waterSound != null)
        {
            waterAudioSource.clip = waterSound;
            waterAudioSource.loop = true;
            waterAudioSource.volume = soundVolume;
        }
    }

    public void StartParticles()
    {
        if (targetParticleSystem != null)
        {
            targetParticleSystem.Play();

            // Iniciar sonido del agua
            if (waterAudioSource != null && !waterAudioSource.isPlaying)
            {
                waterAudioSource.Play();
            }
        }
    }

    public void StopParticles()
    {
        if (targetParticleSystem != null)
        {
            targetParticleSystem.Stop();

            // Detener sonido del agua
            if (waterAudioSource != null)
            {
                waterAudioSource.Stop();
            }
        }
    }
}